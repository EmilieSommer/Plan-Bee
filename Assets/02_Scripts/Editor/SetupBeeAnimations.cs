using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SetupBeeAnimations
{
    [MenuItem("Plan-Bee/Setup Unique Bee Animations")]
    public static void Execute()
    {
        string baseControllerPath = "Assets/06_Animations/BeeController.controller";
        var baseController = AssetDatabase.LoadAssetAtPath<AnimatorController>(baseControllerPath);
        if (baseController == null)
        {
            Debug.LogError("Could not find BeeController at " + baseControllerPath);
            return;
        }

        AnimationClip baseWalkClip = null;
        foreach (var clip in baseController.animationClips)
        {
            if (clip.name == "Bee_Walk")
            {
                baseWalkClip = clip;
                break;
            }
        }

        if (baseWalkClip == null)
        {
            Debug.LogError("Could not find Bee_Walk in the BeeController!");
            return;
        }

        // Strip rotation from base clip so worker bee also rotates correctly
        EditorCurveBinding rotBindingBase = new EditorCurveBinding();
        rotBindingBase.type = typeof(Transform);
        rotBindingBase.path = "";
        rotBindingBase.propertyName = "localEulerAnglesRaw.z";
        AnimationUtility.SetEditorCurve(baseWalkClip, rotBindingBase, null);
        EditorUtility.SetDirty(baseWalkClip);

        string[] beeNames = new string[] { "Builder bee", "Drone", "Forager bee", "Nurse bee", "The Queen", "RobbeBee", "Wasp" };
        string[] prefabPaths = new string[] 
        {
            "Assets/03_Prefabs/Bees/BuilderBee.prefab",
            "Assets/03_Prefabs/Bees/DroneBee.prefab",
            "Assets/03_Prefabs/Bees/ForagerBee.prefab",
            "Assets/03_Prefabs/Bees/NurseBee.prefab",
            "Assets/03_Prefabs/Bees/QueenBee.prefab",
            "Assets/03_Prefabs/TestPrefabs/RobberBee.prefab",
            "Assets/03_Prefabs/TestPrefabs/Wasp.prefab"
        };

        if (!AssetDatabase.IsValidFolder("Assets/06_Animations/Overrides"))
        {
            AssetDatabase.CreateFolder("Assets/06_Animations", "Overrides");
        }

        for (int i = 0; i < beeNames.Length; i++)
        {
            string beeName = beeNames[i];
            string prefabPath = prefabPaths[i];
            
            // Check if sheet is in PNGS or root Bees folder
            string sheetPath = $"Assets/04_Sprites/Bees/PNGS/{beeName}_WalkSheet.png";
            if (!File.Exists(sheetPath))
            {
                sheetPath = $"Assets/04_Sprites/Bees/{beeName}_WalkSheet.png";
            }

            // 1. Slice the spritesheet
            TextureImporter importer = AssetImporter.GetAtPath(sheetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = 32;
                
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
                int frameWidth = tex.width / 4;
                int frameHeight = tex.height;

                SpriteMetaData[] metaData = new SpriteMetaData[4];
                for (int j = 0; j < 4; j++)
                {
                    metaData[j] = new SpriteMetaData
                    {
                        name = $"{beeName}_Walk_{j}",
                        rect = new Rect(j * frameWidth, 0, frameWidth, frameHeight),
                        alignment = 0,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                }
                importer.spritesheet = metaData;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            // 2. Load the sprites
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            List<Sprite> sprites = new List<Sprite>();
            foreach (var asset in assets)
            {
                if (asset is Sprite s) sprites.Add(s);
            }
            sprites.Sort((a, b) => a.name.CompareTo(b.name));

            if (sprites.Count < 4)
            {
                Debug.LogError($"Not enough sprites sliced for {beeName}");
                continue;
            }

            // 3. Create Animation Clip
            AnimationClip newClip = new AnimationClip();
            newClip.name = $"{beeName}_Walk";
            newClip.frameRate = 8;
            
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(newClip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(newClip, settings);

            EditorCurveBinding spriteBinding = new EditorCurveBinding();
            spriteBinding.type = typeof(SpriteRenderer);
            spriteBinding.path = "";
            spriteBinding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[9];
            spriteKeyFrames[0] = new ObjectReferenceKeyframe { time = 0f, value = sprites[0] };
            spriteKeyFrames[1] = new ObjectReferenceKeyframe { time = 0.125f, value = sprites[1] };
            spriteKeyFrames[2] = new ObjectReferenceKeyframe { time = 0.25f, value = sprites[2] };
            spriteKeyFrames[3] = new ObjectReferenceKeyframe { time = 0.375f, value = sprites[3] };
            spriteKeyFrames[4] = new ObjectReferenceKeyframe { time = 0.5f, value = sprites[0] };
            spriteKeyFrames[5] = new ObjectReferenceKeyframe { time = 0.625f, value = sprites[1] };
            spriteKeyFrames[6] = new ObjectReferenceKeyframe { time = 0.75f, value = sprites[2] };
            spriteKeyFrames[7] = new ObjectReferenceKeyframe { time = 0.875f, value = sprites[3] };
            spriteKeyFrames[8] = new ObjectReferenceKeyframe { time = 1f, value = sprites[0] };

            AnimationUtility.SetObjectReferenceCurve(newClip, spriteBinding, spriteKeyFrames);

            // Removed rotation curve to allow scripts to control rotation

            string clipPath = $"Assets/06_Animations/Overrides/{beeName}_Walk.anim";
            AssetDatabase.CreateAsset(newClip, clipPath);

            // 4. Create Override Controller
            AnimatorOverrideController overrideController = new AnimatorOverrideController(baseController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
            for (int j = 0; j < overrides.Count; j++)
            {
                if (overrides[j].Key.name == "Bee_Walk")
                {
                    overrides[j] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[j].Key, newClip);
                }
            }
            overrideController.ApplyOverrides(overrides);

            string overridePath = $"Assets/06_Animations/Overrides/{beeName}_Controller.overrideController";
            AssetDatabase.CreateAsset(overrideController, overridePath);

            // 5. Apply to Prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Animator anim = instance.GetComponent<Animator>();
                if (anim == null) anim = instance.AddComponent<Animator>();
                anim.runtimeAnimatorController = overrideController;
                
                SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = sprites[0];

                instance.transform.localScale = Vector3.one;

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);
                Debug.Log($"Setup complete for {beeName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished setting up all unique bee animations!");
    }
}
