using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class BoardSequence
    {
        public List<MovedTileInfo> MovedTiles { get; set; }
        public List<AddedTileInfo> AddedTiles { get; set; }
        public List<Vector2Int> MatchedPosition { get; set; }
        public int MatchType { get; set; }
        public ResolveType ResolveType { get; set; }
        public int MatchCount { get; set; }
    }
}
