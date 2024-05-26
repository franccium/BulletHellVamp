using Godot;
using System;

public partial class PickupManager : Node
{
    public static PickupManager Instance { get; private set; }

    [Export]
    private PackedScene _powerUpScene;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
    }

    public void SpawnPowerUp(Vector2 position, int value)
    {
        var powerUp = _powerUpScene.Instantiate() as PowerUp;
        powerUp.Position = position;
        powerUp.Value = value;
        AddChild(powerUp);
    }
}
