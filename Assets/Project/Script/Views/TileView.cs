using DG.Tweening;
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

        public void Select() =>
            transform
                .DOPunchScale(Vector3.one * 0.2f, 0.2f)
                .OnComplete(() => _selectedBorder.enabled = true);

        public void UnSelect() =>
            transform
                .DOPunchScale(Vector3.one * 0.2f, 0.2f)
                .OnComplete(() => _selectedBorder.enabled = false);
    }
}
