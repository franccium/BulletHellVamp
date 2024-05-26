using System;
using BulletHellVamp;
using Godot;

public abstract class LaserPatterns
{
    public float LaserSpeed { get; set; } = 0f;

    public virtual void SetupBeams(Laser laser)
    {

    }
}

public class SingleLaserPattern : LaserPatterns
{

}


public class MultipleBeamPattern : LaserPatterns
{
    public float BeamCount { get; set; } = 0f;
    public float BeamAngle { get; set; } = 0f;

    public override void SetupBeams(Laser laser)
    {
        //todo set shapecasts
        //todo set particles their directions etc
    }
}


