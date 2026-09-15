using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private BoardView _boardView;

        [SerializeField]
        private int _boardHeight = 10;

        [SerializeField]
        private int _boardWidth = 10;

        private GameService _gameService;
        private bool _isAnimating;
        private int _selectedX = -1;
        private int _selectedY = -1;

        #region Unity
        private void Awake()
        {
            _gameService = new GameService();
            _boardView.TileClicked += OnTileClick;
        }

        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;
        }

        private void Start()
        {
            List<List<Tile>> board = _gameService.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
            _boardView.MakeAllTilesPopUp().Play();
        }
        #endregion

        private void AnimateBoard(List<BoardSequence> boardSequences, int index, Action onComplete)
        {
            BoardSequence boardSequence = boardSequences[index];
            float appendInterval = 0.3f;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(_boardView.AnimateMatchPosition(boardSequence.MatchedPosition));

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

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating)
                return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (_selectedX == x && _selectedY == y)
                {
                    _boardView.GetTile(_selectedX, _selectedY).UnSelect();
                    _selectedX = -1;
                    _selectedY = -1;
                    return;
                }

                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    _boardView.GetTile(_selectedX, _selectedY).UnSelect();
                    _selectedX = x;
                    _selectedY = y;
                    _boardView.GetTile(x, y).Select();
                }
                else
                {
                    _isAnimating = true;

                    int fromX = _selectedX;
                    int fromY = _selectedY;

                    _selectedX = -1;
                    _selectedY = -1;

                    TileView selectedTile = _boardView.GetTile(fromX, fromY);
                    if (selectedTile != null)
                    {
                        selectedTile.UnSelect();
                    }

                    _boardView.SwapTiles(fromX, fromY, x, y).onComplete += () =>
                    {
                        bool isValid = _gameService.IsValidMovement(fromX, fromY, x, y);
                        if (isValid)
                        {
                            List<BoardSequence> swapResult = _gameService.SwapTile(
                                fromX,
                                fromY,
                                x,
                                y
                            );
                            AnimateBoard(swapResult, 0, () => _isAnimating = false);
                        }
                        else
                        {
                            _boardView.SwapTiles(x, y, fromX, fromY).onComplete += () =>
                            {
                                TileView revertedTile = _boardView.GetTile(fromX, fromY);
                                if (revertedTile != null)
                                {
                                    revertedTile.UnSelect();
                                }

                                _isAnimating = false;
                            };
                        }
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
                _boardView.GetTile(x, y).Select();
            }
        }
    }
}
