using Godot;
using System;

public partial class PowerUp : PickupItem
{
    public override void _Ready()
    {
        base._Ready();
        PickupDistance = 300f;
        PickupSpeed = 200f;
    }

    public override void OnPickup()
    {
        _player.IncreasePower(Value);
        base.OnPickup();
    }
}
