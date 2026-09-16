using System;
using System.Collections;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    public class ExplosionEffectView : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem[] _particles;

        public void Play(Action onComplete)
        {
            foreach (ParticleSystem particle in _particles)
            {
                if (particle.isPlaying)
                {
                    particle.Stop();
                }
                particle.Play();
            }

            StartCoroutine(WaitParticlesRoutine(onComplete));
        }

        private IEnumerator WaitParticlesRoutine(Action onComplete)
        {
            yield return new WaitUntil(() =>
            {
                foreach (ParticleSystem particle in _particles)
                {
                    if (particle.IsAlive(true))
                    {
                        return false;
                    }
                }
                return true;
            });

            onComplete?.Invoke();
        }
    }
}
