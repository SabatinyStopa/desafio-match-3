using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    public class TileView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _selectedBorder;

        [SerializeField]
        private SpriteRenderer _background;

        public void SetColor(Color color) => _background.color = color;

        public void Select() => _selectedBorder.enabled = true;

        public void UnSelect() => _selectedBorder.enabled = false;
    }
}
