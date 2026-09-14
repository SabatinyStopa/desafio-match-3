using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.ScriptableObjects;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    public class BoardView : MonoBehaviour
    {
        public event Action<int, int> TileClicked;

        [SerializeField]
        private Transform _boardContainer;

        [SerializeField]
        private TilePrefabRepository _tilePrefabRepository;

        [SerializeField]
        private SpriteRenderer _tilePrefab;

        private GameObject[][] _tiles;
        private TileSpotView[][] _tileSpots;

        public void CreateBoard(List<List<Tile>> board)
        {
            _tiles = new GameObject[board.Count][];
            _tileSpots = new TileSpotView[board.Count][];

            for (int y = 0; y < board.Count; y++)
            {
                _tiles[y] = new GameObject[board[0].Count];
                _tileSpots[y] = new TileSpotView[board[0].Count];

                for (int x = 0; x < board[0].Count; x++)
                {
                    GameObject spot = new($"Tile Spot ({x}-{y})", typeof(TileSpotView));

                    spot.transform.position = new Vector3(x * 1.3f, y * 1.3f, 0);
                    spot.transform.SetParent(_boardContainer);

                    if (spot.TryGetComponent(out TileSpotView tileSpot))
                    {
                        tileSpot.SetPosition(x, y);
                        tileSpot.Clicked += TileSpot_Clicked;

                        _tileSpots[y][x] = tileSpot;

                        int tileTypeIndex = board[y][x].Type;

                        if (tileTypeIndex < 0)
                        {
                            Debug.LogError(
                                $"[Board View] Tile type should not be under zero, tileTypeIndex is {tileTypeIndex} x:{x} and y:{y}"
                            );
                        }
                        else
                        {
                            SpriteRenderer tile = Instantiate(_tilePrefab);

                            tile.transform.localScale = Vector3.zero;
                            tile.color = _tilePrefabRepository.TileTypes[tileTypeIndex];
                            tileSpot.SetTile(tile.gameObject);
                            _tiles[y][x] = tile.gameObject;
                        }
                    }
                }
            }
        }

        public Tween CreateTile(List<AddedTileInfo> addedTiles)
        {
            Sequence sequence = DOTween.Sequence();
            float animationDuration = 0.5f;

            for (int i = 0; i < addedTiles.Count; i++)
            {
                AddedTileInfo addedTileInfo = addedTiles[i];
                Vector2Int position = addedTileInfo.Position;
                TileSpotView tileSpot = _tileSpots[position.y][position.x];
                SpriteRenderer tile = Instantiate(_tilePrefab);

                tile.color = _tilePrefabRepository.TileTypes[addedTiles[i].Type];
                tileSpot.SetTile(tile.gameObject);
                _tiles[position.y][position.x] = tile.gameObject;
                tile.transform.localScale = Vector2.zero;
                sequence.Join(tile.transform.DOScale(1.0f, animationDuration));
            }

            return sequence;
        }

        public Tween DestroyTiles(List<Vector2Int> matchedPosition)
        {
            for (int i = 0; i < matchedPosition.Count; i++)
            {
                Vector2Int position = matchedPosition[i];
                Destroy(_tiles[position.y][position.x]);
                _tiles[position.y][position.x] = null;
            }

            return DOVirtual.DelayedCall(0.2f, () => { });
        }

        public Tween MoveTiles(List<MovedTileInfo> movedTiles)
        {
            GameObject[][] tiles = new GameObject[_tiles.Length][];
            for (int y = 0; y < _tiles.Length; y++)
            {
                tiles[y] = new GameObject[_tiles[y].Length];
                for (int x = 0; x < _tiles[y].Length; x++)
                {
                    tiles[y][x] = _tiles[y][x];
                }
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < movedTiles.Count; i++)
            {
                MovedTileInfo movedTileInfo = movedTiles[i];

                Vector2Int from = movedTileInfo.From;
                Vector2Int to = movedTileInfo.To;

                sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x]));

                tiles[to.y][to.x] = _tiles[from.y][from.x];
            }

            _tiles = tiles;

            return sequence;
        }

        public Tween SwapTiles(int fromX, int fromY, int toX, int toY)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX]));
            sequence.Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX]));

            (_tiles[toY][toX], _tiles[fromY][fromX]) = (_tiles[fromY][fromX], _tiles[toY][toX]);

            return sequence;
        }


        /// <summary>
        /// Faz o tabuleiro aparecer em efeito cascata
        /// </summary>
        /// <returns></returns>
        public Tween MakeAllTilesPopUp()
        {
            Sequence sequence = DOTween.Sequence();
            float intervalDuration = 0.1f;
            float animationDuration = 0.5f;

            if (_tiles == null || _tiles.Length == 0)
            {
                return sequence;
            }

            int numRows = _tiles.Length;
            int numCols = _tiles[0].Length;

            bool[,] processed = new bool[numRows, numCols];

            float currentTime = 0f;
            int maxSteps = Mathf.Max(numRows, numCols);

            for (int step = 0; step < maxSteps; step++)
            {
                int targetCol = numCols - 1 - step;
                int targetRow = step;
                bool addedAnyInThisStep = false;

                if (targetCol >= 0)
                {
                    for (int r = 0; r < numRows; r++)
                    {
                        if (r < numRows && targetCol < _tiles[r].Length && !processed[r, targetCol])
                        {
                            AnimateTile(
                                _tiles[r][targetCol],
                                sequence,
                                currentTime,
                                animationDuration
                            );
                            processed[r, targetCol] = true;
                            addedAnyInThisStep = true;
                        }
                    }
                }

                if (targetRow < numRows && _tiles[targetRow] != null)
                {
                    for (int c = 0; c < _tiles[targetRow].Length; c++)
                    {
                        if (!processed[targetRow, c])
                        {
                            AnimateTile(
                                _tiles[targetRow][c],
                                sequence,
                                currentTime,
                                animationDuration
                            );
                            processed[targetRow, c] = true;
                            addedAnyInThisStep = true;
                        }
                    }
                }

                if (addedAnyInThisStep)
                {
                    currentTime += intervalDuration;
                }
            }

            return sequence;
        }

        private void AnimateTile(GameObject tile, Sequence sequence, float atTime, float duration)
        {
            if (tile == null)
            {
                return;
            }

            tile.transform.localScale = Vector3.zero;

            Sequence tileSequence = DOTween.Sequence();

            tileSequence.Append(tile.transform.DOScale(Vector3.one, duration * 0.7f));

            tileSequence.Append(
                tile.transform.DOPunchScale(
                    new Vector3(0.2f, 0.2f, 0.2f),
                    duration * 0.3f,
                    vibrato: 1,
                    elasticity: 0.5f
                )
            );

            sequence.Insert(atTime, tileSequence);
        }

        #region Events
        private void TileSpot_Clicked(int x, int y)
        {
            TileClicked(x, y);
        }
        #endregion
    }
}
