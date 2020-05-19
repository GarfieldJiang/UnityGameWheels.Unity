namespace COL.UnityGameWheels.Unity.Editor
{
    public partial class AssetBundleOrganizerEditorWindow
    {
        private abstract class BaseSection
        {
            protected AssetBundleOrganizerEditorWindow m_EditorWindow = null;

            public BaseSection(AssetBundleOrganizerEditorWindow editorWindow)
            {
                m_EditorWindow = editorWindow;
            }

            public abstract void Draw();

            protected AssetBundleOrganizer Organizer => m_EditorWindow.m_AssetBundleOrganizer;
        }
    }
}
