namespace COL.UnityGameWheels.Unity.Editor
{
    using Asset;

    public partial class AssetBundleBuilderConfig
    {
        public class PlatformConfig
        {
            public ResourcePlatform TargetPlatform;
            public bool SkipBuild;
            public bool AutomaticIncrementResourceVersion = true;
        }
    }
}
