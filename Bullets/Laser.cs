using Godot;
using System;

public partial class Laser : Node2D
{
    protected ShapeCast2D _shapeCast;
    protected GpuParticles2D _beamParticles;
    protected GpuParticles2D _hitParticles;
    protected ParticleProcessMaterial _beamMaterial;
    public LaserPattern LaserPattern { get; set; }

    public override void _Ready()
    {
        InitialSetup();
    }

    public virtual void InitialSetup()
    {
        GatherRequirements();

        InitialLength = 250f;
        MaxLength = 500f;
        Width = 25f;
        Speed = 30f;
        Damage = 1;
        _currentLength = InitialLength;

        _shapeCast.Shape = new RectangleShape2D
        {
            Size = new Vector2(Width, InitialLength),
        };
        _shapeCast.Enabled = true;

        _beamParticles.Emitting = true;
        _hitParticles.Emitting = true;

        _beamParticles.ProcessMaterial = (ParticleProcessMaterial)_beamParticles.ProcessMaterial.Duplicate(true);
        _beamMaterial = (ParticleProcessMaterial)_beamParticles.ProcessMaterial;
    }

    protected virtual void GatherRequirements()
    {
        _shapeCast = GetNode<ShapeCast2D>("ShapeCast2D");
        _beamParticles = GetNode<GpuParticles2D>("BeamParticles");
        _hitParticles = GetNode<GpuParticles2D>("HitParticles");
    }

    public override void _Process(double delta)
    {
        UpdateLaser((float)delta);
        UpdateCollisions();
        CheckForOutOfBounds();
    }

    public Vector2 Direction;

    public void SetDirection(Vector2 direction)
    {
        Direction = direction;
        _shapeCast.TargetPosition = direction;


        //_beamMaterial.Direction = new Vector3(direction.X, direction.Y, 0);
        _beamMaterial.InitialVelocityMin = 0;
        _beamMaterial.InitialVelocityMax = 0;

        float angle = direction.Angle();
        if (angle < 0)
            angle += Mathf.Pi / 2;
        else
            angle -= Mathf.Pi / 2;

        _beamParticles.Rotation = angle;
        //_beamParticles.LookAt(direction);


        _shapeCast.Rotation = angle;
    }

    public void SetNotActive()
    {
        Hide();
        SetProcess(false);
        Position = new Vector2(-1000, -1000);

        _currentLength = 0f;
        _shapeCast.Shape = new RectangleShape2D
        {
            Size = new Vector2(Width, InitialLength),
        };

        _shapeCast.Enabled = false;
        _beamParticles.Emitting = false;
        _hitParticles.Emitting = false;
        _beamParticles.Restart();
        _hitParticles.Restart();
    }

    public void SetActive()
    {
        SetProcess(true);
        Show();

        _shapeCast.Enabled = true;
        _beamParticles.Emitting = true;
        _hitParticles.Emitting = true;
    }


    #region ATTACKS

    public float InitialLength;
    public float MaxLength;
    public float Width;
    public float Speed;
    public int Damage;

    public void SetLaser(Entity laser_source, Vector2 position,
        Vector2 direction, float initialLength,
        float maxLength, float width, float speed, int damage)
    {
        _laserSource = laser_source;
        Position = position;
        SetDirection(direction);
        InitialLength = initialLength;
        MaxLength = maxLength;
        Width = width;
        Speed = speed;
        Damage = damage;

        _currentLength = InitialLength;

        _shapeCast.Shape = new RectangleShape2D
        {
            Size = new Vector2(Width, InitialLength),
        };
        _shapeCast.Enabled = true;
        _shapeCast.CollideWithAreas = true;

        _beamMaterial.EmissionBoxExtents = new Vector3(Width, InitialLength, 0);
        _beamMaterial.ScaleMin = 2.5f;
        _beamMaterial.ScaleMax = 3.5f;
        _beamMaterial.AngleMin = -180f;
        _beamMaterial.AngleMax = 180f;
    }

    public virtual void ShootBeams(Entity laser_source, Vector2 position,
        Vector2 direction, float initialLength,
        float maxLength, float width, float speed, int damage)
    {
        SetLaser(laser_source, position, direction, initialLength, maxLength, width, speed, damage);
        SetActive();
        GD.Print("shoot beams in Laser");
    }

    protected float _currentLength;

    public virtual void UpdateLaser(float delta)
    {
        ExtendLength(delta);
        /*
        else
        {
            LaserPool.Instance.ReturnLaser(LaserPattern.Single, this);
            GD.Print("returning laser from length");
        }
        */
    }

    public virtual void MoveLaser(float delta)
    {
        MoveLaser(delta, this);
    }

    public virtual void MoveLaser(float delta, Laser laser)
    {
        laser.Position += laser.Direction * laser.Speed * delta;
    }

    /// <summary>
    /// returns out of bounds lasers
    /// </summary> <summary>
    public void CheckForOutOfBounds()
    {
        Rect2 screenRect = GetViewport().GetVisibleRect();
        if (!screenRect.HasPoint(Position))
        {
            LaserPool.Instance.ReturnEntityLaser(this, _laserSource);
        }
    }

    public virtual void ExtendLength(float delta)
    {
        ExtendLength(delta, this);
    }

    public virtual void ExtendLength(float delta, Laser laser)
    {
        if (laser._currentLength < laser.MaxLength)
        {
            laser._currentLength += laser.Speed * delta;

            if (laser._shapeCast.Shape is RectangleShape2D shape)
            {
                shape.Size = new Vector2(Width, _currentLength * 2);
            }
            //_shapeCast.Scale = new Vector2(1, _currentLength / InitialLength);

            // offsets the origin
            Vector2 offset = Direction.Normalized() * laser._currentLength;
            laser._beamParticles.Position = offset;
            laser._shapeCast.Position = offset;
            laser._beamMaterial.EmissionBoxExtents = new Vector3(laser.Width, laser._currentLength, 0);
        }
    }


    public Entity _laserSource { get; set; }

    public void UpdateCollisions()
    {
        _shapeCast.ForceShapecastUpdate();

        if (_shapeCast.IsColliding())
        {
            Area2D collidingArea = (Area2D)_shapeCast.GetCollider(0); //todo figure out the index
            Node2D collider = (Node2D)collidingArea.GetParent();
            if (collider is Enemy && _laserSource is Enemy
                || collider is Player && _laserSource is Player
                || collider is Bullet)
            {
                return;
            }

            if (collider.IsInGroup("Enemies"))
            {
                Enemy enemy = (Enemy)collider;
                enemy.TakeDamage(Damage);
            }

            if (collider.IsInGroup("Player"))
            {
                Player player = (Player)collider;
                player.TakeDamage(Damage);
            }
        }
    }

    #endregion
}
