using OpenTK.Mathematics;

namespace IntrovertedVampire.Game;

// Pixel-space rectangle with top-left origin (0,0) at the window's top-left.
public struct Rect
{
    public float X, Y, RectangleWidth, RectangleHeight;
    public Rect(float x, float y, float width, float height) { X = x; Y = y; RectangleWidth = width; RectangleHeight = height; }
    public bool Contains(Vector2 area)
        => area.X >= X && area.X <= X + RectangleWidth && area.Y >= Y && area.Y <= Y + RectangleHeight;
}
