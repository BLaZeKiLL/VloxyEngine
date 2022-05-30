using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CodeBlaze.Editor.Build {

    public static class Builder {

        [MenuItem("Build/Mono/Debug")]
        private static void BuildMonoDebug() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Mono-Debug/vloxy_mono_debug.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Mono-Debug");
            }
        }

        [MenuItem("Build/Mono/Release")]
        private static void BuildMonoRelease() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Mono-Release/vloxy_mono_release.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Mono-Release");
            }
        }

        [MenuItem("Build/IL2CPP/Debug")]
        private static void BuildIL2CPPDebug() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/IL2CPP-Debug/vloxy_il2cpp_debug.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/IL2CPP-Debug");
            }
        }

        [MenuItem("Build/IL2CPP/Release")]
        private static void BuildIL2CPPRelease() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/IL2CPP-Release/vloxy_il2cpp_release.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/IL2CPP-Release");
            }
        }

    }

}