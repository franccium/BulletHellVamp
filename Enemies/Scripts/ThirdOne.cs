using Godot;
using System;
using Godot.Collections;

public partial class ThirdOne : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        CreateBulletPool();
    }

    #region MOVEMENT

    #endregion


    #region ATTACKS

    [Export]
    private int _bulletsPerWave = 20;
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
            //var bullet = BulletPool.Instance.GetBullet();
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

            bullet._bulletSource = this;

            _bulletCooldown.Start();

            _currentBulletCreationIndex++;

            GetTree().CreateTimer(0.001f).Connect("timeout", new Callable(this, nameof(SpawnBullet)));
        }
    }

    public override void CreateBulletPool()
    {
        BulletTypePool[] bulletTypePools = new BulletTypePool[]
        {
            new BulletTypePool { Type = BulletType.Enemy, PoolSize = 30 },
        };

        BulletPool.Instance.PopulateBulletPool(bulletTypePools, this);
    }

    #endregion
}
