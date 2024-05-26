using BulletHellVamp;
using Godot;
using System;



public partial class Bullet : Node2D
{
    [Export]
    public float BulletSpeed = 400f;
    public int Damage { get; set; }
    public BulletPattern Pattern { get; set; }
    public BulletType BulletType { get; set; }

    public Vector2 Direction;

    //public Node2D _bulletSource { get; set; }
    public Entity _bulletSource { get; set; }

    public override void _Ready()
    {
        AddToGroup("Bullets");
        GetNode<Area2D>("Area2D").Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));

        Pattern = new StraightPattern
        {
            BulletSpeed = 0f,
        };
    }

    public override void _Process(double delta)
    {
        UpdatePosition((float)delta);
        CheckInScreenBounds();
    }

    /// <summary>
    /// sets the initial direction of the bullet
    /// </summary>
    /// <param name="direction"></param>
    public void SetDirection(Vector2 direction)
    {
        Direction = direction;
        InitialPosition = Position;
        InitialDirection = direction;
        Rotation = direction.Angle();
    }

    public Vector2 InitialDirection;
    public Vector2 InitialPosition;

    public Vector2 Velocity;

    private void UpdatePosition(float delta)
    {
        Pattern.UpdatePosition(this, delta);
    }



    /// <summary>
    /// checks if the bullet can be seen, and if not, it returns to the bullet pool
    /// </summary>
    private void CheckInScreenBounds()
    {
        Rect2 screenRect = GetViewport().GetVisibleRect();
        if (!screenRect.HasPoint(Position))
        {
            BulletPool.Instance.ReturnEntityBullet(this, _bulletSource);
        }
    }

    /// <summary>
    /// handles the collision of the bullet with other objects
    /// </summary>
    /// <param name="area"></param>
    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("Bullets"))
            return;

        Node2D collider = (Node2D)area.GetParent();

        if (collider is Enemy && _bulletSource is Enemy
            || collider is Player && _bulletSource is Player)
            return;

        if(BulletType == BulletType.Enemy)
        {
            if (collider.IsInGroup("Player"))
            {
                BulletPool.Instance.ReturnEntityBullet(this, _bulletSource);
                Player player = area.GetParent() as Player;
                player.TakeDamage(Damage);
            }
        }
        else if (BulletType == BulletType.Player)
        {
            if (collider.IsInGroup("Enemies"))
            {
                BulletPool.Instance.ReturnEntityBullet(this, _bulletSource);
                Enemy enemy = area.GetParent() as Enemy;
                enemy.TakeDamage(Damage);
            }
        }
        /*
        if (collider.IsInGroup("Enemies"))
        {
            BulletPool.Instance.ReturnEntityBullet(this, _bulletSource);
            Enemy enemy = area.GetParent() as Enemy;
            enemy.TakeDamage(Damage);
        }

        if (collider.IsInGroup("Player"))
        {
            BulletPool.Instance.ReturnEntityBullet(this, _bulletSource);
            Player player = area.GetParent() as Player;
            player.TakeDamage(Damage);
        }*/
    }

    /// <summary>
    /// Moves the bullet out of bounds and stops it's update loop.
    /// </summary>
    public void SetNotActive()
    {
        Hide();
        Position = new Vector2(-1000, -1000);
        //_totalRotation = 0f;
        //_elapsedTime = 0f;
        SetProcess(false);
    }

    public void SetActive()
    {
        Show();
        SetProcess(true);
    }
}
