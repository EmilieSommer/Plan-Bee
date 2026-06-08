using UnityEditor;
using UnityEngine;

public static class RunFixLibrary
{
    public static void Execute()
    {
        HiveGrid grid = Object.FindAnyObjectByType<HiveGrid>();
        if (grid != null)
        {
            var method = typeof(HiveGrid).GetMethod("FixLibrary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(grid, null);
                Debug.Log("Successfully ran FixLibrary from command line!");
            }
            else Debug.LogError("Could not find FixLibrary method!");
        }
        else Debug.LogError("Could not find HiveGrid!");
        EditorApplication.Exit(0);
    }
}
