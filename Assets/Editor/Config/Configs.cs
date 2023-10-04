using UnityEditor;

namespace CodeBlaze.Editor.Config {

    public static class Configs {

        [MenuItem("Config/Debug")]
        private static void Debug() {
            UnityEngine.Debug.Log("Debug config set");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
        }

        [MenuItem("Config/Release")]
        private static void Release() {
            UnityEngine.Debug.Log("Release config set");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING",
            });
        }

    }

}