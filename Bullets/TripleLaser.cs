using Godot;
using System;

public partial class TripleLaser : MultipleBeamLaser
{
    public override void _Ready()
    {
        InitialSetup();

        BeamCount = 3;
        BeamAngle = 30f;
        SetupBeams(this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }


    public override void SetupBeams(Laser laser)
    {
        _beams = new Laser[BeamCount]; //todo unoptimised and not utilising pool
        GD.Print("Setting up beams");/*
        for (int i = 0; i < BeamCount; i++)
        {
            Laser newLaser = LaserPool.Instance.GetLaser(LaserPattern.Single);
            //AddChild(newLaser);
            newLaser.RotationDegrees = laser.RotationDegrees + (i - 1) * BeamAngle;
            newLaser.Position = laser.Position;
            newLaser.Speed = laser.Speed;
            newLaser.SetActive();
        }
        LaserPool.Instance.ReturnLaser(laser.LaserPattern, laser);*/

        for (int i = 0; i < BeamCount; i++)
        {
            Laser newLaser = LaserPool.Instance.GetEntityLaser(LaserPattern.Single, _laserSource);
            //AddChild(newLaser);

            //newLaser.Direction = (laser.Direction + new Vector2((i - 1) * 150f, 0)).Normalized();
            //newLaser.Rotation = laser.Rotation + (i - 1) * Mathf.DegToRad(BeamAngle);
            Vector2 directionShift = laser.Direction.Rotated(Mathf.DegToRad((i - 1) * BeamAngle));
            //newLaser.Direction = laser.Direction + (i - 1) * directionShift;
            //newLaser.Direction = directionShift;
            //newLaser.Rotation = Mathf.DegToRad((i - 1) * BeamAngle);

            newLaser.SetDirection(directionShift);

            _beams[i] = newLaser;
            newLaser.SetNotActive();
        }
        LaserPool.Instance.ReturnEntityLaser(laser, _laserSource);
    }

    public override void ShootBeams(Entity laser_source, Vector2 position,
        Vector2 direction, float initialLength,
        float maxLength, float width, float speed, int damage)
    {
        for (int i = 0; i < BeamCount; i++)
        {
            Vector2 directionShift = direction.Rotated(Mathf.DegToRad((i - 1) * BeamAngle));
            _beams[i].SetLaser(laser_source, position, directionShift, initialLength, maxLength - 30f, width, speed - 30f, damage);
            _beams[i].SetActive();
            GD.Print(i + " " + _beams[i].Direction + " " + _beams[i].RotationDegrees);
        }

        GD.Print("shoot beams in TripleLaser");
    }


    public override void UpdateLaser(float delta)
    {
        foreach (Laser beam in _beams)
        {
            ExtendLength(delta, beam);
            MoveLaser(delta, beam);
        }
    }
}
