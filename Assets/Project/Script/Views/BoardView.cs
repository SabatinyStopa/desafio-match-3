using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Controllers;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.ScriptableObjects;
using Gazeus.DesafioMatch3.Utilities;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    public class BoardView : MonoBehaviour
    {
        public event Action<int, int> TileClicked;

        #region Constants
        private const float TILE_SPACING = 1.3f;
        private const float TILE_APPEAR_INTERVAL_DURATION = 0.1f;
        private const float ANIMATION_DURATION = 0.5f;
        private const float SCALE_PHASE_RATIO = 0.7f;
        private const float PUNCH_PHASE_RATIO = 0.3f;
        private static readonly Vector3 PUNCH_STRENGTH = new(0.2f, 0.2f, 0.2f);
        #endregion

        [SerializeField]
        private Transform _boardContainer;

        [SerializeField]
        private TilePrefabRepository _tilePrefabRepository;

        [SerializeField]
        private TileView _tilePrefab;

        [SerializeField]
        private ExplosionEffectView[] _explosionsPrefabs;

        private ObjectPool<TileView> _tilePool;
        private ObjectPool<ExplosionEffectView>[] _explosionPools;
        private TileView[,] _tiles;
        private TileSpotView[,] _tileSpots;

        #region Unity
        private void Awake()
        {
            _tilePool = new ObjectPool<TileView>(_tilePrefab, 100, _boardContainer);
            _explosionPools = new ObjectPool<ExplosionEffectView>[_explosionsPrefabs.Length];

            for (int i = 0; i < _explosionPools.Length; i++)
            {
                _explosionPools[i] = new ObjectPool<ExplosionEffectView>(
                    _explosionsPrefabs[i],
                    10,
                    transform
                );
            }
        }
        #endregion

        public void CreateBoard(List<List<Tile>> board)
        {
            int rowCount = board.Count;
            int colCount = board[0].Count;

            _tiles = new TileView[rowCount, colCount];
            _tileSpots = new TileSpotView[rowCount, colCount];

            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < colCount; x++)
                {
                    CreateTileSpotAndTile(board, x, y);
                }
            }
        }

        public Tween CreateTile(List<AddedTileInfo> addedTiles)
        {
            Sequence sequence = DOTween.Sequence();

            foreach (var addedTile in addedTiles)
            {
                Vector2Int pos = addedTile.Position;
                TileView tile = SetupTile(addedTile.Type);

                _tileSpots[pos.y, pos.x].SetTile(tile.gameObject);
                _tiles[pos.y, pos.x] = tile;

                sequence.Join(tile.transform.DOScale(Vector3.one, ANIMATION_DURATION));
            }

            return sequence;
        }

        public Tween SwapTiles(int fromX, int fromY, int toX, int toY)
        {
            TileView tileFrom = GetTile(fromX, fromY);
            TileView tileTo = GetTile(toX, toY);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(GetTileSpot(fromX, fromY).AnimateSetTile(tileTo.gameObject));
            sequence.Join(GetTileSpot(toX, toY).AnimateSetTile(tileFrom.gameObject));
            sequence.InsertCallback(0f, () => SoundController.Play("SwapTile"));

            _tiles[toY, toX] = tileFrom;
            _tiles[fromY, fromX] = tileTo;

            return sequence;
        }

        public Tween MakeAllTilesPopUp()
        {
            Sequence sequence = DOTween.Sequence();

            if (IsBoardInvalid())
            {
                return sequence;
            }

            int numRows = _tiles.GetLength(0);
            int numCols = _tiles.GetLength(1);
            bool[,] processed = new bool[numRows, numCols];

            float currentTime = 0f;
            int maxSteps = Mathf.Max(numRows, numCols);

            for (int step = 0; step < maxSteps; step++)
            {
                if (EnqueueStepAnimations(step, numRows, numCols, processed, sequence, currentTime))
                {
                    currentTime += TILE_APPEAR_INTERVAL_DURATION;
                }
            }

            sequence.AppendCallback(() => SoundController.Play("SpawnTile"));

            return sequence;
        }

        public void DestroyBoard()
        {
            if (_tiles == null)
            {
                return;
            }

            for (int y = 0; y < _tiles.GetLength(1); y++)
            {
                for (int x = 0; x < _tiles.GetLength(0); x++)
                {
                    if (_tiles[x, y] != null)
                    {
                        _tilePool.Release(_tiles[x, y]);
                    }
                }
            }

            _tiles = null;
        }

        public Tween AnimateMatchPosition(List<Vector2Int> matchedPositions)
        {
            Sequence sequence = DOTween.Sequence();

            foreach (var pos in matchedPositions)
            {
                TileView tile = GetTile(pos.x, pos.y);
                if (tile == null)
                {
                    continue;
                }

                tile.transform.DOKill();
                sequence.Join(tile.transform.DOShakePosition(0.25f, strength: 0.12f, vibrato: 25));
                sequence.Join(tile.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
            }

            return sequence;
        }

        public void DestroyTiles(List<Vector2Int> matchedPositions)
        {
            foreach (var pos in matchedPositions)
            {
                TileView tile = _tiles[pos.y, pos.x];
                if (tile == null)
                {
                    continue;
                }

                int index = tile.GetCurrentType();
                ExplosionEffectView explosionEffect = _explosionPools[index]
                    .Get(tile.transform.position);

                explosionEffect.Play(() =>
                {
                    _explosionPools[index].Release(explosionEffect);
                });

                tile.transform.DOKill();
                _tilePool.Release(tile);
                _tiles[pos.y, pos.x] = null;
            }
        }

        public Tween MoveTiles(List<MovedTileInfo> movedTiles)
        {
            Sequence sequence = DOTween.Sequence();

            foreach (var movedTile in movedTiles)
            {
                Vector2Int from = movedTile.From;
                Vector2Int to = movedTile.To;

                TileView tileToMove = _tiles[from.y, from.x];
                if (tileToMove == null)
                {
                    continue;
                }

                sequence.Join(GetTileSpot(to.x, to.y).AnimateSetTile(tileToMove.gameObject));
                _tiles[to.y, to.x] = tileToMove;
                _tiles[from.y, from.x] = null;
            }

            return sequence;
        }

        public TileSpotView GetTileSpot(int x, int y) => _tileSpots[y, x];

        public TileView GetTile(int x, int y) => _tiles[y, x];

        private TileView SetupTile(int typeIndex)
        {
            return _tilePool.Get(tile =>
            {
                tile.transform.localScale = Vector3.zero;
                tile.SetImage(_tilePrefabRepository.TileTypes[typeIndex]);
                tile.SetType(typeIndex);
            });
        }

        private void CreateTileSpotAndTile(List<List<Tile>> board, int x, int y)
        {
            GameObject spotGo = new($"Tile Spot ({x}-{y})", typeof(TileSpotView));
            spotGo.transform.SetParent(_boardContainer);
            spotGo.transform.position = new Vector3(x * TILE_SPACING, y * TILE_SPACING, 0f);

            if (!spotGo.TryGetComponent(out TileSpotView tileSpot))
            {
                return;
            }

            tileSpot.SetPosition(x, y);
            tileSpot.Clicked += TileSpot_Clicked;
            _tileSpots[y, x] = tileSpot;

            int tileTypeIndex = board[y][x].Type;
            if (tileTypeIndex < 0)
            {
                Debug.LogError($"[BoardView] Invalid tile type {tileTypeIndex} at x:{x}, y:{y}");
                return;
            }

            TileView tile = SetupTile(tileTypeIndex);
            tileSpot.SetTile(tile.gameObject);
            _tiles[y, x] = tile;
        }

        private bool IsBoardInvalid() => _tiles == null || _tiles.GetLength(0) == 0;

        private bool EnqueueStepAnimations(
            int step,
            int numRows,
            int numCols,
            bool[,] processed,
            Sequence sequence,
            float currentTime
        )
        {
            int targetCol = numCols - 1 - step;
            int targetRow = step;

            bool columnAnimated =
                targetCol >= 0
                && EnqueueColumnAnimations(targetCol, numRows, processed, sequence, currentTime);
            bool rowAnimated =
                targetRow < numRows
                && EnqueueRowAnimations(targetRow, processed, sequence, currentTime);

            return columnAnimated || rowAnimated;
        }

        private bool EnqueueColumnAnimations(
            int col,
            int numRows,
            bool[,] processed,
            Sequence sequence,
            float currentTime
        )
        {
            bool hasAnimatedAny = false;

            for (int r = 0; r < numRows; r++)
            {
                if (processed[r, col] || _tiles[r, col] == null)
                {
                    continue;
                }

                AnimateTile(_tiles[r, col].gameObject, sequence, currentTime);
                processed[r, col] = true;
                hasAnimatedAny = true;
            }

            return hasAnimatedAny;
        }

        private bool EnqueueRowAnimations(
            int row,
            bool[,] processed,
            Sequence sequence,
            float currentTime
        )
        {
            bool hasAnimatedAny = false;
            int numCols = _tiles.GetLength(1);

            for (int c = 0; c < numCols; c++)
            {
                if (processed[row, c] || _tiles[row, c] == null)
                {
                    continue;
                }

                AnimateTile(_tiles[row, c].gameObject, sequence, currentTime);
                processed[row, c] = true;
                hasAnimatedAny = true;
            }

            return hasAnimatedAny;
        }

        private void AnimateTile(GameObject tile, Sequence mainSequence, float atTime)
        {
            if (tile == null)
            {
                return;
            }

            tile.transform.localScale = Vector3.zero;

            mainSequence.Insert(
                atTime,
                tile.transform.DOScale(Vector3.one, ANIMATION_DURATION * SCALE_PHASE_RATIO)
            );
            mainSequence.Insert(
                atTime + (ANIMATION_DURATION * SCALE_PHASE_RATIO),
                tile.transform.DOPunchScale(
                    PUNCH_STRENGTH,
                    ANIMATION_DURATION * PUNCH_PHASE_RATIO,
                    1,
                    0.5f
                )
            );
        }

        #region Events
        private void TileSpot_Clicked(int x, int y) => TileClicked?.Invoke(x, y);
        #endregion
    }
}
