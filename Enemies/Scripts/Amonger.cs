using Godot;
using System;
using Godot.Collections;

public partial class Amonger : Enemy
{


    #region MOVEMENT

    #endregion


    #region ATTACKS

    protected override void Fire()
    {
        for (int i = 0; i < 3; i++)
        {
            var bullet = (Bullet)_bullet.Instantiate();
            GetTree().Root.AddChild(bullet);
            bullet.Position = Position;
            Vector2 bulletDirection = _player.Position - Position - new Vector2(0, 30) + i * new Vector2(0, 30);
            bullet.SetDirection(bulletDirection.Normalized());

            _bulletCooldown.Start();
        }
    }

    #endregion
}
