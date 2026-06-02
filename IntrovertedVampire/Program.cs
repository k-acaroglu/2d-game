using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;

GameWindow window = GameSetup.Create("Introverted Vampire");
Game game = new(window);
game.Run();