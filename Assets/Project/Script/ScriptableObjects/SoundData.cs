using UnityEngine;

namespace Gazeus.DesafioMatch3.ScriptableObjects.Audio
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "Custom/Audio/Sound Data")]
    public class SoundData : ScriptableObject
    {
        public string Id;
        public AudioClip[] Clips;

        [Range(0f, 1f)]
        public float Volume = 1f;

        public AudioClip GetRandomClip()
        {
            if (Clips == null || Clips.Length == 0)
            {
                return null;
            }

            return Clips[Random.Range(0, Clips.Length)];
        }
    }
}
