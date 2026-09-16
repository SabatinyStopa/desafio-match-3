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
        private UIScore _score;

        [SerializeField]
        private int _boardHeight = 10;

        [SerializeField]
        private int _boardWidth = 10;

        private GameService _gameService;
        private ScoreController _scoreController;
        private bool _isAnimating;
        private int _selectedX = -1;
        private int _selectedY = -1;

        #region Unity
        private void Awake()
        {
            _gameService = new GameService();
            _scoreController = new ScoreController();
            _boardView.TileClicked += OnTileClick;

            _scoreController.OnScoreUpdated += _score.UpdateScore;
        }

        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;
            _scoreController.OnScoreUpdated -= _score.UpdateScore;
        }

        private void Start()
        {
            List<List<Tile>> board = _gameService.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
            _boardView.MakeAllTilesPopUp().Play();
        }
        #endregion

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating)
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

            ExecuteMove(_selectedX, _selectedY, targetX, targetY);
        }

        private void ExecuteMove(int fromX, int fromY, int toX, int toY)
        {
            _isAnimating = true;

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
            if (_gameService.IsValidMovement(fromX, fromY, toX, toY))
            {
                List<BoardSequence> sequences = _gameService.SwapTile(fromX, fromY, toX, toY);
                AnimateBoard(sequences, 0, () => _isAnimating = false);
            }
            else
            {
                RevertSwap(fromX, fromY, toX, toY);
            }
        }

        private void RegisterSequenceScores(List<BoardSequence> sequences)
        {
            for (int i = 0; i < sequences.Count; i++)
            {
                BoardSequence sequence = sequences[i];
                int comboLevel = i + 1;

                _scoreController.RegisterMatch(sequence.MatchType, sequence.MatchCount, comboLevel);
            }
        }

        private void RevertSwap(int fromX, int fromY, int toX, int toY)
        {
            _boardView.SwapTiles(toX, toY, fromX, fromY).onComplete += () =>
            {
                _boardView.GetTile(fromX, fromY)?.UnSelect();
                _isAnimating = false;
            };
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
                comboLevel
            );

            sequence.AppendCallback(() =>
            {
                _boardView.DestroyTiles(boardSequence.MatchedPosition);
            });

            sequence.AppendCallback(() =>
            {
                _boardView.MoveTiles(boardSequence.MovedTiles).Play();
            });

            sequence.AppendInterval(appendInterval);

            sequence.AppendCallback(() =>
            {
                _boardView.CreateTile(boardSequence.AddedTiles).Play();
            });

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
