using Framework;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Zenseless.OpenTK;

public class TextRenderer
{
    private readonly Texture2D _texture;
    private readonly uint _columns;
    private readonly uint _rows;

    public TextRenderer(string fontResourceName, uint columns, uint rows)
    {
        _texture = EmbeddedResource.LoadTexture(fontResourceName);
        _columns = columns;
        _rows = rows;

        // Blend lets the transparent background of each glyph show through
        // instead of drawing a solid box around every letter.
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public float MeasureWidth(string text, float size) => text.Length * size;

    public void Draw(string text, float x, float y, float size, Color4 color)
    {
        GL.Enable(EnableCap.Texture2D);   // turn texturing ON only while drawing text
        _texture.Bind();
        GL.Color4(color);

        const uint firstCharacter = 32; // ASCII of the first glyph in the sheet (space)
        int i = 0;
        foreach (var spriteId in SpriteSheetTools.StringToSpriteIds(text, firstCharacter))
        {
            Box2 rect = new(x + i * size, y, x + (i + 1) * size, y + size);
            Box2 texCoords = Framework.SpriteSheetTools.CalcTexCoords(spriteId, _columns, _rows);
            Framework.Draw.Rectangle(rect, texCoords);
            i++;
        }

        GL.Disable(EnableCap.Texture2D);  // turn it back OFF so plain-colored shapes render correctly
    }

    public void DrawCentered(string text, float centerX, float y, float size, Color4 color)
    {
        float width = MeasureWidth(text, size);
        Draw(text, centerX - width / 2f, y, size, color);
    }
}