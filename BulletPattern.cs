using System;
using Godot;

public abstract class BulletPattern
{
    public float BulletSpeed { get; set; } = 0f;

    public virtual void UpdatePosition(Bullet bullet, float delta)
    {
        bullet.Velocity = bullet.Direction * BulletSpeed;
        bullet.Position += bullet.Velocity * delta;
        //GD.Print(BulletSpeed);
    }
}

public class StraightPattern : BulletPattern
{
    public override void UpdatePosition(Bullet bullet, float delta)
    {
        base.UpdatePosition(bullet, delta);
    }
}

public class CircularSpreadPattern : BulletPattern
{
    public float CircleRadius { get; set; } = 0f;
    public float SpreadAngle { get; set; } = 0f;
    public float SpreadSpeed { get; set; } = 0f;

    private float _elapsedTime = 0f;

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        base.UpdatePosition(bullet, delta);
    }
}

public class CurvePattern : BulletPattern
{
    public Vector2 AnchorPoint { get; set; } = Vector2.Zero;
    public float CurveRadius { get; set; } = 0f;
    public float MaxRotation { get; set; } = 0f;
    public float SpinSpeed { get; set; } = 0f;
    public const int CURVE_DIRECTION_UP = -1;
    public const int CURVE_DIRECTION_DOWN = 1;
    public int CurveDirection { get; set; } = CURVE_DIRECTION_UP;

    private float _totalRotation = 0f;

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        // changes the direction vector to a vector that is perpendicular to the radius of anchor point to current position vector
        if (_totalRotation < MaxRotation)
        {
            Vector2 radiusVector = bullet.Position - AnchorPoint;
            radiusVector *= CurveDirection;
            Vector2 newDirection = new Vector2(-radiusVector.Y, radiusVector.X).Normalized();
            /*
            float dotProduct = _direction.Dot(newDirection);
            float angle = Mathf.Acos(dotProduct / (_direction.Length() * newDirection.Length()));
            _totalRotation += Mathf.Abs(angle);*/
            float curveAngle = bullet.Direction.AngleTo(newDirection);
            _totalRotation += Mathf.Abs(curveAngle);

            bullet.Direction = bullet.Direction.Lerp(newDirection, SpinSpeed * delta);
        }
        // if the curving is finished, the bullet translates into travelling a straight line in it's initial direction
        else if (_totalRotation >= MaxRotation)
        {
            bullet.Direction = bullet.Direction.Lerp(bullet.InitialDirection, SpinSpeed * delta * 0.1f);
        }

        base.UpdatePosition(bullet, delta);
    }

    public CurvePattern() { }

    public CurvePattern(Vector2 anchorPoint, float curveRadius, float spinSpeed,
        float bulletSpeed, float maxRotation, int curveDirection)
    {
        AnchorPoint = anchorPoint;
        CurveRadius = curveRadius;
        SpinSpeed = spinSpeed;
        BulletSpeed = bulletSpeed;
        MaxRotation = maxRotation;
        CurveDirection = curveDirection;
    }
}


public class CenterReturnPattern : BulletPattern
{
    public float Amplitude { get; set; } = 0f;
    public float InitialX { get; set; } = 0f;

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        if (Mathf.Abs(bullet.Position.X - InitialX) > Amplitude)
        {
            bullet.Direction = -bullet.Direction;
        }

        base.UpdatePosition(bullet, delta);
    }
}

public class WigglePattern : BulletPattern
{
    public float Amplitude { get; set; } = 0f;
    public float Frequency { get; set; } = 0f;
    public float Angle { get; set; } = 0f;
    public Vector2 InitialPosition { get; set; } = Vector2.Zero;

    private int _flip = 1;
    public override void UpdatePosition(Bullet bullet, float delta)
    {
        if (bullet.Position.DistanceTo(InitialPosition) > Amplitude)
        {
            float angleRad = Mathf.DegToRad(Angle * _flip);
            bullet.Direction = bullet.Direction.Rotated(angleRad);
            _flip = -_flip;
            InitialPosition = bullet.Position;
        }

        base.UpdatePosition(bullet, delta);
    }
}

