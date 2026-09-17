using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Controllers;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.UI.Buff
{
    public class UIBuffSelection : MonoBehaviour
    {
        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private UIBuffView[] _buffViews;

        public void OpenSelectionData(BuffController buffController, Action onChooseBuff)
        {
            _canvas.enabled = true;

            List<BuffData> buffs = buffController.GetRandomBuffOptions();

            if (buffs.Count > 3)
            {
                Debug.LogError(
                    "[UIBuffSelection] Invalid size, there too many buffs for selections in view"
                );
                return;
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                BuffData buffData = buffs[i];

                _buffViews[i]
                    .Setup(
                        buffData.GetDescription(),
                        buffData.GetTitle(),
                        () =>
                        {
                            buffController.ApplyBuff(buffData);
                            SoundController.Play("Click");
                            _canvas.enabled = false;
                            onChooseBuff?.Invoke();
                        }
                    );
            }
        }
    }
}
