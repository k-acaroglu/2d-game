using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class InputSystem
{
    private readonly GameWindow _window;

    public InputSystem(GameWindow window)
    {
        _window = window;
    }

    // Returns mouse position normalized to [-1, 1] range, matching OpenGL's coordinate system
    public Vector2 MousePosition
    {
        get
        {
            var pixel = _window.MousePosition;
            var size = _window.ClientSize;
            float x = (pixel.X / size.X) * 2f - 1f;
            float y = 1f - (pixel.Y / size.Y) * 2f; // Y is flipped: OpenGL's Y goes up, window pixels go down
            return new Vector2(x, y);
        }
    }

    public bool IsMouseClicked => _window.IsMouseButtonPressed(MouseButton.Left);
    public bool IsClickInRect(float x, float y, float width, float height)
    {
        var pos = MousePosition;
        return IsMouseClicked &&
            pos.X >= x && pos.X <= x + width &&
            pos.Y >= y && pos.Y <= y + height;
    }
}