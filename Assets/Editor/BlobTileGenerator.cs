using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BlobTileGenerator : EditorWindow
{
    [MenuItem("Tools/Plan Bee/Blob Tile Generator")]
    public static void ShowWindow()
    {
        GetWindow<BlobTileGenerator>("Blob Generator");
    }

    private Texture2D centerBlock;

    private Texture2D edgeTop;
    private Texture2D edgeBottom;
    private Texture2D edgeLeft;
    private Texture2D edgeRight;

    private Texture2D outerCornerTL;
    private Texture2D outerCornerTR;
    private Texture2D outerCornerBL;
    private Texture2D outerCornerBR;

    private Texture2D innerCornerTL;
    private Texture2D innerCornerTR;
    private Texture2D innerCornerBL;
    private Texture2D innerCornerBR;

    private void OnGUI()
    {
        GUILayout.Label("47-Tile Blob Generator (32x32)", EditorStyles.boldLabel);
        GUILayout.Label("Assign your 13 base tiles here.", EditorStyles.wordWrappedLabel);

        centerBlock = (Texture2D)EditorGUILayout.ObjectField("Center (All Filled)", centerBlock, typeof(Texture2D), false);

        EditorGUILayout.Space();
        edgeTop = (Texture2D)EditorGUILayout.ObjectField("Edge Top", edgeTop, typeof(Texture2D), false);
        edgeBottom = (Texture2D)EditorGUILayout.ObjectField("Edge Bottom", edgeBottom, typeof(Texture2D), false);
        edgeLeft = (Texture2D)EditorGUILayout.ObjectField("Edge Left", edgeLeft, typeof(Texture2D), false);
        edgeRight = (Texture2D)EditorGUILayout.ObjectField("Edge Right", edgeRight, typeof(Texture2D), false);

        EditorGUILayout.Space();
        outerCornerTL = (Texture2D)EditorGUILayout.ObjectField("Outer Corner TL", outerCornerTL, typeof(Texture2D), false);
        outerCornerTR = (Texture2D)EditorGUILayout.ObjectField("Outer Corner TR", outerCornerTR, typeof(Texture2D), false);
        outerCornerBL = (Texture2D)EditorGUILayout.ObjectField("Outer Corner BL", outerCornerBL, typeof(Texture2D), false);
        outerCornerBR = (Texture2D)EditorGUILayout.ObjectField("Outer Corner BR", outerCornerBR, typeof(Texture2D), false);

        EditorGUILayout.Space();
        innerCornerTL = (Texture2D)EditorGUILayout.ObjectField("Inner Corner TL", innerCornerTL, typeof(Texture2D), false);
        innerCornerTR = (Texture2D)EditorGUILayout.ObjectField("Inner Corner TR", innerCornerTR, typeof(Texture2D), false);
        innerCornerBL = (Texture2D)EditorGUILayout.ObjectField("Inner Corner BL", innerCornerBL, typeof(Texture2D), false);
        innerCornerBR = (Texture2D)EditorGUILayout.ObjectField("Inner Corner BR", innerCornerBR, typeof(Texture2D), false);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate 47-Tile Spritesheet", GUILayout.Height(40)))
        {
            GenerateTiles();
        }
    }

    private void GenerateTiles()
    {
        if (centerBlock == null || innerCornerTL == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign all 13 textures first!", "OK");
            return;
        }

        List<int> validMasks = Get47ValidMasks();
        
        // Create an 8x6 spritesheet (48 slots, we use 47)
        int cols = 8;
        int rows = 6;
        int tileSize = 32;
        int quadSize = 16;
        
        Texture2D sheet = new Texture2D(cols * tileSize, rows * tileSize, TextureFormat.RGBA32, false);
        sheet.filterMode = FilterMode.Point;
        
        // Fill transparent
        Color[] clearCols = new Color[sheet.width * sheet.height];
        for (int i = 0; i < clearCols.Length; i++) clearCols[i] = Color.clear;
        sheet.SetPixels(clearCols);

        for (int i = 0; i < validMasks.Count; i++)
        {
            int mask = validMasks[i];
            
            bool t = (mask & 1) != 0;
            bool r = (mask & 2) != 0;
            bool b = (mask & 4) != 0;
            bool l = (mask & 8) != 0;
            bool tl = (mask & 16) != 0;
            bool tr = (mask & 32) != 0;
            bool bl = (mask & 64) != 0;
            bool br = (mask & 128) != 0;

            int gridX = i % cols;
            int gridY = rows - 1 - (i / cols); // start from top-left

            int startX = gridX * tileSize;
            int startY = gridY * tileSize;

            // Draw TL Quadrant
            Texture2D texTL = GetSourceTexture(t, l, tl, outerCornerTL, edgeTop, edgeLeft, innerCornerTL, centerBlock);
            CopyQuadrant(texTL, sheet, 0, 1, startX, startY + quadSize, quadSize);

            // Draw TR Quadrant
            Texture2D texTR = GetSourceTexture(t, r, tr, outerCornerTR, edgeTop, edgeRight, innerCornerTR, centerBlock);
            CopyQuadrant(texTR, sheet, 1, 1, startX + quadSize, startY + quadSize, quadSize);

            // Draw BL Quadrant
            Texture2D texBL = GetSourceTexture(b, l, bl, outerCornerBL, edgeBottom, edgeLeft, innerCornerBL, centerBlock);
            CopyQuadrant(texBL, sheet, 0, 0, startX, startY, quadSize);

            // Draw BR Quadrant
            Texture2D texBR = GetSourceTexture(b, r, br, outerCornerBR, edgeBottom, edgeRight, innerCornerBR, centerBlock);
            CopyQuadrant(texBR, sheet, 1, 0, startX + quadSize, startY, quadSize);
        }

        sheet.Apply();

        byte[] bytes = sheet.EncodeToPNG();
        string path = "Assets/05_Tiles/Generated_47_Blob.png";
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();

        // Make the imported texture readable and set to Point filter
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.SaveAndReimport();
        }

        EditorUtility.DisplayDialog("Success", "Generated 47-Tile Spritesheet at " + path + "\n\nYou can now slice it in the Sprite Editor!", "Awesome!");
    }

    private Texture2D GetSourceTexture(bool side1, bool side2, bool diag, Texture2D outer, Texture2D edge1, Texture2D edge2, Texture2D inner, Texture2D center)
    {
        if (!side1 && !side2) return outer;
        if (!side1 && side2) return edge2;
        if (side1 && !side2) return edge1;
        if (side1 && side2 && !diag) return inner;
        return center;
    }

    private void CopyQuadrant(Texture2D src, Texture2D dest, int qx, int qy, int destX, int destY, int size)
    {
        if (src == null) return;

        // Ensure texture is readable
        string path = AssetDatabase.GetAssetPath(src);
        TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp != null && !imp.isReadable)
        {
            imp.isReadable = true;
            imp.SaveAndReimport();
        }

        int srcX = qx * size;
        int srcY = qy * size;

        Color[] pixels = src.GetPixels(srcX, srcY, size, size);
        dest.SetPixels(destX, destY, size, size, pixels);
    }

    private List<int> Get47ValidMasks()
    {
        HashSet<int> uniqueMasks = new HashSet<int>();

        for (int i = 0; i < 256; i++)
        {
            bool t = (i & 1) != 0;
            bool r = (i & 2) != 0;
            bool b = (i & 4) != 0;
            bool l = (i & 8) != 0;
            bool tl = (i & 16) != 0;
            bool tr = (i & 32) != 0;
            bool bl = (i & 64) != 0;
            bool br = (i & 128) != 0;

            // Normalize diagonals: diagonal only matters if both adjacent sides are present
            if (!t || !l) tl = false;
            if (!t || !r) tr = false;
            if (!b || !l) bl = false;
            if (!b || !r) br = false;

            int mask = 0;
            if (t) mask |= 1;
            if (r) mask |= 2;
            if (b) mask |= 4;
            if (l) mask |= 8;
            if (tl) mask |= 16;
            if (tr) mask |= 32;
            if (bl) mask |= 64;
            if (br) mask |= 128;

            uniqueMasks.Add(mask);
        }

        List<int> sorted = uniqueMasks.ToList();
        sorted.Sort();
        return sorted;
    }
}
