using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        private Tile[,] _board;
        private int[] _tileTypes;
        private int _nextTileId;
        private int _columns;
        private int _rows;

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
        {
            _columns = boardWidth;
            _rows = boardHeight;
            _tileTypes = new[] { 0, 1, 2, 3 };
            _board = GenerateBoard(_columns, _rows, _tileTypes);

            return ConvertToListOfLists(_board);
        }

        public List<BoardSequence> SwapTile(int originX, int originY, int targetX, int targetY)
        {
            Tile[,] workingBoard = (Tile[,])_board.Clone();
            SwapTiles(workingBoard, originX, originY, targetX, targetY);

            List<BoardSequence> sequenceHistory = new();
            bool[,] matchGrid = new bool[_rows, _columns];

            while (EvaluateMatches(workingBoard, matchGrid))
            {
                HashSet<Vector2Int> matchedPositions = ExtractMatchedPositions(matchGrid);
                Vector2Int actionPivot = DetermineActionPivot(
                    matchedPositions,
                    originX,
                    originY,
                    targetX,
                    targetY
                );

                int matchType = workingBoard[actionPivot.y, actionPivot.x].Type;
                int maxLineCount = GetMaxStraightLineCount(
                    workingBoard,
                    matchedPositions,
                    actionPivot,
                    matchType
                );
                bool isSquare = HasSquarePattern(matchedPositions);
                ResolveType resolveType = ResolveType.Simple;

                if (maxLineCount >= 5)
                {
                    resolveType = ResolveType.FiveSequence;
                    ClearSingleAxis(matchedPositions, actionPivot);
                }
                else if (maxLineCount == 4)
                {
                    resolveType = ResolveType.FourSequence;
                    ClearMatchingTypeOnDominantAxis(
                        workingBoard,
                        matchedPositions,
                        actionPivot,
                        matchType
                    );
                }
                else if (isSquare)
                {
                    resolveType = ResolveType.Square;
                    ExpandExplosionArea(matchedPositions, actionPivot, 3, 3);
                }

                List<Vector2Int> matchedList = new(matchedPositions);
                ClearPositionsOnBoard(workingBoard, matchedList);

                List<MovedTileInfo> movedTiles = ApplyGravity(workingBoard);
                List<AddedTileInfo> addedTiles = ReplenishBoard(workingBoard);

                sequenceHistory.Add(
                    new BoardSequence
                    {
                        MatchedPosition = matchedList,
                        MovedTiles = movedTiles,
                        AddedTiles = addedTiles,
                        MatchType = matchType,
                        MatchCount = matchedList.Count,
                        ResolveType = resolveType,
                    }
                );
            }

            _board = workingBoard;
            return sequenceHistory;
        }

        private Vector2Int DetermineActionPivot(
            HashSet<Vector2Int> matchedPositions,
            int originX,
            int originY,
            int targetX,
            int targetY
        )
        {
            Vector2Int target = new(targetX, targetY);
            if (matchedPositions.Contains(target))
                return target;

            Vector2Int origin = new(originX, originY);
            if (matchedPositions.Contains(origin))
                return origin;

            using var enumerator = matchedPositions.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        private int GetMaxStraightLineCount(
            Tile[,] board,
            HashSet<Vector2Int> matchedPositions,
            Vector2Int pivot,
            int targetType
        )
        {
            int horizontalCount = 1;
            for (
                int x = pivot.x + 1;
                x < _columns
                    && matchedPositions.Contains(new Vector2Int(x, pivot.y))
                    && board[pivot.y, x].Type == targetType;
                x++
            )
                horizontalCount++;
            for (
                int x = pivot.x - 1;
                x >= 0
                    && matchedPositions.Contains(new Vector2Int(x, pivot.y))
                    && board[pivot.y, x].Type == targetType;
                x--
            )
                horizontalCount++;

            int verticalCount = 1;
            for (
                int y = pivot.y + 1;
                y < _rows
                    && matchedPositions.Contains(new Vector2Int(pivot.x, y))
                    && board[y, pivot.x].Type == targetType;
                y++
            )
                verticalCount++;
            for (
                int y = pivot.y - 1;
                y >= 0
                    && matchedPositions.Contains(new Vector2Int(pivot.x, y))
                    && board[y, pivot.x].Type == targetType;
                y--
            )
                verticalCount++;

            return Math.Max(horizontalCount, verticalCount);
        }

        private void ClearSingleAxis(HashSet<Vector2Int> matchedPositions, Vector2Int pivot)
        {
            if (IsHorizontalMatchDominant(matchedPositions, pivot))
            {
                for (int x = 0; x < _columns; x++)
                    matchedPositions.Add(new Vector2Int(x, pivot.y));
            }
            else
            {
                for (int y = 0; y < _rows; y++)
                    matchedPositions.Add(new Vector2Int(pivot.x, y));
            }
        }

        private void ClearMatchingTypeOnDominantAxis(
            Tile[,] board,
            HashSet<Vector2Int> matchedPositions,
            Vector2Int pivot,
            int targetType
        )
        {
            int rowMatches = 0;
            for (int x = 0; x < _columns; x++)
            {
                if (board[pivot.y, x].Type == targetType)
                    rowMatches++;
            }

            int colMatches = 0;
            for (int y = 0; y < _rows; y++)
            {
                if (board[y, pivot.x].Type == targetType)
                    colMatches++;
            }

            if (rowMatches >= colMatches)
            {
                for (int x = 0; x < _columns; x++)
                {
                    if (board[pivot.y, x].Type == targetType)
                        matchedPositions.Add(new Vector2Int(x, pivot.y));
                }
            }
            else
            {
                for (int y = 0; y < _rows; y++)
                {
                    if (board[y, pivot.x].Type == targetType)
                        matchedPositions.Add(new Vector2Int(pivot.x, y));
                }
            }
        }

        private void ExpandExplosionArea(
            HashSet<Vector2Int> matchedPositions,
            Vector2Int pivot,
            int areaWidth,
            int areaHeight
        )
        {
            int startX = pivot.x - (areaWidth / 2);
            int endX = startX + areaWidth - 1;
            int startY = pivot.y - (areaHeight / 2);
            int endY = startY + areaHeight - 1;

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (x >= 0 && x < _columns && y >= 0 && y < _rows)
                    {
                        matchedPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private bool HasSquarePattern(HashSet<Vector2Int> matchedPositions)
        {
            foreach (var pos in matchedPositions)
            {
                if (
                    matchedPositions.Contains(new Vector2Int(pos.x + 1, pos.y))
                    && matchedPositions.Contains(new Vector2Int(pos.x, pos.y + 1))
                    && matchedPositions.Contains(new Vector2Int(pos.x + 1, pos.y + 1))
                )
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsHorizontalMatchDominant(
            HashSet<Vector2Int> matchedPositions,
            Vector2Int pivot
        )
        {
            int horizontalCount = 0;
            int verticalCount = 0;

            foreach (Vector2Int pos in matchedPositions)
            {
                if (pos.y == pivot.y)
                    horizontalCount++;
                if (pos.x == pivot.x)
                    verticalCount++;
            }

            return horizontalCount >= verticalCount;
        }

        private Tile[,] GenerateBoard(int columns, int rows, int[] availableTypes)
        {
            Tile[,] board = new Tile[rows, columns];
            _nextTileId = 0;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int selectedType;
                    do
                    {
                        selectedType = availableTypes[
                            UnityEngine.Random.Range(0, availableTypes.Length)
                        ];
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

                    board[y, x] = new Tile { Id = _nextTileId++, Type = selectedType };
                }
            }

            return board;
        }

        private bool EvaluateMatches(Tile[,] board, bool[,] matchGrid)
        {
            Array.Clear(matchGrid, 0, matchGrid.Length);
            bool foundMatches = false;

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    if (board[y, x].Type == -1)
                        continue;

                    if (
                        x >= 2
                        && board[y, x].Type == board[y, x - 1].Type
                        && board[y, x - 1].Type == board[y, x - 2].Type
                    )
                    {
                        matchGrid[y, x] = matchGrid[y, x - 1] = matchGrid[y, x - 2] = true;
                        foundMatches = true;
                    }

                    if (
                        y >= 2
                        && board[y, x].Type == board[y - 1, x].Type
                        && board[y - 1, x].Type == board[y - 2, x].Type
                    )
                    {
                        matchGrid[y, x] = matchGrid[y - 1, x] = matchGrid[y - 2, x] = true;
                        foundMatches = true;
                    }

                    if (x < _columns - 1 && y < _rows - 1)
                    {
                        int currentType = board[y, x].Type;
                        if (
                            currentType == board[y, x + 1].Type
                            && currentType == board[y + 1, x].Type
                            && currentType == board[y + 1, x + 1].Type
                        )
                        {
                            matchGrid[y, x] = matchGrid[y, x + 1] = true;
                            matchGrid[y + 1, x] = matchGrid[y + 1, x + 1] = true;
                            foundMatches = true;
                        }
                    }
                }
            }

            return foundMatches;
        }

        private HashSet<Vector2Int> ExtractMatchedPositions(bool[,] matchGrid)
        {
            HashSet<Vector2Int> positions = new();
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    if (matchGrid[y, x])
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
            return positions;
        }

        private void ClearPositionsOnBoard(Tile[,] board, List<Vector2Int> positions)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2Int pos = positions[i];
                board[pos.y, pos.x] = new Tile { Id = -1, Type = -1 };
            }
        }

        private List<MovedTileInfo> ApplyGravity(Tile[,] board)
        {
            List<MovedTileInfo> movedTiles = new();

            for (int x = 0; x < _columns; x++)
            {
                int emptySpacesBelow = 0;
                for (int y = 0; y < _rows; y++)
                {
                    if (board[y, x].Type == -1)
                    {
                        emptySpacesBelow++;
                    }
                    else if (emptySpacesBelow > 0)
                    {
                        Tile tileToMove = board[y, x];
                        int targetY = y - emptySpacesBelow;

                        board[targetY, x] = tileToMove;
                        board[y, x] = new Tile { Id = -1, Type = -1 };

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

            return movedTiles;
        }

        private List<AddedTileInfo> ReplenishBoard(Tile[,] board)
        {
            List<AddedTileInfo> addedTiles = new();

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    if (board[y, x].Type == -1)
                    {
                        int newType = _tileTypes[UnityEngine.Random.Range(0, _tileTypes.Length)];
                        board[y, x] = new Tile { Id = _nextTileId++, Type = newType };

                        addedTiles.Add(
                            new AddedTileInfo { Position = new Vector2Int(x, y), Type = newType }
                        );
                    }
                }
            }

            return addedTiles;
        }

        private void SwapTiles(Tile[,] board, int originX, int originY, int targetX, int targetY)
        {
            (board[targetY, targetX], board[originY, originX]) = (
                board[originY, originX],
                board[targetY, targetX]
            );
        }

        private List<List<Tile>> ConvertToListOfLists(Tile[,] board)
        {
            List<List<Tile>> nestedList = new(_rows);
            for (int y = 0; y < _rows; y++)
            {
                List<Tile> row = new(_columns);
                for (int x = 0; x < _columns; x++)
                {
                    row.Add(board[y, x]);
                }
                nestedList.Add(row);
            }
            return nestedList;
        }
    }
}
