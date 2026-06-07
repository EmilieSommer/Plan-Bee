using UnityEditor;
using UnityEngine;

public class SpriteSlicer
{
    [MenuItem("Tools/Plan Bee/Slice Grass Sprites")]
    public static void SliceGrass()
    {
        string[] paths = {
            "Assets/04_Sprites/Grass_base.png",
            "Assets/04_Sprites/Grass_base_2.png",
            "Assets/04_Sprites/Grass_base_3.png"
        };

        foreach (string path in paths)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.isReadable = true;
                ti.spriteImportMode = SpriteImportMode.Multiple;
                
                SpriteMetaData[] metaData = new SpriteMetaData[2];
                
                // Frame 0 (Left side: 0 to 32)
                metaData[0] = new SpriteMetaData { 
                    name = ti.name + "_0", 
                    rect = new Rect(0, 0, 32, 32), 
                    alignment = 9, // Custom pivot
                    pivot = new Vector2(0.5f, 0.5f) 
                };
                
                // Frame 1 (Right side: 32 to 64)
                metaData[1] = new SpriteMetaData { 
                    name = ti.name + "_1", 
                    rect = new Rect(32, 0, 32, 32), 
                    alignment = 9, 
                    pivot = new Vector2(0.5f, 0.5f) 
                };
                
                ti.spritesheet = metaData;
                ti.SaveAndReimport();
            }
        }
        Debug.Log("Grass sprites successfully sliced into 32x32 frames!");
    }
}
