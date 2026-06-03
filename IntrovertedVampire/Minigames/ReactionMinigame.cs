using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ReactionMinigame : IMinigame
{
    private readonly int _requiredHits;
    private int _hits = 0;
    private float _markerPos;
    private float _markerDir = 1f;
    private float _zoneCenter;

    private readonly float _markerSpeed = 0.8f;
    private readonly float _zoneHalfWidth = 0.12f;
    private readonly float _trackLeft = -0.25f;
    private readonly float _trackRight = 0.25f;
    private readonly float _trackY = 0;
    private readonly float _trackHeight = 0.1f;
    private readonly float _markerWidth = 0.04f;

    public bool IsCompleted => _hits >= _requiredHits;
    public string Instruction => "Press SPACE in the green zone!";

    public ReactionMinigame(int requiredHits)
    {
        _requiredHits = requiredHits;
        _markerPos = Random.Shared.NextSingle();
        _zoneCenter = RandomZoneCenter();
    }

    public void Update(float deltaTime, InputSystem input)
    {
        if (IsCompleted) return;

        _markerPos += _markerDir * _markerSpeed * deltaTime;
        if (_markerPos >= 1f) { _markerPos = 1f; _markerDir = -1f; }
        if (_markerPos <= 0f) { _markerPos = 0f; _markerDir = 1f; }

        if (input.IsKeyPressed(Keys.Space))
        {
            if (Math.Abs(_markerPos - _zoneCenter) <= _zoneHalfWidth)
            {
                _hits++;
                _zoneCenter = RandomZoneCenter();
            }
        }
    }

    public void Draw()
    {
        float trackWidth = _trackRight - _trackLeft;

        // Track (gray background)
        GL.Color3(0.7f, 0.7f, 0.7f);
        DrawRect(_trackLeft, _trackY, trackWidth, _trackHeight);

        // Target zone (green)
        float zoneLeft = _trackLeft + (_zoneCenter - _zoneHalfWidth) * trackWidth;
        float zoneWidth = _zoneHalfWidth * 2f * trackWidth;
        GL.Color3(0f, 0.8f, 0f);
        DrawRect(zoneLeft, _trackY, zoneWidth, _trackHeight);

        // Marker (orange)
        float markerLeft = _trackLeft + _markerPos * trackWidth - _markerWidth / 2f;
        GL.Color3(1f, 0.5f, 0f);
        DrawRect(markerLeft, _trackY, _markerWidth, _trackHeight);

        // Progress pips
        const float pipSize = 0.04f;
        const float pipSpacing = 0.07f;
        float pipsStartX = -(_requiredHits - 1) * pipSpacing / 2f;
        float pipY = -0.16f;
        for (int i = 0; i < _requiredHits; i++)
        {
            if (i < _hits) GL.Color3(0f, 0.8f, 0f);
            else GL.Color3(0.5f, 0.5f, 0.5f);
            DrawRect(pipsStartX + i * pipSpacing - pipSize / 2f, pipY, pipSize, pipSize);
        }
    }

    private static void DrawRect(float x, float y, float width, float height)
    {
        GL.Begin(PrimitiveType.Quads);
        GL.Vertex2(x, y);
        GL.Vertex2(x + width, y);
        GL.Vertex2(x + width, y + height);
        GL.Vertex2(x, y + height);
        GL.End();
    }

    private float RandomZoneCenter()
    {
        return _zoneHalfWidth + Random.Shared.NextSingle() * (1f - 2f * _zoneHalfWidth);
    }
}