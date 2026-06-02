using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class Game
{
    // ---------- infrastructure
    private readonly GameWindow _window;
    private readonly InputSystem _input;
    private readonly TextRenderer _text;
    private readonly SceneRenderer _renderer;
    private readonly Button _restartButton;

    // ----------- game state
    private DoorSystem _doorSystem = null!;
    private MinigameRunner _minigameRunner = null!;
    // these are null at first cuz they're initialized later in startnewgame function
    private GameMode _mode;

    public Game(GameWindow window)
    {
        _window = window;
        _input = new InputSystem(window);
        _text = new TextRenderer("nullptr_hq4x.png", 8, 12);
        _renderer = new SceneRenderer(_text);
        _restartButton = new Button(-0.25f, -0.4f, 0.5f, 0.2f, "Restart", () => StartNewGame(), _text);

        StartNewGame();
    }

    private void StartNewGame()
    {
        _doorSystem = new DoorSystem();
        _minigameRunner = new MinigameRunner(new IMinigame[]
        {
            new ClickMinigame(5),
        });
        _mode = GameMode.Playing;
        GL.ClearColor(0.1f, 0.1f, 0.15f, 1f);
    }

    public void Run()
    {
        do
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            switch (_mode)
            {
                case GameMode.Playing:
                    UpdatePlaying(GameSetup.TimeDelta);
                    break;
                case GameMode.Won:
                    UpdateEndScreen("You Win!", new Color4(0f, 0.3f, 0f, 1f));
                    break;
                case GameMode.Lost:
                    UpdateEndScreen("You Lose!", new Color4(0.3f, 0f, 0f, 1f));
                    break;
            }
        } while (_window.NextFrame());
    }

    private void UpdatePlaying(float deltaTime)
    {
        if (_input.IsClickInRect(-1f, -1f, 0.25f, 2f)) _doorSystem.LeftDoor.Close();
        if (_input.IsClickInRect(0.75f, -1f, 0.25f, 2f)) _doorSystem.RightDoor.Close();

        _doorSystem.Update(deltaTime);
        _minigameRunner.Update(deltaTime, _input);

        if (_doorSystem.IsLosing) _mode = GameMode.Lost;
        if (_minigameRunner.AllCompleted) _mode = GameMode.Won;

        _renderer.DrawScene(_doorSystem.LeftDoor, _doorSystem.RightDoor, _minigameRunner.CurrentInstruction);
        _minigameRunner.Draw();
    }

    private void UpdateEndScreen(string message, Color4 background)
    {
        GL.ClearColor(background);
        _text.DrawCentered(message, 0f, 0.3f, 0.1f, Color4.White);
        _restartButton.Update(_input);
        _restartButton.Draw();
    }
}

public enum GameMode { Playing, Won, Lost }