public class SinPattern : BulletPattern
{
    public float Amplitude { get; set; } = 0f;
    public float Frequency { get; set; } = 0f;
    public float Phase { get; private set; } = 0f;

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        // Update the phase of the sine wave
        Phase += Frequency * delta;

        // Calculate the y offset using the sine wave function
        float yOffset = Amplitude * Mathf.Sin(Phase);

        // Calculate the new position
        Vector2 newPosition = bullet.InitialPosition + bullet.InitialDirection * bullet.BulletSpeed * delta;
        newPosition.Y += yOffset;

        // Update the bullet's position
        bullet.Position = newPosition;
        base.UpdatePosition(bullet, delta);
    }
}

public class XBoxPattern : BulletPattern
{
    public float Amplitude { get; set; } = 0f;
    public Vector2 InitialPosition { get; set; } = Vector2.Zero;

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        if (bullet.Position.DistanceTo(InitialPosition) > Amplitude)
        {
            Vector2 nextDirection = -bullet.Direction;
            nextDirection = nextDirection.Rotated((float)(Math.PI / 8f));
            bullet.Direction = nextDirection;
        }

        base.UpdatePosition(bullet, delta);
    }
}

public class HomingBullet : BulletPattern
{
    public Entity Target { get; set; } = null;
    public float HomingSpeed { get; set; } = 0f;

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        Vector2 direction = (Target.Position - bullet.Position).Normalized();
        bullet.Direction = bullet.Direction.Lerp(direction, HomingSpeed * delta);

        base.UpdatePosition(bullet, delta);
    }
}

public class DoubleHomingBullet : CurvePattern2
{
    public Entity Target { get; set; } = null;
    public float HomingSpeed { get; set; } = 0f;

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        Vector2 direction = (Target.Position - bullet.Position).Normalized();

        Vector2 directionAdd = CalculateDirection(bullet, delta);
        bullet.Direction += directionAdd;
        bullet.Direction = bullet.Direction.Lerp(direction, HomingSpeed * delta);
        bullet.Velocity = bullet.Direction * BulletSpeed;
        bullet.Position += bullet.Velocity * delta;

        //base.UpdatePosition(bullet, delta);
        //bullet.ApplyHoming(Target, 0.1f, delta);
    }
}



#region DECORATORS

public static class BulletExtensions
{
    public static void ApplyHoming(this Bullet bullet, Entity target, float homingSpeed, float delta)
    {
        Vector2 direction = (target.Position - bullet.Position).Normalized();
        bullet.Direction = bullet.Direction.Lerp(direction, homingSpeed * delta);
    }

    public static void ApplyCurve()
    {

    }
}

/// <summary>
/// these work cause direction is calculated through all stages before calling the classic update of position with bullet speed, should look for a more aestethic way though
/// </summary> <summary>
public class BulletPatternDecorator : BulletPattern
{
    protected BulletPattern _bulletPattern;

    public BulletPatternDecorator(BulletPattern bulletPattern)
    {
        _bulletPattern = bulletPattern;
    }

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        _bulletPattern.UpdatePosition(bullet, delta);
    }
}

public class CurvePatternDecorator : BulletPatternDecorator
{
    private CurvePattern _curve;
    private float _totalRotation = 0f;

    public CurvePatternDecorator(CurvePattern curve_pattern, BulletPattern bullet_pattern) : base(bullet_pattern)
    {
        _curve = curve_pattern;
    }

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        if (_totalRotation < _curve.MaxRotation)
        {
            Vector2 radiusVector = bullet.Position - _curve.AnchorPoint;
            radiusVector *= _curve.CurveDirection;
            Vector2 newDirection = new Vector2(-radiusVector.Y, radiusVector.X).Normalized();
            float curveAngle = bullet.Direction.AngleTo(newDirection);
            _totalRotation += Mathf.Abs(curveAngle);

            bullet.Direction = bullet.Direction.Lerp(newDirection, _curve.SpinSpeed * delta);
        }

