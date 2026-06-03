public enum DoorState { Closed, Opening, Open, Closing }
public class Door
{
    public DoorState State { get; private set; } = DoorState.Closed;

    public float AnimationProgress { get; private set; } = 0f; // 0 = fully closed, 1 = fully open
    
    private float _openTimer = 0f; // how long the door has been open
    private const float AnimationDuration = 0.5f; // seconds to fully open or close
    private const float LoseThreshold = 3f; // seconds before the player loses

    public bool IsLosing => State == DoorState.Open && _openTimer >= LoseThreshold;

    public void Open()
    {
        if (State == DoorState.Closed) State = DoorState.Opening;
    }

    public void Close()
    {
        if (State == DoorState.Open) State = DoorState.Closing;
    }

    public void Update(float deltaTime)
    {
        switch (State)
        {
            case DoorState.Opening:
                AnimationProgress += deltaTime / AnimationDuration;
                if (AnimationProgress >= 1f)
                {
                    AnimationProgress = 1f;
                    State = DoorState.Open;
                    _openTimer = 0f;
                }
                break;

            case DoorState.Open:
                _openTimer += deltaTime;
                break;

            case DoorState.Closing:
                AnimationProgress -= deltaTime / AnimationDuration;
                if (AnimationProgress <= 0f)
                {
                    AnimationProgress = 0f;
                    State = DoorState.Closed;
                }
                break;
        }
    }
}