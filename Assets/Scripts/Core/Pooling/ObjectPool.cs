using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGameFramework.Core.Pooling
{
    /// <summary>
    /// Generic object pool for any Component type.
    /// Reduces garbage collection by reusing objects instead of Instantiate/Destroy.
    /// </summary>
    /// <typeparam name="T">Component type to pool</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available = new Queue<T>();
        private readonly HashSet<T> _inUse = new HashSet<T>();
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onCreate;
        private readonly int _maxSize;

        public int AvailableCount => _available.Count;
        public int InUseCount => _inUse.Count;
        public int TotalCount => AvailableCount + InUseCount;

        /// <summary>
        /// Create a new object pool
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="parent">Parent transform for pooled objects</param>
        /// <param name="initialSize">Number of objects to pre-instantiate</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        /// <param name="onCreate">Called when a new object is created</param>
        /// <param name="onGet">Called when an object is retrieved from pool</param>
        /// <param name="onRelease">Called when an object is returned to pool</param>
        public ObjectPool(
            T prefab,
            Transform parent = null,
            int initialSize = 0,
            int maxSize = 0,
            Action<T> onCreate = null,
            Action<T> onGet = null,
            Action<T> onRelease = null)
        {
            _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            _parent = parent;
            _maxSize = maxSize;
            _onCreate = onCreate;
            _onGet = onGet;
            _onRelease = onRelease;

            // Pre-warm the pool
            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _available.Enqueue(obj);
            }
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        public T Get()
        {
            T obj;

            if (_available.Count > 0)
            {
                obj = _available.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.gameObject.SetActive(true);
            _inUse.Add(obj);
            _onGet?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// Get an object from the pool and set its position
        /// </summary>
        public T Get(Vector3 position)
        {
            var obj = Get();
            obj.transform.position = position;
            return obj;
        }

        /// <summary>
        /// Get an object from the pool and set its position and rotation
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Release(T obj)
        {
            if (obj == null) return;
            if (!_inUse.Contains(obj)) return;

            _inUse.Remove(obj);
            _onRelease?.Invoke(obj);
            obj.gameObject.SetActive(false);

            // If we have a max size and we're over it, destroy instead of pooling
            if (_maxSize > 0 && _available.Count >= _maxSize)
            {
                UnityEngine.Object.Destroy(obj.gameObject);
                return;
            }

            _available.Enqueue(obj);
        }

        /// <summary>
        /// Release all objects currently in use
        /// </summary>
        public void ReleaseAll()
        {
            var inUseList = new List<T>(_inUse);
            foreach (var obj in inUseList)
            {
                Release(obj);
            }
        }

        /// <summary>
        /// Clear the pool and destroy all objects
        /// </summary>
        public void Clear()
        {
            foreach (var obj in _inUse)
            {
                if (obj != null)
                    UnityEngine.Object.Destroy(obj.gameObject);
            }
            _inUse.Clear();

            while (_available.Count > 0)
            {
                var obj = _available.Dequeue();
                if (obj != null)
                    UnityEngine.Object.Destroy(obj.gameObject);
            }
        }

        private T CreateNewObject()
        {
            var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
            _onCreate?.Invoke(obj);
            return obj;
        }
    }
}
