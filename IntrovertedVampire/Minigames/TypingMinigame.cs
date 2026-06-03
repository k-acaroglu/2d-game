using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TypingMinigame : IMinigame
{
    private readonly int _requiredRounds;
    private int _rounds = 1;
    private char _target;
    private readonly TextRenderer _text;

    public bool IsCompleted => _rounds > _requiredRounds;
    public string Instruction => $"Press the correct letters!";

    public TypingMinigame(int rounds, TextRenderer text)
    {
        _requiredRounds = rounds;
        _text = text;
        _target = PickRandomLetter();
    }

    public void Update(float deltaTime, InputSystem input)
    {
        if (IsCompleted) return;
        if (input.IsKeyPressed((Keys)_target))
        {
            _rounds++;
            if (!IsCompleted)
                _target = PickRandomLetter();
        }
    }

    public void Draw()
    {
        // Big target letter centered on the monitor
        _text.DrawCentered(_target.ToString(), 0f, -0.08f, 0.18f, Color4.White);
        // Progress below it
        _text.DrawCentered($"{_rounds}/{_requiredRounds}", 0f, -0.15f, 0.05f, new Color4(0.7f, 0.7f, 0.7f, 1f));
    }

    private static char PickRandomLetter()
    {
        return (char)('A' + Random.Shared.Next(26));
    }
}