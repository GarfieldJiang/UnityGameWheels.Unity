using COL.UnityGameWheels.Core;
using UnityEngine;

namespace COL.UnityGameWheels.Unity
{
    public class RefPoolServiceConfig : ScriptableObject, IRefPoolServiceConfigReader
    {
        public int DefaultCapacity => m_DefaultCapacity;

        [SerializeField]
        private int m_DefaultCapacity = 1;
    }
}