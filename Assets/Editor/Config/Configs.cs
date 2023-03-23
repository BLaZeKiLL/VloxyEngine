using UnityEditor;

namespace CodeBlaze.Editor.Config {

    public static class Configs {

        [MenuItem("Config/Debug")]
        private static void Debug() {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
            UnityEngine.Debug.Log("Debug config set");
        }

        [MenuItem("Config/Release")]
        private static void Release() {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2"
            });
            UnityEngine.Debug.Log("Release config set");
        }

    }

}