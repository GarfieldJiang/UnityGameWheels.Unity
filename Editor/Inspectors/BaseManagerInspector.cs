using UnityEditor;

namespace COL.UnityGameWheels.Unity.Editor
{
    public abstract class BaseManagerInspector : UnityEditor.Editor
    {
        public abstract bool AvailableWhenPlaying { get; }

        public abstract bool AvailableWhenNotPlaying { get; }

        public virtual bool AvailableWhenCompiling { get { return false; } }

        public override void OnInspectorGUI()
        {
            if (!CheckAvailableOrDrawHelpBox())
            {
                return;
            }

            DrawContent();
        }

        private bool CheckAvailableOrDrawHelpBox()
        {
            if (!AvailableWhenNotPlaying && !EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Only available when playing.", MessageType.Info);
                return false;
            }

            if (!AvailableWhenPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("Not available when playing.", MessageType.Info);
                return false;
            }

            if (!AvailableWhenCompiling && EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("Not available when compiling.", MessageType.Info);
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Changing play mode...", MessageType.Info);
                return false;
            }

            return true;
        }

        protected abstract void DrawContent();
    }
}
