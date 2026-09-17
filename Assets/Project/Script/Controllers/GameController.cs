using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.UI;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private BoardView _boardView;

        [SerializeField]
        private UIGame _gameUI;

        [SerializeField]
        private int _boardHeight = 10;

        [SerializeField]
        private int _boardWidth = 10;

        private int _targetScore = 1000;

        private int _maxMoves = 15;

        private int _currentMoves = 0;

        private int _currentLevel = 1;

        private GameService _gameService;

        private ScoreController _scoreController;

        private BuffController _buffController;

        private bool _isAnimating;
        private int _selectedX = -1;
        private int _selectedY = -1;

        #region Unity
        private void Awake()
        {
            DOTween.SetTweensCapacity(500, 50);

            _gameService = new GameService();
            _buffController = new BuffController();
            _scoreController = new ScoreController(_buffController);

            _boardView.TileClicked += OnTileClick;

            _gameUI.SubscribeEvents(_scoreController);
            _gameUI.SetupRestartButton(RestartGame);
        }

        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;

            _gameUI.UnsubscribeEvents(_scoreController);
        }

        private void Start()
        {
            _currentMoves = _maxMoves;
            _gameUI.SetTargetScore(_targetScore);
            _gameUI.SetCurrentMoves(_currentMoves);
            _gameUI.SetLevel(_currentLevel);

            List<List<Tile>> board = _gameService.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
            _boardView.MakeAllTilesPopUp().Play();
        }
        #endregion

        private void RestartGame()
        {
            DOTween.KillAll();
            _isAnimating = false;

            _currentLevel = 1;
            _targetScore = 100;

            _boardView.DestroyBoard();
            _gameUI.SetEnableRestartScreen(false);

            ResetSelection();
            _currentMoves = _maxMoves;

            _scoreController.ResetScore();

            _gameUI.SetTargetScore(_targetScore);
            _gameUI.SetCurrentMoves(_currentMoves);
            _gameUI.SetLevel(_currentLevel);

            List<List<Tile>> board = _gameService.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
            _boardView.MakeAllTilesPopUp().Play();
            SoundController.Play("Click");
        }

        private void AdvanceLevel()
        {
            DOTween.KillAll();
            _isAnimating = false;

            _currentLevel++;
            _targetScore += Mathf.RoundToInt(_targetScore * 0.5f);

            _boardView.DestroyBoard();

            ResetSelection();
            _currentMoves = _maxMoves;

            _scoreController.ResetScore();

            _gameUI.SetTargetScore(_targetScore);
            _gameUI.SetCurrentMoves(_currentMoves);
            _gameUI.SetLevel(_currentLevel);

            List<List<Tile>> board = _gameService.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
            _boardView.MakeAllTilesPopUp().Play();
        }

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating || _currentMoves <= 0)
            {
                return;
            }

            if (HasSelectedTile())
            {
                HandleSecondTileSelection(x, y);
            }
            else
            {
                SelectTile(x, y);
            }
        }

        private bool HasSelectedTile() => _selectedX > -1 && _selectedY > -1;

        private bool IsSameTile(int x, int y) => _selectedX == x && _selectedY == y;

        private bool IsAdjacentTile(int x, int y) =>
            Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) == 1;

        private void SelectTile(int x, int y)
        {
            _selectedX = x;
            _selectedY = y;
            _boardView.GetTile(x, y)?.Select();
        }

        private void UnselectCurrentTile()
        {
            if (HasSelectedTile())
            {
                _boardView.GetTile(_selectedX, _selectedY)?.UnSelect();
                ResetSelection();
            }
        }

        private void ResetSelection()
        {
            _selectedX = -1;
            _selectedY = -1;
        }

        private void HandleSecondTileSelection(int targetX, int targetY)
        {
            if (IsSameTile(targetX, targetY))
            {
                UnselectCurrentTile();
                return;
            }

            if (!IsAdjacentTile(targetX, targetY))
            {
                UnselectCurrentTile();
                SelectTile(targetX, targetY);
                return;
            }

            SoundController.Play("SelectTile");
            ExecuteMove(_selectedX, _selectedY, targetX, targetY);
        }

        private void ExecuteMove(int fromX, int fromY, int toX, int toY)
        {
            _isAnimating = true;

            _currentMoves--;
            _gameUI.SetCurrentMoves(_currentMoves);
            TileView selectedTile = _boardView.GetTile(fromX, fromY);
            selectedTile?.UnSelectImmediate();
            ResetSelection();

            _boardView.SwapTiles(fromX, fromY, toX, toY).onComplete += () =>
            {
                ExecuteMoveResult(fromX, fromY, toX, toY);
            };
        }

        private void ExecuteMoveResult(int fromX, int fromY, int toX, int toY)
        {
            List<BoardSequence> sequences = _gameService.SwapTile(fromX, fromY, toX, toY);

            if (sequences != null && sequences.Count > 0)
            {
                AnimateBoard(
                    sequences,
                    0,
                    () =>
                    {
                        _isAnimating = false;
                        TryToGameOver();
                    }
                );
            }
            else
            {
                _isAnimating = false;
                TryToGameOver();
            }
        }

        private void TryToGameOver()
        {
            if (_scoreController.GetCurrentScore() >= _targetScore)
            {
                _gameUI.OpenBuffSelection(_buffController, AdvanceLevel);
            }
            else if (_currentMoves <= 0)
            {
                _gameUI.SetEnableRestartScreen(true);
            }
        }

        private void AnimateBoard(List<BoardSequence> boardSequences, int index, Action onComplete)
        {
            if (boardSequences == null || index >= boardSequences.Count)
            {
                onComplete?.Invoke();
                return;
            }

            BoardSequence boardSequence = boardSequences[index];
            float appendInterval = 0.3f;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(_boardView.AnimateMatchPosition(boardSequence.MatchedPosition));

            int comboLevel = index + 1;
            _scoreController.RegisterMatch(
                boardSequence.MatchType,
                boardSequence.MatchCount,
                comboLevel,
                boardSequence.ResolveType
            );

            sequence.AppendCallback(() => _boardView.DestroyTiles(boardSequence.MatchedPosition));

            sequence.AppendCallback(() => SoundController.Play("Explosion"));

            sequence.AppendCallback(() => _boardView.MoveTiles(boardSequence.MovedTiles).Play());

            sequence.AppendInterval(appendInterval);

            sequence.AppendCallback(() => _boardView.CreateTile(boardSequence.AddedTiles).Play());

            sequence.AppendCallback(() => SoundController.Play("SpawnTile"));

            sequence.AppendInterval(appendInterval);

            index++;

            if (index < boardSequences.Count)
            {
                sequence.OnComplete(() => AnimateBoard(boardSequences, index, onComplete));
            }
            else
            {
                sequence.OnComplete(() => onComplete?.Invoke());
            }
        }
    }
}
