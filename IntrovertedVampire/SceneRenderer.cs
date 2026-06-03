using Framework;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class SceneRenderer
{
    private readonly TextRenderer _text;
    private readonly SpriteRenderer _doorSprite;
    private readonly SpriteRenderer _monitorSprite;

    public SceneRenderer(TextRenderer text)
    {
        _text = text;
        _doorSprite = new SpriteRenderer(EmbeddedResource.LoadTexture("door.png"), 5, 1); // 5 rows 1 column
        _monitorSprite = new SpriteRenderer(EmbeddedResource.LoadTexture("monitor.png"), 1, 1); // 1 row 1 column, image
    }

    public void DrawScene(Door leftDoor, Door rightDoor, string instruction)
    {
        // Left door — progress 0=closed, 1=open maps directly to animation frame
        _doorSprite.DrawProgress(new Box2(-1f, -1f, -0.75f, 1f), leftDoor.AnimationProgress);

        // Right door — same sprite, mirrored so it opens the opposite direction
        _doorSprite.DrawProgress(new Box2(0.75f, -1f, 1f, 1f), rightDoor.AnimationProgress, flipX: true);

        // Monitor — single frame static image
        // box2 is left - bottom - right - top
        _monitorSprite.DrawProgress(new Box2(-0.5f, -0.6f, 0.5f, 0.4f), 0f);

        // Instruction text — white now, since it sits on the dark game background
        _text.DrawCentered(instruction, 0f, 0.9f, 0.04f, Color4.White);
    }

    public void DrawRect(float x, float y, float width, float height)
    {
        GL.Begin(PrimitiveType.Quads);
        GL.Vertex2(x, y);
        GL.Vertex2(x + width, y);
        GL.Vertex2(x + width, y + height);
        GL.Vertex2(x, y + height);
        GL.End();
    }
}