using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node
{
    public static EnemySpawner Instance { get; private set; }

    public enum EnemyTypes
    {
        ThirdOne,
        BossTest,
        MusicBoss,
    }

    private struct EnemyTypePool
    {
        public EnemyTypes EnemyType { get; set; }
        public int PoolSize { get; set; }
    }

    [Export]
    private PackedScene[] _enemyScenes;
    private EnemyTypes _chosenEnemy;
    private Label _chosenEnemyLabel;

    private Player _player;

    private Dictionary<EnemyTypes, int> _activeEnemies = new Dictionary<EnemyTypes, int>();


    private Dictionary<EnemyTypes, Queue<Enemy>> _enemyPool = new Dictionary<EnemyTypes, Queue<Enemy>>();
    private EnemyTypePool[] _enemyTypePools = new EnemyTypePool[]
    {
        new EnemyTypePool { EnemyType = EnemyTypes.ThirdOne, PoolSize = 10 },
        new EnemyTypePool { EnemyType = EnemyTypes.BossTest, PoolSize = 1 },
        new EnemyTypePool { EnemyType = EnemyTypes.MusicBoss, PoolSize = 1 },
    };

    public override void _Ready()
    {
        Instance = this;

        _chosenEnemy = EnemyTypes.ThirdOne;
        _chosenEnemyLabel = GetNode<Label>("Control/ChosenEnemy");
        _chosenEnemyLabel.Text = _chosenEnemy.ToString();

        _player = GetTree().Root.GetNode<Player>("Game/Player");

        CallDeferred(nameof(InitialiseEnemyPool));
        CallDeferred(nameof(InitialSpawn));
    }

    public override void _Process(double delta)
    {
        GetInput();
    }

    #region POOL

    /// <summary>
    /// creates the initial pools for the entities of one type
    /// </summary> <summary>
    private void InitialiseEnemyPool()
    {
        for (int i = 0; i < _enemyScenes.Length; i++)
        {
            _enemyPool[(EnemyTypes)i] = new Queue<Enemy>();
        }

        foreach (EnemyTypePool enemyTypePool in _enemyTypePools)
        {
            for (int i = 0; i < enemyTypePool.PoolSize; i++)
            {
                Enemy enemy = CreateEnemyOfType(enemyTypePool.EnemyType);
                _enemyPool[enemyTypePool.EnemyType].Enqueue(enemy);
                GD.Print("Created " + enemy.GetType() + " in pool");
            }
        }
    }

    /// <summary>
    /// creates a new enemy of the given type and adds it to the pool
    /// </summary>
    /// <param name="enemy_type"></param>
    /// <returns></returns>
    private Enemy CreateEnemyOfType(EnemyTypes enemy_type)
    {
        var enemy = (Enemy)_enemyScenes[(int)enemy_type].Instantiate();
        enemy.EnemyType = enemy_type;
        AddChild(enemy);
        enemy.AddToGroup("Enemies");
        enemy.Position = new Vector2(-2000, -2000);
        enemy.SetNotActive();

        return enemy;
    }

    /// <summary>
    /// returns an enemy of given type from the pool, if there are no enemies of this type left in the pool, returns null
    /// </summary>
    /// <returns></returns>
    public Enemy GetEnemyFromPool(EnemyTypes enemy_type)
    {
        if (_enemyPool[enemy_type].Count > 0)
        {
            Enemy enemy = _enemyPool[enemy_type].Dequeue();
            return enemy;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// return an enemy to the pool, to be utilised again
    /// </summary>
    /// <param name="enemy"></param>
    public void ReturnEnemyToPool(Enemy enemy) //todo should restart the enemy too
    {
        enemy.SetNotActive();
        //enemy.Restart(); or sth
        _enemyPool[enemy.EnemyType].Enqueue(enemy);
        GD.Print("Returned " + enemy.GetType() + " to pool");
        GD.Print("Pool size for " + enemy.GetType() + " is now " + _enemyPool[enemy.EnemyType].Count);
    }

    #endregion

    #region SPAWNING

    private void InitialSpawn()
    {
        var enemyType = EnemyTypes.ThirdOne;
        for (int i = 0; i < 2; i++)
        {
            SpawnEnemyOfType(enemyType);
        }

        enemyType = EnemyTypes.BossTest;
        SpawnEnemyOfType(enemyType);
    }

    /*
        private void SpawnEnemies()
        {
            AddToActiveEnemies();

            Node enemyScene = _enemyScenes[(int)_chosenEnemy].Instantiate();
            Enemy enemy = (Enemy)enemyScene;
            enemy.EnemyType = _chosenEnemy;

            AddChild(enemy);
            enemy.AddToGroup("Enemies");
            enemy.Position = _player.Position + new Vector2(GD.RandRange(-300, 300), GD.RandRange(-300, 300));
        }
        */

    private void SpawnEnemyOfType(EnemyTypes enemy_type)
    {
        AddToActiveEnemies(enemy_type);

        Enemy enemy = GetEnemyFromPool(enemy_type);
        if (enemy == null)
        {
            enemy = CreateEnemyOfType(enemy_type);
            GD.Print("No enemies left in pool, Creating new " + enemy.GetType());
        }

        enemy.SetActive();
        enemy.Position = _player.Position + new Vector2(GD.RandRange(-300, 300), GD.RandRange(-300, 300));
    }

    private void AddToActiveEnemies(EnemyTypes enemy_type)
    {
        if (!_activeEnemies.ContainsKey(enemy_type))
        {
            _activeEnemies[enemy_type] = 0;
            GD.Print("Created activeEnemies entry for " + enemy_type.ToString());
        }
        ++_activeEnemies[enemy_type];
    }

    #endregion

    #region INPUT

    private void GetInput()
    {
        if (Input.IsActionJustPressed("spawn_enemy"))
        {
            SpawnEnemyOfType(_chosenEnemy);
        }
        if (Input.IsActionJustPressed("change_enemy"))
        {
            _chosenEnemy = (EnemyTypes)(((int)_chosenEnemy + 1) % _enemyScenes.Length);
            _chosenEnemyLabel.Text = _chosenEnemy.ToString();
        }
    }

    #endregion

    public void RemoveActiveEnemy(EnemyTypes enemy_type)
    {
        GD.Print("count before " + _activeEnemies[enemy_type] + " " + enemy_type.ToString());
        --_activeEnemies[enemy_type];
        GD.Print("count after " + _activeEnemies[enemy_type] + " " + enemy_type.ToString());
        if (_activeEnemies[enemy_type] <= 0)
        {
            _activeEnemies.Remove(enemy_type);
            GD.Print("Removed activeEnemies entry for " + enemy_type.ToString());
        }
    }

    public int GetActiveEnemiesOfType(EnemyTypes enemy_type)
    {
        if (_activeEnemies.ContainsKey(enemy_type))
        {
            GD.Print("count " + _activeEnemies[enemy_type] + " " + enemy_type.ToString());
            return _activeEnemies[enemy_type];
        }
        GD.Print("not found " + enemy_type.ToString());
        return 0;
    }
}
