using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CodeBlaze.Editor.Build {

    public static class Builder {

        #region Windows

        [MenuItem("Build/Windows/Mono/Debug")]
        private static void BuildWindowsMonoDebug() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Windows/Mono-Debug/vloxy_mono_debug.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Windows/Mono-Debug");
            }
        }

        [MenuItem("Build/Windows/Mono/Release")]
        private static void BuildWindowsMonoRelease() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Windows/Mono-Release/vloxy_mono_release.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Windows/Mono-Release");
            }
        }

        [MenuItem("Build/Windows/IL2CPP/Debug")]
        private static void BuildWindowsIL2CPPDebug() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Windows/IL2CPP-Debug/vloxy_il2cpp_debug.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Windows/IL2CPP-Debug");
            }
        }

        [MenuItem("Build/Windows/IL2CPP/Release")]
        private static void BuildWindowsIL2CPPRelease() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Windows/IL2CPP-Release/vloxy_il2cpp_release.exe",
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Windows/IL2CPP-Release");
            }
        }
        
        #endregion

        #region Android

        [MenuItem("Build/Android/Mono/Debug")]
        private static void BuildAndroidMonoDebug() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Android/Mono-Debug/vloxy_mono_debug.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Android/Mono-Debug");
            }
        }

        [MenuItem("Build/Android/Mono/Release")]
        private static void BuildAndroidMonoRelease() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Android/Mono-Release/vloxy_mono_release.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Android/Mono-Release");
            }
        }

        [MenuItem("Build/Android/IL2CPP/Debug")]
        private static void BuildAndroidIL2CPPDebug() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING", "VLOXY_DEBUG"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Android/IL2CPP-Debug/vloxy_il2cpp_debug.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Android/IL2CPP-Debug");
            }
        }

        [MenuItem("Build/Android/IL2CPP/Release")]
        private static void BuildAndroidIL2CPPRelease() {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,new [] {
                "UNITY_POST_PROCESSING_STACK_V2", "VLOXY_LOGGING"
            });
            
            var options = new BuildPlayerOptions {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "Build/Android/IL2CPP-Release/vloxy_il2cpp_release.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded) {
                EditorUtility.RevealInFinder("Build/Android/IL2CPP-Release");
            }
        }
        
        #endregion
    }

}