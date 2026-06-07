using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class GenerateAnimations : EditorWindow
{
    [MenuItem("Plan Bee/Generate Character Animations")]
    public static void GenerateAllAnimations()
    {
        string dir = "Assets/06_Animations";
        if (!AssetDatabase.IsValidFolder("Assets/06_Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "06_Animations");
        }

        // 1. Generate Bee Idle (Breathing scale)
        AnimationClip beeIdle = new AnimationClip { name = "Bee_Idle" };
        AnimationCurve idleScaleX = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.05f);
        idleScaleX.preWrapMode = WrapMode.PingPong;
        idleScaleX.postWrapMode = WrapMode.PingPong;
        AnimationCurve idleScaleY = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.95f);
        idleScaleY.preWrapMode = WrapMode.PingPong;
        idleScaleY.postWrapMode = WrapMode.PingPong;

        beeIdle.SetCurve("", typeof(Transform), "localScale.x", idleScaleX);
        beeIdle.SetCurve("", typeof(Transform), "localScale.y", idleScaleY);
        
        AnimationClipSettings idleSettings = AnimationUtility.GetAnimationClipSettings(beeIdle);
        idleSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(beeIdle, idleSettings);
        
        AssetDatabase.CreateAsset(beeIdle, $"{dir}/Bee_Idle.anim");

        // 2. Generate Bee Walk (Waddling rotation)
        AnimationClip beeWalk = new AnimationClip { name = "Bee_Walk" };
        AnimationCurve walkRotZ = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 15f),
            new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, -15f),
            new Keyframe(1f, 0f)
        );
        walkRotZ.preWrapMode = WrapMode.Loop;
        walkRotZ.postWrapMode = WrapMode.Loop;

        // Note: animating localEulerAngles requires specific property names in Unity
        beeWalk.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", walkRotZ);

        AnimationClipSettings walkSettings = AnimationUtility.GetAnimationClipSettings(beeWalk);
        walkSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(beeWalk, walkSettings);

        AssetDatabase.CreateAsset(beeWalk, $"{dir}/Bee_Walk.anim");

        // 3. Generate Mite Idle (Bounce)
        AnimationClip miteIdle = new AnimationClip { name = "Mite_Idle" };
        AnimationCurve miteBounceY = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.15f, 0.2f),
            new Keyframe(0.3f, 0f)
        );
        miteBounceY.preWrapMode = WrapMode.Loop;
        miteBounceY.postWrapMode = WrapMode.Loop;

        miteIdle.SetCurve("", typeof(Transform), "localPosition.y", miteBounceY);

        AnimationClipSettings miteSettings = AnimationUtility.GetAnimationClipSettings(miteIdle);
        miteSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(miteIdle, miteSettings);

        AssetDatabase.CreateAsset(miteIdle, $"{dir}/Mite_Idle.anim");

        // 4. Create Animator Controller for Bees
        AnimatorController beeController = AnimatorController.CreateAnimatorControllerAtPath($"{dir}/BeeController.controller");
        beeController.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        
        AnimatorState idleState = beeController.layers[0].stateMachine.AddState("Idle");
        idleState.motion = beeIdle;
        
        AnimatorState walkState = beeController.layers[0].stateMachine.AddState("Walk");
        walkState.motion = beeWalk;

        AnimatorStateTransition toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        toWalk.duration = 0.1f;

        AnimatorStateTransition toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        toIdle.duration = 0.1f;

        // 5. Create Animator Controller for Mites
        AnimatorController miteController = AnimatorController.CreateAnimatorControllerAtPath($"{dir}/MiteController.controller");
        miteController.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        AnimatorState mIdleState = miteController.layers[0].stateMachine.AddState("Idle");
        mIdleState.motion = miteIdle;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Animations generated in Assets/06_Animations!");
    }
}
