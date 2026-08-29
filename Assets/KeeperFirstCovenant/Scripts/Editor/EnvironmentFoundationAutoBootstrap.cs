#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace KeeperFirstCovenant.EditorTools
{
    [InitializeOnLoad]
    public static class EnvironmentFoundationAutoBootstrap
    {
        private const string SessionKey =
            "KeeperFirstCovenant.EnvironmentFoundation.AutoBootstrap.v1";

        static EnvironmentFoundationAutoBootstrap()
        {
            EditorApplication.delayCall += TryBuild;
        }

        private static void TryBuild()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            string requiredTexture =
                EnvironmentFoundationPackBuilder.ArtRoot +
                "/Foliage/Grass_Wildflowers_A.png";

            if (!File.Exists(requiredTexture))
                return;

            EnvironmentFoundationPackBuilder.EnsureBuilt(true);
        }
    }
}
#endif
