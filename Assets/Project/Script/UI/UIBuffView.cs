using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.UI.Buff
{
    public class UIBuffView : MonoBehaviour
    {
        [SerializeField]
        private Button _upgradeButton;

        [SerializeField]
        private TextMeshProUGUI _buffDescriptionText;

        [SerializeField]
        private TextMeshProUGUI _buffTitleText;

        public void Setup(string description, string title, UnityAction action)
        {
            _upgradeButton.onClick.RemoveAllListeners();
            _upgradeButton.onClick.AddListener(action);
            _buffDescriptionText.SetText(description);
            _buffTitleText.SetText(title);
        }
    }
}
