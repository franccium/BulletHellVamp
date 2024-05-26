using Godot;
using System;
using Godot.Collections;

public partial class MusicBoss : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        GatherMusicTestRequirements();
        CreateBulletPool();
    }

    public override void _Process(double delta)
    {
        if (_onBeat)
        {
            CalculateVelocity();
            UpdatePosition((float)delta);
            base.UpdateAnimations();

            Fire();

            _onBeat = false;
        }
    }

    #region MOVEMENT

    public override void CalculateDirection()
    {
        _direction = (_player.Position - Position).Normalized();
    }

    public override void CalculateVelocity()
    {
        CalculateDirection();
        _velocity = _direction * _moveSpeed;
    }

    public override void UpdatePosition(float delta)
    {
        Position += _velocity;
    }

    #endregion


    #region ATTACKS

    [Export]
    private int _bulletsPerWave = 2;
    [Export]
    private float _buletSpeed = 100f;
    [Export]
    private float _bulletCurveRadius = 50f;
    [Export]
    private float _bulletSpinSpeed = 25f;

    private int _currentBulletCreationIndex = 0;

    protected override void Fire()
    {
        _currentBulletCreationIndex = 0;
        GetTree().CreateTimer(0.001f).Connect("timeout", new Callable(this, nameof(SpawnBullet)));
    }

    private void SpawnBullet()
    {
        if (_currentBulletCreationIndex < _bulletsPerWave)
        {
            var bullet = BulletPool.Instance.GetEntityBullet(BulletType.Enemy, this);
            bullet.Position = Position;
            float bulletAngle = _currentBulletCreationIndex * (6 * Mathf.Pi / _bulletsPerWave);
            Vector2 bulletDirection = new Vector2(Mathf.Cos(bulletAngle), Mathf.Sin(bulletAngle));
            bullet.SetDirection(bulletDirection.Normalized());
            bullet._bulletSource = this;
            bullet.Damage = 1;

            bullet.Pattern = new CircularSpreadPattern
            {
                CircleRadius = _bulletCurveRadius,
                BulletSpeed = _buletSpeed,
            };

            _bulletCooldown.Start();

            bullet._bulletSource = this;

            _currentBulletCreationIndex++;

            GetTree().CreateTimer(0.001f).Connect("timeout", new Callable(this, nameof(SpawnBullet)));
        }
    }

    public override void CreateBulletPool()
    {
        BulletTypePool[] bulletTypePools = new BulletTypePool[]
        {
            new BulletTypePool { Type = BulletType.Enemy, PoolSize = 100 },
        };

        BulletPool.Instance.PopulateBulletPool(bulletTypePools, this);
    }

    #endregion



    #region MUSIC

    private AudioStreamPlayer2D _musicPlayer;
    private Timer _beatTimer;
    private ColorRect _beatIndicator;
    [Export]
    public float _bpm = 130f;
    private float _beatInterval;
    private bool _onBeat;

    /// <summary>
    /// Gather all requirements for the music test, set the beat timer and start the music.
    /// </summary>
    private void GatherMusicTestRequirements()
    {
        _musicPlayer = GetNode<AudioStreamPlayer2D>("MusicPlayer");
        _beatTimer = GetNode<Timer>("BeatTimer");
        _beatInterval = 60f / _bpm; // time between beats in seconds
        _beatIndicator = GetNode<ColorRect>("BeatIndicator");
        _onBeat = false;

        _beatTimer.Start(_beatInterval);
        _musicPlayer.Play();
        _beatIndicator.Visible = true;
    }

    /// <summary>
    /// Called when the beat timer times out.
    /// </summary>
    private void OnBeat()
    {
        _onBeat = true;
        _beatIndicator.Color = new Color(1, 1, 1, 1);
        GD.Print("BOSS BEAT");
    }

    #endregion
}
