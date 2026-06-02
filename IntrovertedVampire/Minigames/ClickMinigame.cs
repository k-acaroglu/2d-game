using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class ClickMinigame : IMinigame
{
    private readonly int _requiredClicks;
    private int _clickCount = 0;
    private readonly float _buttonX = -0.2f;
    private readonly float _buttonY = -0.15f;
    private readonly float _buttonWidth = 0.4f;
    private readonly float _buttonHeight = 0.3f;

    public bool IsCompleted => _clickCount >= _requiredClicks;
    public string Instruction => $"Click the button {_requiredClicks} times!";

    public ClickMinigame(int requiredClicks = 5)
    {
        _requiredClicks = requiredClicks;
    }

    public void Update(float deltaTime, InputSystem input)
    {
        if (IsCompleted) return;

        if (input.IsMouseClicked && IsMouseOver(input.MousePosition))
        {
            _clickCount++;
        }
    }

    public void Draw()
    {
        // Draw button, color shifts from red to green as progress increases
        float progress = (float)_clickCount / _requiredClicks;
        GL.Color3(1f - progress, progress, 0f);
        DrawButton();
    }

    private bool IsMouseOver(OpenTK.Mathematics.Vector2 mousePos)
    {
        return mousePos.X >= _buttonX && mousePos.X <= _buttonX + _buttonWidth &&
               mousePos.Y >= _buttonY && mousePos.Y <= _buttonY + _buttonHeight;
    }

    private void DrawButton()
    {
        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Quads);
        GL.Vertex2(_buttonX, _buttonY);
        GL.Vertex2(_buttonX + _buttonWidth, _buttonY);
        GL.Vertex2(_buttonX + _buttonWidth, _buttonY + _buttonHeight);
        GL.Vertex2(_buttonX, _buttonY + _buttonHeight);
        GL.End();
    }
}