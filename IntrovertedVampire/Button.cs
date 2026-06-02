using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class Button
{
    private readonly float _x, _y, _width, _height;
    private readonly string _label;
    private readonly System.Action _onClick; // System.Action is essentially just a function.
    // An example could be System.Action doSomething = () => Console.WriteLine("Hello");
    private readonly TextRenderer _text;

    public Button(float x, float y, float width, float height, string label, System.Action onClick, TextRenderer text)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _label = label;
        _onClick = onClick;
        _text = text;
    }

    public void Update(InputSystem input)
    {
        if (input.IsClickInRect(_x, _y, _width, _height))
            _onClick();
    }

    public void Draw()
    {
        // Button background
        GL.Color4(Color4.DarkGray);
        GL.Begin(PrimitiveType.Quads);
        GL.Vertex2(_x, _y);
        GL.Vertex2(_x + _width, _y);
        GL.Vertex2(_x + _width, _y + _height);
        GL.Vertex2(_x, _y + _height);
        GL.End();

        // Centered label
        float centerX = _x + _width / 2f;
        float labelY = _y + (_height - 0.05f) / 2f;
        _text.DrawCentered(_label, centerX, labelY, 0.05f, Color4.White);
    }
}