using UnityEditor;
using UnityEngine;

public static class BuildWebGL
{
    // Invoked from the command line via -executeMethod BuildWebGL.Build
    public static void Build()
    {
        string[] scenes = { "Assets/01_Scenes/K-MainScene.unity" };
        string outputPath = "Builds/WebGL";

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"WebGL build succeeded: {summary.totalSize} bytes at {outputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"WebGL build failed: {summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
