using UnityEditor;
using UnityEngine;

public static class StripRotation
{
    [MenuItem("Plan-Bee/Strip Rotation from Animations")]
    public static void Execute()
    {
        string[] clips = new string[] 
        {
            "Assets/06_Animations/Bee_Walk.anim"
        };

        foreach (var path in clips)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                EditorCurveBinding rotBinding = new EditorCurveBinding();
                rotBinding.type = typeof(Transform);
                rotBinding.path = "";
                rotBinding.propertyName = "localEulerAnglesRaw.z";

                AnimationUtility.SetEditorCurve(clip, rotBinding, null);
                EditorUtility.SetDirty(clip);
                Debug.Log("Stripped rotation from " + path);
            }
        }
        AssetDatabase.SaveAssets();
    }
}
