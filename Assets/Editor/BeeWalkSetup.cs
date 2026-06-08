using UnityEditor;
using UnityEngine;

public class BeeWalkSetup
{
    [MenuItem("Tools/Plan Bee/Setup Bee Walk Animation")]
    public static void Setup()
    {
        string spritePath = "Assets/04_Sprites/Bees/PNGS/Bee_WalkSheet.png";
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
        if (tex == null)
        {
            Debug.LogError("Sprite sheet not found at " + spritePath);
            return;
        }

        TextureImporter ti = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (ti != null)
        {
            ti.isReadable = true;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spritePixelsPerUnit = 32f; // 32 PPU on a 32x32 image = exactly 1 unit wide!

            // Get actual dimensions
            int width = 0;
            int height = 0;
            ti.GetSourceTextureWidthAndHeight(out width, out height);
            
            if (width == 0 || height == 0) {
                // fallback
                width = tex.width;
                height = tex.height;
            }

            int frameWidth = width / 4;
            SpriteMetaData[] metaData = new SpriteMetaData[4];
            for (int i = 0; i < 4; i++)
            {
                metaData[i] = new SpriteMetaData {
                    name = "Bee_WalkSheet_" + i,
                    rect = new Rect(i * frameWidth, 0, frameWidth, height),
                    alignment = 9,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }

            ti.spritesheet = metaData;
            ti.SaveAndReimport();
        }

        // Now load the sliced sprites
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        Sprite[] frames = new Sprite[4];
        int frameIndex = 0;
        foreach (Object asset in assets)
        {
            if (asset is Sprite)
            {
                if (frameIndex < 4) frames[frameIndex] = (Sprite)asset;
                frameIndex++;
            }
        }

        if (frames[0] == null)
        {
            Debug.LogError("Failed to slice sprites.");
            return;
        }

        // Now update the Animation Clip
        string animPath = "Assets/06_Animations/Bee_Walk.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
        if (clip == null)
        {
            Debug.LogError("Walk animation not found at " + animPath);
            return;
        }

        // We'll run the walk cycle at 8 FPS
        clip.frameRate = 8;
        
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[5];
        // Frame 0
        spriteKeyFrames[0] = new ObjectReferenceKeyframe();
        spriteKeyFrames[0].time = 0f;
        spriteKeyFrames[0].value = frames[0];
        
        // Frame 1
        spriteKeyFrames[1] = new ObjectReferenceKeyframe();
        spriteKeyFrames[1].time = 1f / 8f;
        spriteKeyFrames[1].value = frames[1];
        
        // Frame 2
        spriteKeyFrames[2] = new ObjectReferenceKeyframe();
        spriteKeyFrames[2].time = 2f / 8f;
        spriteKeyFrames[2].value = frames[2];
        
        // Frame 3
        spriteKeyFrames[3] = new ObjectReferenceKeyframe();
        spriteKeyFrames[3].time = 3f / 8f;
        spriteKeyFrames[3].value = frames[3];

        // Frame 4 (loop back to start so it holds the last frame until 0.5s)
        spriteKeyFrames[4] = new ObjectReferenceKeyframe();
        spriteKeyFrames[4].time = 4f / 8f;
        spriteKeyFrames[4].value = frames[0];

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);

        // Adjust animation length if necessary by adding a dummy curve or adjusting existing ones, 
        // but the wobble is 1 second long, so we can just let it loop the sprite twice per wobble!
        // To loop sprite twice per wobble (which is 1s at 8fps = 8 frames):
        ObjectReferenceKeyframe[] loopedKeyFrames = new ObjectReferenceKeyframe[9];
        for(int i=0; i<8; i++) {
            loopedKeyFrames[i] = new ObjectReferenceKeyframe();
            loopedKeyFrames[i].time = i / 8f;
            // 0,1,2,3, 0,1,2,3
            loopedKeyFrames[i].value = frames[i % 4];
        }
        loopedKeyFrames[8] = new ObjectReferenceKeyframe();
        loopedKeyFrames[8].time = 1f; // end of 1 second wobble
        loopedKeyFrames[8].value = frames[0];

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, loopedKeyFrames);

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();

        Debug.Log("Bee walk animation successfully updated with moving legs!");
    }
}
