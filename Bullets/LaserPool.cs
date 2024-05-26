using Godot;
using System;
using System.Collections.Generic;

public enum LaserPattern
{
    Single,
    Triple,
    Star,
    StarWiggly
}

public struct LaserPatternPool
{
    public LaserPattern LaserPattern { get; set; }
    public int PoolSize { get; set; }
}

public partial class LaserPool : Node
{
    public static LaserPool Instance { get; private set; }

    [Export]
    private PackedScene[] _laserScenes;
    //private PackedScene _laser;
    private Dictionary<LaserPattern, PackedScene> _lasers;

    [Export]
    private int _poolSize;

    //private Queue<Laser> _laserPool;
    private Dictionary<LaserPattern, Queue<Laser>> _laserPools;


    private Dictionary<Entity, Dictionary<LaserPattern, Queue<Laser>>> _entityLaserPool;


    public override void _Ready()
    {
        Instance = this;

        _lasers = new Dictionary<LaserPattern, PackedScene>();
        for (int i = 0; i < _laserScenes.Length; i++)
        {
            _lasers[(LaserPattern)i] = _laserScenes[i]; // scenes must be in the same order an enum
        }
        /*
                _laserPools = new Dictionary<LaserPattern, Queue<Laser>>();

                foreach (var laserPattern in _lasers.Keys)
                {
                    Queue<Laser> laserPool = new Queue<Laser>();
                    for (int i = 0; i < _poolSize; i++)
                    {
                        var laser = (Laser)_lasers[laserPattern].Instantiate();
                        AddChild(laser);
                        laser.SetNotActive();
                        laserPool.Enqueue(laser);
                    }

                    _laserPools.Add(laserPattern, laserPool);
                }
        */

        _entityLaserPool = new Dictionary<Entity, Dictionary<LaserPattern, Queue<Laser>>>();
    }
    /*
        public Laser GetLaser(LaserPattern laser_pattern)
        {
            if (_laserPools[laser_pattern].Count == 0)
            {
                var laser = (Laser)_lasers[laser_pattern].Instantiate();
                laser.LaserPattern = laser_pattern; // needed for returning to the pool, as it's called from base class
                AddChild(laser);
                return laser;
            }
            else
            {
                var laser = _laserPools[laser_pattern].Dequeue();
                laser.SetActive();
                laser.SetProcess(true);
                return laser;
            }
        }

        public void ReturnLaser(LaserPattern laser_pattern, Laser laser)
        {
            laser.Hide();
            laser.SetNotActive();
            _laserPools[laser_pattern].Enqueue(laser);
        }

    */


    private Laser CreateLaser(LaserPattern laser_pattern, Entity entity)
    {
        // supposed to create a laser of a given type for the entity pool

        //Laser laser = new 

        Laser laser = (Laser)_lasers[laser_pattern].Instantiate();
        laser._laserSource = entity;
        AddChild(laser);
        laser.SetNotActive();
        laser.LaserPattern = laser_pattern; // needed?
        //GD.Print(laser.GetType());
        return laser;
    }

    public void PopulateEntityLaserPool(LaserPatternPool[] laser_pattterns, Entity entity)
    {
        if (!_entityLaserPool.ContainsKey(entity))
        {
            _entityLaserPool[entity] = new Dictionary<LaserPattern, Queue<Laser>>();
        }

        foreach (var laserPatternPool in laser_pattterns)
        {
            _entityLaserPool[entity][laserPatternPool.LaserPattern] = new Queue<Laser>();
            for (int j = 0; j < laserPatternPool.PoolSize; j++)
            {
                Laser laser = CreateLaser(laserPatternPool.LaserPattern, entity);
                _entityLaserPool[entity][laserPatternPool.LaserPattern].Enqueue(laser);
            }
        }
    }

    public Laser GetEntityLaser(LaserPattern laser_pattern, Entity entity)
    {
        if (_entityLaserPool[entity][laser_pattern].Count == 0) //? will create a new laser if pool is empty, so it's not really a pool, idk how i feel about that but maybe its fine idk
        {
            Laser laser = CreateLaser(laser_pattern, entity);
            GD.Print("Creating laser for pool");
            return laser;
        }
        else
        {
            Laser laser = _entityLaserPool[entity][laser_pattern].Dequeue();
            GD.Print("Getting laser from pool");
            return laser;
        }
    }


    public void ReturnEntityLaser(Laser laser, Entity entity)
    {
        laser.SetNotActive();
        _entityLaserPool[entity][laser.LaserPattern].Enqueue(laser);
        //GD.Print("returned laser: " + laser.GetType());
    }


    /*
        public override void _Ready()
        {
            Instance = this;

            _laserPool = new Queue<Laser>();

            for (int i = 0; i < _poolSize; i++)
            {
                var laser = (Laser)_laser.Instantiate();
                AddChild(laser);
                laser.SetNotActive();
                _laserPool.Enqueue(laser);
            }
        }

        public Laser GetLaser()
        {
            //GD.Print(_laserPool.Count);
            if (_laserPool.Count == 0)
            {
                var laser = (Laser)_laser.Instantiate();
                AddChild(laser);
                return laser;
            }
            else
            {
                var laser = _laserPool.Dequeue();
                laser.SetActive();
                laser.SetProcess(true);
                return laser;
            }
        }

        public void ReturnLaser(Laser laser)
        {
            laser.Hide();
            laser.SetNotActive();
            _laserPool.Enqueue(laser);
            //GD.Print(_laserPool.Count);
        }

        public void OnLaserDestroyed(Laser laser) // nie mam jeszcze delete eventu
        {
            ReturnLaser(laser);
        }
        */

}
