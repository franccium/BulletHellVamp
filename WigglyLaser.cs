using Godot;

public partial class WigglyLaser : MultipleBeamLaser
{
    public float Amplitude { get; set; } = 200f;
    protected Vector2 _initialDirection;
    protected Vector2 _initialPosition;

    public override void SetupBeams(Laser laser)
    {
        _initialPosition = Position;
        _initialDirection = Direction;
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
        _initialDirection = Direction;
        _initialPosition = Position;
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
        CheckForWiggle();
    }

    private int _flip = 1;
    protected void CheckForWiggle()
    {
        if (_beams[0].Position.DistanceTo(_initialPosition) > Amplitude)
        {
            foreach (Laser beam in _beams)
            {
                Vector2 newDirection = beam.Direction.Rotated(Mathf.Pi / 4f * _flip);
                beam.SetDirection(newDirection);
                _initialPosition = beam.Position;
                GD.Print("rotated");
            }
            _flip = -_flip;
        }
    }

    public override void MoveLaser(float delta, Laser laser)
    {
        laser.Position += laser.Direction * laser.Speed * delta;
    }
}
