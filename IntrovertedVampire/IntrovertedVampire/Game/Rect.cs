using OpenTK.Mathematics;

namespace FnafLike.Game;

// Pixel-space rectangle with top-left origin (0,0) at the window's top-left.
public struct Rect
{
    public float X, Y, RectangleWidth, RectangleHeight;
    public Rect(float x, float y, float w, float h) { X = x; Y = y; RectangleWidth = w; RectangleHeight = h; }
    public bool Contains(Vector2 p)
        => p.X >= X && p.X <= X + RectangleWidth && p.Y >= Y && p.Y <= Y + RectangleHeight;
}