        base.UpdatePosition(bullet, delta);
    }
}

public class CenterReturnDecorator : BulletPatternDecorator
{
    private CenterReturnPattern _return;
    private float _initialX;
    private float _amplitude;

    public CenterReturnDecorator(BulletPattern bulletPattern, float initial_x, float Amplitude) : base(bulletPattern)
    {
        _return = bulletPattern as CenterReturnPattern;
        _initialX = initial_x;
        _amplitude = Amplitude;
    }

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        if (Mathf.Abs(bullet.Position.X - _initialX) > _amplitude)
        {
            bullet.Direction = -bullet.Direction;
        }

        base.UpdatePosition(bullet, delta);
    }
}

public class HomingDecorator : BulletPatternDecorator
{
    private HomingBullet _homing;
    Entity _target;
    float _homingSpeed;

    public HomingDecorator(BulletPattern bulletPattern, Entity target, float homing_speed) : base(bulletPattern)
    {
        _homing = bulletPattern as HomingBullet;
        _target = target;
        _homingSpeed = homing_speed;
    }

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        Vector2 direction = (_target.Position - bullet.Position).Normalized();
        bullet.Direction = bullet.Direction.Lerp(direction, _homingSpeed * delta);

        base.UpdatePosition(bullet, delta);
    }
}

#endregion



public class CurvePattern2 : BulletPattern
{
    public Vector2 AnchorPoint { get; set; } = Vector2.Zero;
    public float CurveRadius { get; set; } = 0f;
    public float MaxRotation { get; set; } = 0f;
    public float SpinSpeed { get; set; } = 0f;
    public const int CURVE_DIRECTION_UP = -1;
    public const int CURVE_DIRECTION_DOWN = 1;
    public int CurveDirection { get; set; } = CURVE_DIRECTION_UP;

    private float _totalRotation = 0f;

    public Vector2 CalculateDirection(Bullet bullet, float delta)
    {
        //if (_totalRotation < MaxRotation)
        //{
            // changes the direction vector to a vector that is perpendicular to the radius of anchor point to current position vector
            Vector2 radiusVector = bullet.Position - AnchorPoint;
            radiusVector *= CurveDirection;
            Vector2 newDirection = new Vector2(-radiusVector.Y, radiusVector.X).Normalized();

            float curveAngle = bullet.Direction.AngleTo(newDirection);
            //_totalRotation += Mathf.Abs(curveAngle);
            GD.Print("curveAngle: " + curveAngle, " totalRotation: " + _totalRotation);

            //bullet.Direction = bullet.Direction.Lerp(newDirection, SpinSpeed * delta);
            Vector2 newDirection2 = bullet.Direction.Lerp(newDirection, SpinSpeed * delta).Normalized();
            GD.Print("curve");
            return newDirection2;
        //}
        // if the curving is finished, the bullet translates into travelling a straight line in it's initial direction
        //else if (_totalRotation >= MaxRotation)
        //{
            //bullet.Direction = bullet.Direction.Lerp(bullet.InitialDirection, SpinSpeed * delta * 0.1f).Normalized();
            //Vector2 newDirection2 = bullet.Direction.Lerp(bullet.InitialDirection, SpinSpeed * delta * 0.1f).Normalized();
            //GD.Print(" totalRotation: " + _totalRotation, " maxRotation: " + MaxRotation, " straight");
            //return newDirection2;
        //}
        //return Vector2.Zero;
        //return newDirection; //? maybe like that instead
    }

    public override void UpdatePosition(Bullet bullet, float delta)
    {
        CalculateDirection(bullet, delta);

        base.UpdatePosition(bullet, delta);
    }

    public CurvePattern2() { }

    public CurvePattern2(Vector2 anchorPoint, float curveRadius, float spinSpeed,
        float bulletSpeed, float maxRotation, int curveDirection)
    {
        AnchorPoint = anchorPoint;
        CurveRadius = curveRadius;
        SpinSpeed = spinSpeed;
        BulletSpeed = bulletSpeed;
        MaxRotation = maxRotation;
        CurveDirection = curveDirection;
    }
}