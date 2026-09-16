using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Utilities
{
    /// <summary>
    /// A Pool of T objects to have prewarmed and instantiated objects of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T>
        where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _availableObjects = new();
        private readonly HashSet<T> _activeObjects = new();

        public ObjectPool(T prefab, int initialSize = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;

            Prewarm(initialSize);
        }

        public T Get(Vector3 position)
        {
            return Get(instance =>
            {
                instance.transform.position = position;
            });
        }

        public T Get(Action<T> setup)
        {
            T instance;

            if (_availableObjects.Count > 0)
            {
                instance = _availableObjects.Dequeue();
            }
            else
            {
                instance = CreateInstance();
            }

            _activeObjects.Add(instance);

            setup?.Invoke(instance);

            instance.gameObject.SetActive(true);

            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!_activeObjects.Remove(instance))
            {
                Debug.LogWarning(
                    $"[Object Pool] Trying to release {instance.name}, but it is not active in this pool."
                );

                return;
            }

            instance.gameObject.SetActive(false);

            if (_parent != null)
            {
                instance.transform.SetParent(_parent);
            }

            _availableObjects.Enqueue(instance);
        }

        public void Prewarm(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                T instance = CreateInstance();

                instance.gameObject.SetActive(false);

                _availableObjects.Enqueue(instance);
            }
        }

        private T CreateInstance() => UnityEngine.Object.Instantiate(_prefab, _parent);
    }
}
