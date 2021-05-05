using COL.UnityGameWheels.Core.Ioc;
using UnityEngine;

namespace COL.UnityGameWheels.Unity.Editor
{
    public abstract class BaseServiceInspector
    {
        protected internal virtual void OnShow(object serviceInstance)
        {
            //Debug.Log($"[{GetType()} {nameof(OnShow)}]");
        }

        protected internal virtual void OnHide()
        {
            //Debug.Log($"[{GetType()} {nameof(OnHide)}]");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="serviceInstance"></param>
        /// <returns>Whether the outer view should repaint.</returns>
        protected internal abstract bool DrawContent(object serviceInstance);
    }
}