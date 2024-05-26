using Godot;
using System;
using System.Collections.Generic;

public enum BulletType
{
    Player,
    Enemy,
    Big
}

public struct BulletTypePool
{
    public BulletType Type { get; set; }
    public int PoolSize { get; set; }
}

public partial class BulletPool : Node
{
    public static BulletPool Instance { get; private set; }

    [Export]
    private PackedScene[] _bulletScenes;
    private Dictionary<BulletType, PackedScene> _bullets;
    private Dictionary<Type, Dictionary<BulletType, Queue<Bullet>>> _entityBulletPool;
    //private Dictionary<Entity, Dictionary<BulletType, Queue<Bullet>>> _entityBulletPool;

    public override void _Ready()
    {
        Instance = this;

        _bullets = new Dictionary<BulletType, PackedScene>();
        for (int i = 0; i < _bulletScenes.Length; i++)
        {
            _bullets[(BulletType)i] = _bulletScenes[i]; // scenes must be in the same order an enum
        }
        _entityBulletPool = new Dictionary<Type, Dictionary<BulletType, Queue<Bullet>>>();
        //_entityBulletPool = new Dictionary<Entity, Dictionary<BulletType, Queue<Bullet>>>();
    }

    public Bullet CreateBullet(BulletType bulletType, Entity entity)
    {
        var bullet = (Bullet)_bullets[bulletType].Instantiate();
        bullet._bulletSource = entity;
        bullet.BulletType = bulletType;
        AddChild(bullet);
        bullet.SetNotActive();
        return bullet;
    }

    /// <summary>
    /// if the scene is entered by a unique class entity, it will create a pool for that entity, if the pool for the entity is already present, it expands it with the requirements of the new entity
    /// </summary>
    /// <param name="bullet_types"></param>
    /// <param name="entity"></param> <summary>
    public void PopulateBulletPool(BulletTypePool[] bullet_types, Entity entity)
    {
        Type entityType = entity.GetType();

        if (!_entityBulletPool.ContainsKey(entityType))
        {
            _entityBulletPool[entityType] = new Dictionary<BulletType, Queue<Bullet>>();
            GD.Print("Created bullet pool for " + entity.GetType());
        }

        foreach (var bulletTypePool in bullet_types)
        {
            // if the pool is already big enough, skip and reuse the bullets
            if (_entityBulletPool[entityType].ContainsKey(bulletTypePool.Type))
            {
                if (entity is Enemy enemy
                    && EnemySpawner.Instance.GetActiveEnemiesOfType(enemy.EnemyType) * bulletTypePool.PoolSize
                        < _entityBulletPool[entityType][bulletTypePool.Type].Count)
                {
                    GD.Print("Pool for " + bulletTypePool.Type + " for " + entity.GetType() + " is already big enough"
                        + " (" + _entityBulletPool[entityType][bulletTypePool.Type].Count + " bullets)" + " there are presently " +
                        EnemySpawner.Instance.GetActiveEnemiesOfType(enemy.EnemyType) * bulletTypePool.PoolSize + " enemies * bullets of this type");
                    continue;
                }
            }

            _entityBulletPool[entityType][bulletTypePool.Type] = new Queue<Bullet>();
            for (int j = 0; j < bulletTypePool.PoolSize; j++)
            {
                Bullet bullet = CreateBullet(bulletTypePool.Type, entity);
                _entityBulletPool[entityType][bulletTypePool.Type].Enqueue(bullet);
            }
            GD.Print("Populated " + bulletTypePool.Type + " pool for " + entity.GetType());
        }
    }

    public Bullet GetEntityBullet(BulletType bulletType, Entity entity)
    {
        Type entityType = entity.GetType();

        if (_entityBulletPool[entityType][bulletType].Count == 0)
        {
            Bullet bullet = CreateBullet(bulletType, entity);
            return bullet;
        }
        else
        {
            Bullet bullet = _entityBulletPool[entityType][bulletType].Dequeue();
            bullet.SetActive();
            return bullet;
        }
    }

    public void ReturnEntityBullet(Bullet bullet, Entity entity)
    {
        Type entityType = entity.GetType();

        bullet.SetNotActive();
        //GD.Print("returned bullet: " + bullet.GetType() + " for entity " + entity.GetType() + "");

        _entityBulletPool[entityType][bullet.BulletType].Enqueue(bullet);
    }


    /*
        public override void _Ready()
        {
            Instance = this;

            _bullets = new Dictionary<BulletType, PackedScene>();
            for (int i = 0; i < _bulletScenes.Length; i++)
            {
                _bullets[(BulletType)i] = _bulletScenes[i]; // scenes must be in the same order an enum
            }
            _entityBulletPool = new Dictionary<Entity, Dictionary<BulletType, Queue<Bullet>>>();
        }

        public Bullet CreateBullet(BulletType bulletType, Entity entity)
        {
            var bullet = (Bullet)_bullets[bulletType].Instantiate();
            bullet._bulletSource = entity;
            bullet.BulletType = bulletType;
            AddChild(bullet);
            bullet.SetNotActive();
            return bullet;
        }

        public void PopulateBulletPool(BulletTypePool[] bullet_types, Entity entity)
        {
            if (!_entityBulletPool.ContainsKey(entity))
            {
                _entityBulletPool[entity] = new Dictionary<BulletType, Queue<Bullet>>();
                GD.Print("Created bullet pool for " + entity.GetType());
            }

            foreach (var bulletTypePool in bullet_types)
            {
                _entityBulletPool[entity][bulletTypePool.Type] = new Queue<Bullet>();
                for (int j = 0; j < bulletTypePool.PoolSize; j++)
                {
                    Bullet bullet = CreateBullet(bulletTypePool.Type, entity);
                    _entityBulletPool[entity][bulletTypePool.Type].Enqueue(bullet);
                }
                GD.Print("Populated " + bulletTypePool.Type + " pool for " + entity.GetType());
            }
        }

        public Bullet GetEntityBullet(BulletType bulletType, Entity entity)
        {
            if (_entityBulletPool[entity][bulletType].Count == 0)
            {
                Bullet bullet = CreateBullet(bulletType, entity);
                return bullet;
            }
            else
            {
                Bullet bullet = _entityBulletPool[entity][bulletType].Dequeue();
                bullet.SetActive();
                return bullet;
            }
        }

        public void ReturnEntityBullet(Bullet bullet, Entity entity)
        {
            bullet.SetNotActive();
            GD.Print("returned bullet: " + bullet.GetType() + " for entity " + entity.GetType() + "");

            _entityBulletPool[entity][bullet.BulletType].Enqueue(bullet);
        }
    */

}
