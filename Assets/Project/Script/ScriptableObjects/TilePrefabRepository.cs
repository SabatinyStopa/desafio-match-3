using UnityEngine;

namespace Gazeus.DesafioMatch3.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/TilePrefabRepository")]
    public class TilePrefabRepository : ScriptableObject
    {
        [SerializeField]
        private Sprite[] _tilesTypes;

        public Sprite[] TileTypes => _tilesTypes;
    }
}
