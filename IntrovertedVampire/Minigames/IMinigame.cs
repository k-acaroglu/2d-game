public interface IMinigame
{
    string Instruction { get; }
    bool IsCompleted { get; }
    void Update(float deltaTime, InputSystem input);
    void Draw();
}