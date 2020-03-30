using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;

#endif

namespace COL.UnityGameWheels.Unity.Editor
{
    public abstract class BaseManagerInspector : UnityEditor.Editor
    {
        public abstract bool AvailableWhenPlaying { get; }

        public abstract bool AvailableWhenNotPlaying { get; }

        public virtual bool AvailableWhenCompiling
        {
            get { return false; }
        }


        
#if UNITY_2019_1_OR_NEWER
        private void ImGuiDraw()
#else
        public override void OnInspectorGUI()
#endif
        {
            if (!CheckAvailableOrDrawHelpBox())
            {
                return;
            }

            DrawContent();
        }

#if UNITY_2019_1_OR_NEWER
        public override VisualElement CreateInspectorGUI()
        {
            var ve = new VisualElement();
            ve.Add(new IMGUIContainer(ImGuiDraw));
            return ve;
        }
#endif

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