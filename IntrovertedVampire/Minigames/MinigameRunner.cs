public class MinigameRunner
{
    private readonly IMinigame[] _minigames;
    private int _currentIndex = 0;

    public bool AllCompleted => _currentIndex >= _minigames.Length;

    // returns the instruction for the current minigame, or an empty string if all are completed.
    public string CurrentInstruction => AllCompleted ? string.Empty : _minigames[_currentIndex].Instruction;

    public MinigameRunner(IMinigame[] minigames)
    {
        _minigames = Shuffle(minigames);
    }

    private static IMinigame[] Shuffle(IMinigame[] source)
    {
        IMinigame[] arr = (IMinigame[])source.Clone();
        Random.Shared.Shuffle(arr);
        return arr;
    }

    public void Update(float deltaTime, InputSystem input)
    {
        if (AllCompleted) return;

        var current = _minigames[_currentIndex];
        current.Update(deltaTime, input);

        if (current.IsCompleted) _currentIndex++;
    }

    public void Draw()
    {
        if (AllCompleted) return;
        _minigames[_currentIndex].Draw();
    }
}