using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.ScriptableObjects.Audio
{
    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "Custom/Audio/AudioDatabase")]
    public class AudioDatabase : ScriptableObject
    {
        [SerializeField]
        private List<SoundData> _sounds;

        private Dictionary<string, SoundData> _soundDictionary;

        public void Initialize()
        {
            _soundDictionary = new Dictionary<string, SoundData>();
            foreach (var sound in _sounds)
            {
                if (sound != null && !_soundDictionary.ContainsKey(sound.Id))
                {
                    _soundDictionary.Add(sound.Id, sound);
                }
            }
        }

        public SoundData GetSound(string id)
        {
            if (_soundDictionary == null)
            {
                Initialize();
            }

            _soundDictionary.TryGetValue(id, out var sound);
            return sound;
        }
    }
}
