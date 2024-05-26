using Godot;

public partial class MultipleBeamLaser : Laser
{
    public int BeamCount { get; set; } = 0;
    public float BeamAngle { get; set; } = 0f;
    protected Laser[] _beams;

    public virtual void SetupBeams(Laser laser)
    {
        _beams = new Laser[BeamCount];

        for (int i = 0; i < BeamCount; i++)
        {
            Laser newLaser = LaserPool.Instance.GetEntityLaser(LaserPattern.Single, _laserSource);

            Vector2 directionShift = laser.Direction.Rotated((i - 1) * BeamAngle);

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
            Vector2 directionShift = direction.Rotated((i - 1) * BeamAngle);
            _beams[i].SetLaser(laser_source, position, directionShift, initialLength, maxLength, width, speed, damage);
            _beams[i].SetActive();
            GD.Print(i + " " + _beams[i].Direction + " " + _beams[i].RotationDegrees);
        }
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
