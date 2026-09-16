using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        private Tile[,] _boardTiles;
        private int[] _tilesTypes;
        private int _tileCount;
        private int _width;
        private int _height;
        private readonly System.Random _random = new();

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
        {
            _width = boardWidth;
            _height = boardHeight;
            _tilesTypes = new int[] { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(_width, _height, _tilesTypes);

            return ConvertToNestedList(_boardTiles);
        }

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            if (Math.Abs(fromX - toX) + Math.Abs(fromY - toY) != 1)
            {
                return false;
            }

            Tile[,] tempBoard = (Tile[,])_boardTiles.Clone();
            (tempBoard[toY, toX], tempBoard[fromY, fromX]) = (
                tempBoard[fromY, fromX],
                tempBoard[toY, toX]
            );

            return HasAnyMatch(tempBoard);
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            Tile[,] currentBoard = (Tile[,])_boardTiles.Clone();
            (currentBoard[toY, toX], currentBoard[fromY, fromX]) = (
                currentBoard[fromY, fromX],
                currentBoard[toY, toX]
            );

            List<BoardSequence> boardSequences = new();
            bool[,] matched = new bool[_height, _width];

            while (FindMatches(currentBoard, matched))
            {
                List<Vector2Int> matchedPositions = new();

                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        if (matched[y, x])
                        {
                            matchedPositions.Add(new Vector2Int(x, y));
                            currentBoard[y, x] = new Tile { Id = -1, Type = -1 };
                        }
                    }
                }

                List<MovedTileInfo> movedTiles = new();
                for (int x = 0; x < _width; x++)
                {
                    int emptySpaces = 0;
                    for (int y = 0; y < _height; y++)
                    {
                        if (currentBoard[y, x].Type == -1)
                        {
                            emptySpaces++;
                        }
                        else if (emptySpaces > 0)
                        {
                            Tile tileToMove = currentBoard[y, x];
                            int targetY = y - emptySpaces;

                            currentBoard[targetY, x] = tileToMove;
                            currentBoard[y, x] = new Tile { Id = -1, Type = -1 };

                            movedTiles.Add(
                                new MovedTileInfo
                                {
                                    From = new Vector2Int(x, y),
                                    To = new Vector2Int(x, targetY),
                                }
                            );
                        }
                    }
                }

                List<AddedTileInfo> addedTiles = new();
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        if (currentBoard[y, x].Type == -1)
                        {
                            int randomType = _tilesTypes[_random.Next(_tilesTypes.Length)];
                            currentBoard[y, x] = new Tile { Id = _tileCount++, Type = randomType };

                            addedTiles.Add(
                                new AddedTileInfo
                                {
                                    Position = new Vector2Int(x, y),
                                    Type = randomType,
                                }
                            );
                        }
                    }
                }

                boardSequences.Add(
                    new BoardSequence
                    {
                        MatchedPosition = matchedPositions,
                        MovedTiles = movedTiles,
                        AddedTiles = addedTiles,
                    }
                );
            }

            _boardTiles = currentBoard;
            return boardSequences;
        }

        private Tile[,] CreateBoard(int width, int height, int[] tileTypes)
        {
            Tile[,] board = new Tile[height, width];
            _tileCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int selectedType;
                    do
                    {
                        selectedType = tileTypes[_random.Next(tileTypes.Length)];
                    } while (
                        (
                            x >= 2
                            && board[y, x - 1].Type == selectedType
                            && board[y, x - 2].Type == selectedType
                        )
                        || (
                            y >= 2
                            && board[y - 1, x].Type == selectedType
                            && board[y - 2, x].Type == selectedType
                        )
                    );

                    board[y, x] = new Tile { Id = _tileCount++, Type = selectedType };
                }
            }

            return board;
        }

        private bool FindMatches(Tile[,] board, bool[,] matchedOutput)
        {
            Array.Clear(matchedOutput, 0, matchedOutput.Length);
            bool foundMatch = false;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (board[y, x].Type == -1)
                        continue;

                    if (
                        x >= 2
                        && board[y, x].Type == board[y, x - 1].Type
                        && board[y, x - 1].Type == board[y, x - 2].Type
                    )
                    {
                        matchedOutput[y, x] =
                            matchedOutput[y, x - 1] =
                            matchedOutput[y, x - 2] =
                                true;
                        foundMatch = true;
                    }

                    if (
                        y >= 2
                        && board[y, x].Type == board[y - 1, x].Type
                        && board[y - 1, x].Type == board[y - 2, x].Type
                    )
                    {
                        matchedOutput[y, x] =
                            matchedOutput[y - 1, x] =
                            matchedOutput[y - 2, x] =
                                true;
                        foundMatch = true;
                    }
                }
            }

            return foundMatch;
        }

        private bool HasAnyMatch(Tile[,] board)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int type = board[y, x].Type;
                    if (type == -1)
                        continue;

                    if (x >= 2 && type == board[y, x - 1].Type && type == board[y, x - 2].Type)
                        return true;
                    if (y >= 2 && type == board[y - 1, x].Type && type == board[y - 2, x].Type)
                        return true;
                }
            }
            return false;
        }

        private List<List<Tile>> ConvertToNestedList(Tile[,] board)
        {
            List<List<Tile>> list = new(_height);
            for (int y = 0; y < _height; y++)
            {
                List<Tile> row = new(_width);
                for (int x = 0; x < _width; x++)
                {
                    row.Add(board[y, x]);
                }
                list.Add(row);
            }
            return list;
        }
    }
}
