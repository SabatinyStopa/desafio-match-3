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

        private int _currentType;

        #region Unity
        private void OnDestroy() => transform.DOKill();
        #endregion

        public void SetColor(Color color) => _background.color = color;

        public void Select()
        {
            transform.DOKill();
            transform
                .DOPunchScale(Vector3.one * 0.2f, 0.2f)
                .OnComplete(() => _selectedBorder.enabled = true);
        }

        public void UnSelect()
        {
            transform.DOKill();
            transform
                .DOPunchScale(Vector3.one * 0.2f, 0.2f)
                .OnComplete(() => _selectedBorder.enabled = false);
        }

        public void UnSelectImmediate() => _selectedBorder.enabled = false;

        public void SetType(int type) => _currentType = type;

        public int GetCurrentType() => _currentType;
    }
}
