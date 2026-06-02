using System;

public class DoorSystem
{
    public Door LeftDoor { get; } = new();
    public Door RightDoor { get; } = new();

    private readonly Random _random = new();
    private float _leftTimer;
    private float _rightTimer;

    private const float MinClosedTime = 5f;
    private const float MaxClosedTime = 20f;

    public DoorSystem()
    {
        _leftTimer = NextDelay();
        _rightTimer = NextDelay();
    }

    public bool IsLosing => LeftDoor.IsLosing || RightDoor.IsLosing;

    public void Update(float deltaTime)
    {
        _leftTimer = UpdateDoor(LeftDoor, _leftTimer, deltaTime);
        _rightTimer = UpdateDoor(RightDoor, _rightTimer, deltaTime);
    }

    private float UpdateDoor(Door door, float timer, float deltaTime)
    {
        if (door.State == DoorState.Closed)
        {
            timer -= deltaTime;
            if (timer <= 0f)
            {
                door.Open();
                timer = NextDelay();
            }
        }
        door.Update(deltaTime);
        return timer;
    }

    private float NextDelay()
        => MinClosedTime + (float)_random.NextDouble() * (MaxClosedTime - MinClosedTime);
}