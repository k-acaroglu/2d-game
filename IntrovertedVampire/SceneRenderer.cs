using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class SceneRenderer
{
    private readonly TextRenderer _text;

    public SceneRenderer(TextRenderer text)
    {
        _text = text;
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

    public void DrawScene(Door leftDoor, Door rightDoor, string instruction)
    {
        // Left door
        float leftOffset = -leftDoor.AnimationProgress * 0.25f;
        GL.Color3(1f, 1f, 1f);
        DrawRect(-1f + leftOffset, -1f, 0.25f, 2f);

        // Right door
        float rightOffset = rightDoor.AnimationProgress * 0.25f;
        GL.Color3(1f, 1f, 1f);
        DrawRect(0.75f + rightOffset, -1f, 0.25f, 2f);

        // Computer monitor background
        GL.Color3(1f, 1f, 1f);
        DrawRect(-0.3f, -0.3f, 0.6f, 0.5f);

        // Instruction text, centered on the monitor
        _text.DrawCentered(instruction, 0f, 0.9f, 0.04f, Color4.Black);
    }
}