using Godot;
using System;

public partial class Entity : Node2D
{
    public override void _Ready()
    {
        GatherRequirements();
    }

    public override void _Process(double delta)
    {
    }

    protected virtual void GatherRequirements()
    {
        _sprite = GetNode<AnimatedSprite2D>("Sprite");

        _healthBar = GetNode<ProgressBar>("HealthBar");
        _currentHealth = _maxHealth;
        _healthBar.MaxValue = _maxHealth;
        _healthBar.Value = _currentHealth;
        _healthBar.Hide();
    }

    #region ATTACKS

    public virtual void CreateBulletPool()
    {
    }

    public virtual void CreateLaserPool()
    {
    }

    #endregion

    #region MOVEMENT

    [Export]
    protected float _moveSpeed;
    protected Vector2 _velocity;
    protected Vector2 _direction;

    public virtual void CalculateVelocity()
    {
        _velocity = _direction * _moveSpeed;
    }

    public virtual void UpdatePosition(float delta)
    {
        Position += _velocity * delta;
    }

    #endregion

    #region ANIMATION

    protected AnimatedSprite2D _sprite;

    public virtual void FlipSprite()
    {
        _sprite.FlipH = _direction.X < 0;
    }

    public virtual void UpdateAnimations()
    {
        if (_velocity == Vector2.Zero)
            _sprite.Play("idle");
        else if (_velocity.Y == 0)
            _sprite.Play("walk_right");
        else if (_velocity.X == 0 && _velocity.Y > 0)
            _sprite.Play("walk_front");
        else if (_velocity.X == 0 && _velocity.Y < 0)
            _sprite.Play("walk_top");
        else if (_velocity.X != 0 && _velocity.Y < 0)
            _sprite.Play("walk_topright");

        if (_direction.X != 0)
            FlipSprite();
    }

    #endregion

    #region HEALTH

    [Export]
    protected int _maxHealth;
    protected int _currentHealth;
    private ProgressBar _healthBar;

    public virtual void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
        UpdateHealth(_currentHealth);
    }

    public virtual void UpdateHealth(int new_health)
    {
        _currentHealth = new_health;
        _healthBar.Value = _currentHealth;
        if (_currentHealth < _maxHealth)
            _healthBar.Show();
    }

    public virtual void Die()
    {
        //QueueFree();
    }

    #endregion
}
