using Godot;
using System;

public partial class StarWigglyLaser : WigglyLaser
{
    public override void _Ready()
    {
        InitialSetup();

        BeamCount = 5;
        BeamAngle = 2 * Mathf.Pi / BeamCount;
        SetupBeams(this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
