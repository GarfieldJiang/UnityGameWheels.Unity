using UnityEngine;

namespace COL.UnityGameWheels.Unity
{
    /// <summary>
    /// A extension of Unity's MonoBehaviour
    /// </summary>
    public class MonoBehaviourEx : MonoBehaviour
    {
        /// <summary>
        /// Cached transform.
        /// </summary>
        public Transform CacheTransform { get; private set; }

        protected virtual void Awake()
        {
            CacheTransform = transform;
        }

        protected virtual void OnDestroy()
        {
            CacheTransform = null;
        }
    }
}
