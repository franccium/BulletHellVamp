using Godot;
using System;
using Godot.Collections;

public partial class Enemy : Entity
{
    public EnemySpawner.EnemyTypes EnemyType { get; set; }

    #region INITIALISE

    public override void _Ready()
    {
        GatherRequirements();
        _bulletCooldown.Start();
    }

    public override void _Process(double delta)
    {
        CalculateVelocity();
        UpdatePosition((float)delta);
        UpdateAnimations();
        if (_bulletCooldown.IsStopped()) Fire();
    }

    protected override void GatherRequirements()
    {
        base.GatherRequirements();

        _player = GetTree().GetFirstNodeInGroup("Player") as Player;
        _bulletCooldown = GetNode<Timer>("BulletCooldown");
    }

    public void SetNotActive()
    {
        Hide();
        SetProcess(false);
        //todo set some immunity, or ill kill them with something accidentally
        Position = new Vector2(-2000, -2000);
    }

    public void SetActive()
    {
        Show();
        SetProcess(true);
    }

    #endregion

    #region MOVEMENT

    public virtual void CalculateDirection()
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
        Position += _velocity * delta;
    }

    #endregion


    #region ATTACKS

    protected Player _player;

    [Export]
    protected PackedScene _bullet;

    protected Timer _bulletCooldown;

    protected virtual void Fire()
    {
        var bullet = (Bullet)_bullet.Instantiate();
        bullet.Modulate = Colors.Red;
        GetTree().Root.AddChild(bullet);
        bullet.Position = Position;
        Vector2 bulletDirection = _player.Position - Position;
        bullet.SetDirection(bulletDirection.Normalized());

        bullet._bulletSource = this;

        _bulletCooldown.Start();
    }

    #endregion


    #region DEATH

    protected int _powerUpValue = 1;
    protected int _expValue = 25;

    private bool _isDead = false; //? needed to prevent multiple calls to Die() when multiple sources of damage, the problem is probably that death is queued for the frame, and in the frame multiple lasers are going to update; could also create some sort of queue for deletion by myself, but i dont think its that big of a deal for now

    public override void Die()
    {
        if (_isDead) return;
        _isDead = true;

        _player.IncreaseExp(_expValue);
        PickupManager.Instance.SpawnPowerUp(Position, _powerUpValue);
        EnemySpawner.Instance.RemoveActiveEnemy(this.EnemyType);
        Restart();
        //QueueFree();
        EnemySpawner.Instance.ReturnEnemyToPool(this);
    }

    public virtual void Restart()
    {
        _isDead = false;
        _currentHealth = _maxHealth;
    }

    #endregion
}
