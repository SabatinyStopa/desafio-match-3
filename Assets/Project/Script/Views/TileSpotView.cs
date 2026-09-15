using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gazeus.DesafioMatch3.Views
{
    public class TileSpotView : MonoBehaviour, IPointerDownHandler
    {
        public event Action<int, int> Clicked;

        private int _x;
        private int _y;

        public Tween AnimateSetTile(GameObject tile)
        {
            tile.transform.SetParent(transform);
            tile.transform.DOKill();

            return tile.transform.DOMove(transform.position, 0.3f);
        }

        public void SetPosition(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public void SetTile(GameObject tile)
        {
            tile.transform.SetParent(transform, false);
            tile.transform.position = transform.position;
        }

        #region Interface
        public void OnPointerDown(PointerEventData eventData) => Clicked?.Invoke(_x, _y);

        #endregion
    }
}
