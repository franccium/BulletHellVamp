using Godot;

public partial class PickupItem : Node2D
{
    [Export]
    public int Value { get; set; }
    [Export]
    public float PickupDistance { get; set; } = 50f;
    [Export]
    public float PickupSpeed { get; set; } = 100f;

    protected Player _player;

    public override void _Ready()
    {
        _player = GetTree().Root.GetNode<Player>("Game/Player");
    }

    public override void _Process(double delta)
    {
        if (ScanForPlayer())
        {
            MoveTowardsPlayer((float)delta);
        }
    }

    public bool ScanForPlayer()
    {
        if (Position.DistanceTo(_player.Position) < PickupDistance * _player.PickupDistanceModifier)
        {
            return true;
        }
        return false;
    }

    public void MoveTowardsPlayer(float delta)
    {
        Vector2 direction = (_player.Position - Position).Normalized();
        Position += direction * PickupSpeed * delta;

        if (Position.DistanceTo(_player.Position) < 5f)
        {
            OnPickup();
        }
    }

    public virtual void OnPickup()
    {
        QueueFree();
    }
}