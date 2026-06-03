using Framework;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Zenseless.OpenTK;

public class SpriteRenderer
{
    private readonly Texture2D _texture;
    private readonly uint _columns;
    private readonly uint _rows;

    public SpriteRenderer (Texture2D texture, uint columns, uint rows)
    {
        _texture = texture;
        _columns = columns;
        _rows = rows;
    }

    // for doors, maps AnimationProgress (0=closed, 1=open) to the correct frame
    public void DrawProgress(Box2 bounds, float progress, bool flipX = false)
    {
        uint totalFrames = _columns * _rows;

        // progress * totalFrames gives us the current frame as a float, we cast to int to get the frame index, and clamp to valid range
        uint frame = (uint)Math.Clamp((int)(progress * totalFrames), 0, (int)totalFrames - 1);
        DrawFrame(bounds, frame, flipX);
    }

    // loops through all the frames
    public void DrawLooping(Box2 bounds, float time, float duration)
    {
        uint totalFrames = _columns * _rows;
        
        // time % duration / duration gives us the progress within the cycle, multiplied by totalFrames gives us the frame index
        uint frame = (uint)(time % duration / duration * totalFrames) % totalFrames;
        DrawFrame(bounds, frame, false);
    }

    // draws a specific frame (0-based index)
    private void DrawFrame(Box2 bounds, uint frame, bool flipX)
    {
        Box2 texCoords = SpriteSheetTools.CalcTexCoords(frame, _columns, _rows);
        float texLeft  = flipX ? texCoords.Max.X : texCoords.Min.X;
        float texRight = flipX ? texCoords.Min.X : texCoords.Max.X;

        GL.Enable(EnableCap.Texture2D);
        GL.Color4(Color4.White);
        _texture.Bind();

        GL.Begin(PrimitiveType.Quads);
        GL.TexCoord2(texLeft,  texCoords.Min.Y); GL.Vertex2(bounds.Min.X, bounds.Min.Y);
        GL.TexCoord2(texRight, texCoords.Min.Y); GL.Vertex2(bounds.Max.X, bounds.Min.Y);
        GL.TexCoord2(texRight, texCoords.Max.Y); GL.Vertex2(bounds.Max.X, bounds.Max.Y);
        GL.TexCoord2(texLeft,  texCoords.Max.Y); GL.Vertex2(bounds.Min.X, bounds.Max.Y);
        GL.End();

        GL.Disable(EnableCap.Texture2D);
    }
}