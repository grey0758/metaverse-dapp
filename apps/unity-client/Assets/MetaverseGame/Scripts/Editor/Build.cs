using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MetaverseGame.Editor
{
    public static class Build
    {
        private const string BootstrapScene = "Assets/MetaverseGame/Scenes/Bootstrap.unity";

        public static void PerformWindowsDevelopment()
        {
            string output = RequireBuildOutput("Featherfall/Featherfall.exe");
            BuildPlayer(BuildTarget.StandaloneWindows64, output);
        }

        public static void PerformAndroidDevelopment()
        {
            string output = RequireBuildOutput("Featherfall.apk");
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android,
                ScriptingImplementation.IL2CPP);
            BuildPlayer(BuildTarget.Android, output);
        }

        private static void BuildPlayer(BuildTarget target, string output)
        {
            string commit = Environment.GetEnvironmentVariable("BUILD_COMMIT");
            string buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");
            if (string.IsNullOrWhiteSpace(commit) || string.IsNullOrWhiteSpace(buildNumber))
            {
                throw new InvalidOperationException(
                    "BUILD_COMMIT and BUILD_NUMBER are required for identifiable artifacts.");
            }
            if (!File.Exists(BootstrapScene))
            {
                ProjectBootstrap.CreateDevelopmentScene();
            }

            string outputDirectory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { BootstrapScene },
                locationPathName = output,
                target = target,
                options = BuildOptions.Development,
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unity build failed: {report.summary.result}");
            }

            WriteManifest(output, target, commit, buildNumber, report);
        }

        private static string RequireBuildOutput(string fallback)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] == "-buildOutput")
                {
                    return Path.GetFullPath(arguments[index + 1]);
                }
            }
            return Path.GetFullPath(Path.Combine("Builds", fallback));
        }

        private static void WriteManifest(
            string output,
            BuildTarget target,
            string commit,
            string buildNumber,
            BuildReport report)
        {
            var manifest = new BuildManifest
            {
                commit = commit,
                unityVersion = Application.unityVersion,
                target = target.ToString(),
                scriptingBackend = PlayerSettings.GetScriptingBackend(
                    NamedBuildTarget.FromBuildTargetGroup(
                        BuildPipeline.GetBuildTargetGroup(target))).ToString(),
                buildNumber = buildNumber,
                timestampUtc = DateTime.UtcNow.ToString("O"),
                output = output,
                totalBytes = report.summary.totalSize,
                sha256 = ComputeSha256(output),
            };
            File.WriteAllText(
                output + ".manifest.json",
                JsonUtility.ToJson(manifest, true));
        }

        private static string ComputeSha256(string path)
        {
            using SHA256 algorithm = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(algorithm.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        [Serializable]
        private sealed class BuildManifest
        {
            public string commit;
            public string unityVersion;
            public string target;
            public string scriptingBackend;
            public string buildNumber;
            public string timestampUtc;
            public string output;
            public ulong totalBytes;
            public string sha256;
        }
    }
}
