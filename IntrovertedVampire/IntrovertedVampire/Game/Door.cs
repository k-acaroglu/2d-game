using OpenTK.Mathematics;

namespace FnafLike.Game;

public class Door
{
    public string Name { get; }
    public Rect Bounds;
    public Vector3 Color;

    public Door(string name, Rect bounds, Vector3 color)
    {
        Name = name;
        Bounds = bounds;
        Color = color;
    }
}
