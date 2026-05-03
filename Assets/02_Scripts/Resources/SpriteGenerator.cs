using UnityEngine;

/// <summary>
/// Runtime sprite generator for placeholder visuals.
/// Creates simple colored squares for bees and tiles.
/// </summary>
public static class SpriteGenerator
{
    public static Sprite CreateColoredSquare(Color color, int size = 32)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            16f
        );

        return sprite;
    }

    public static Sprite GetBeeSprite(Bee.BeeType type)
    {
        return type switch
        {
            Bee.BeeType.Queen => CreateColoredSquare(new Color(1f, 0.2f, 0.2f, 1f)),    // Red
            Bee.BeeType.Nurse => CreateColoredSquare(new Color(1f, 1f, 0f, 1f)),        // Yellow
            Bee.BeeType.Builder => CreateColoredSquare(new Color(0f, 1f, 1f, 1f)),      // Cyan
            Bee.BeeType.Drone => CreateColoredSquare(new Color(0.5f, 0.5f, 1f, 1f)),    // Blue
            Bee.BeeType.Forager => CreateColoredSquare(new Color(1f, 0.8f, 0f, 1f)),    // Orange
            Bee.BeeType.House => CreateColoredSquare(new Color(0f, 1f, 0.5f, 1f)),      // Green
            _ => CreateColoredSquare(Color.white)
        };
    }

    public static Sprite GetHiveTileSprite(TileState state)
    {
        return state switch
        {
            TileState.Solid => CreateColoredSquare(new Color(0.5f, 0.3f, 0.1f, 1f)),           // Dark brown
            TileState.Passage => CreateColoredSquare(new Color(0.7f, 0.5f, 0.2f, 1f)),         // Light brown
            TileState.Room => CreateColoredSquare(new Color(0.8f, 0.6f, 0.3f, 1f)),            // Golden brown
            TileState.MarkedForSolid => CreateColoredSquare(new Color(1f, 1f, 0.5f, 0.5f)),    // Yellow transparent
            TileState.MarkedForPassage => CreateColoredSquare(new Color(0.5f, 1f, 0.5f, 0.5f)), // Green transparent
            TileState.MarkedForRoom => CreateColoredSquare(new Color(0.5f, 0.5f, 1f, 0.5f)),   // Blue transparent
            _ => CreateColoredSquare(Color.white)
        };
    }
}
