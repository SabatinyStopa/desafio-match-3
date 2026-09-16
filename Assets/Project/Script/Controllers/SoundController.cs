using System.Collections;
using Gazeus.DesafioMatch3.ScriptableObjects.Audio;
using Gazeus.DesafioMatch3.Utilities;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class SoundController : MonoBehaviour
    {
        private static SoundController _instance;

        [SerializeField]
        private AudioDatabase _database;

        [SerializeField]
        private AudioSource _musicSource;

        [SerializeField]
        private int _poolInitialSize = 10;

        private ObjectPool<AudioSource> _sfxPool;

        private void Awake()
        {
            _instance = this;
            _database.Initialize();

            GameObject tempGo = new("AudioSource_Template");
            AudioSource audioSourceComponent = tempGo.AddComponent<AudioSource>();

            _sfxPool = new ObjectPool<AudioSource>(
                audioSourceComponent,
                _poolInitialSize,
                transform
            );

            Destroy(tempGo);
        }

        public static void Play(string id)
        {
            if (_instance == null)
            {
                return;
            }
            _instance.PlaySound(id);
        }

        public static void PlayMusic(string id, bool loop = true)
        {
            if (_instance == null)
            {
                return;
            }
            _instance.PlayBackgroundMusic(id, loop);
        }

        private void PlaySound(string id)
        {
            SoundData sound = _database.GetSound(id);
            if (sound == null)
                return;

            AudioClip clip = sound.GetRandomClip();
            if (clip == null)
                return;

            AudioSource source = _sfxPool.Get();
            source.clip = clip;
            source.volume = sound.Volume;
            source.Play();

            StartCoroutine(ReturnToPoolWhenFinished(source, clip.length));
        }

        private void PlayBackgroundMusic(string id, bool loop)
        {
            SoundData sound = _database.GetSound(id);
            if (sound == null)
            {
                return;
            }

            AudioClip clip = sound.GetRandomClip();

            if (clip == null)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.volume = sound.Volume;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        private IEnumerator ReturnToPoolWhenFinished(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);
            source.Stop();
            _sfxPool.Release(source);
        }
    }
}
