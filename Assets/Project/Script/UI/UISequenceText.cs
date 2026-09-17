using System.Collections;
using Gazeus.DesafioMatch3.Utilities;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3.UI
{
    public class UISequenceText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _text;

        [SerializeField]
        private float _delayBetweenChars = 0.01f;

        [SerializeField]
        private float _delayBeforeRelease = 2f;

        private Coroutine _typewriterCoroutine;

        public void SetText(string fullText, ObjectPool<UISequenceText> pool)
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }

            _typewriterCoroutine = StartCoroutine(TypeWriterEffect(fullText, pool));
        }

        private IEnumerator TypeWriterEffect(string fullText, ObjectPool<UISequenceText> pool)
        {
            _text.SetText(fullText);

            _text.ForceMeshUpdate();

            int totalVisibleCharacters = _text.textInfo.characterCount;
            _text.maxVisibleCharacters = 0;

            for (int visibleCount = 0; visibleCount <= totalVisibleCharacters; visibleCount++)
            {
                _text.maxVisibleCharacters = visibleCount;
                yield return new WaitForSeconds(_delayBetweenChars);
            }

            yield return new WaitForSeconds(_delayBeforeRelease);

            _typewriterCoroutine = null;

            pool.Release(this);
        }
    }
}
