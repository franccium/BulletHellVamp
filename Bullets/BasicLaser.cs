using Godot;
using System;

public partial class BasicLaser : Laser
{
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void UpdateLaser(float delta)
    {
        ExtendLength(delta);

        MoveLaser(delta);
    }
}
