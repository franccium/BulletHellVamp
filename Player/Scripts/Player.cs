using Godot;
using System;
using Godot.Collections;

public partial class Player : Entity
{
    public override void _Ready()
    {
        GatherRequirements();
        //GatherMusicTestRequirements();
        //_onBeat = true;
    }

    public override void _Process(double delta)
    {
        //if(_onBeat) // easy way to make everything work with music instead of dividing into rhytmic mode
        GatherInput((float)delta);
        UpdateMovement((float)delta);
        base.UpdateAnimations();
        UpdateGUI();
    }

    protected override void GatherRequirements()
    {
        base.GatherRequirements();

        _bullet = ResourceLoader.Load<PackedScene>("res://Player/Attacks/bullet.tscn");
        _bulletCooldown = GetNode<Timer>("BulletCooldown");

        _bulletPattern = PlayerBulletPatterns.STRAIGHT;
        _laserType = LaserPattern.Single;

        CallDeferred(nameof(InitialiseGUI));

        CreateBulletPool();
        CreateLaserPool();

        InitialiseDash();
    }

    #region INPUT

    private Vector2 _input;

    private void GatherInput(float delta)
    {
        _input = Input.GetVector("walk_left", "walk_right", "walk_up", "walk_down");

        if (!_rhytmicMode)
        {
            if (Input.IsActionPressed("fire") && _bulletCooldown.IsStopped())
            {
                Fire();
            }
            else if (Input.IsActionJustPressed("fire_alt") && _bulletCooldown.IsStopped())
            {
                FireLaser();
            }
        }
        else
        {
            if (Input.IsActionJustPressed("fire"))
            {
                FireOnBeat();
            }
        }

        // rhythm switch
        if (Input.IsActionPressed("rythm_switch"))
        {
            if (!_rhytmicMode)
            {
                GatherMusicTestRequirements();
            }
            else if (_rhytmicMode)
            {
                _musicPlayer.Stop();
                _beatTimer.Stop();
                _beatToleranceTimer.Stop();
            }

            _rhytmicMode = !_rhytmicMode;
        }

        // weapon switch
        if (Input.IsActionJustPressed("change_main_weapon"))
        {
            ChangeBulletPattern();
        }
        else if (Input.IsActionJustPressed("change_alt_weapon"))
        {
            ChangeLaserType();
        }
        if (Input.IsActionJustPressed("change_main_weapon_type"))
        {
            ChangeBulletType();
        }
        else if (Input.IsActionJustPressed("change_alt_weapon_type"))
        {
            ChangeLaserType();
        }


        // level up
        if (Input.IsActionJustPressed("open_level_up_tab"))
        {
            ToggleLevelUpTab();
        }
        // dialogue debus
        if (Input.IsActionJustPressed("next_dialogue"))
        {
            MakeDialogue();
        }

        // dash
        if (Input.IsActionJustPressed("player_dash"))
        {
            Dash();
        }
    }

    #endregion


    #region MOVEMENT

    private void UpdateMovement(float delta)
    {
        CalculateVelocity();
        UpdatePosition(delta);
        UpdateDash(delta);
    }

    public override void CalculateVelocity()
    {
        _velocity = _input * _moveSpeed;
    }

    public override void UpdatePosition(float delta)
    {
        Position += _velocity * delta;
    }

    #region DASH

    private Timer _dashCooldown;
    private float _dashDistance = 60f;
    private float _dashRemainingDistance = 0f;
    private float _dashSpeed = 900f;
    private Vector2 _dashDirection;

    private void Dash()
    {
        if (_dashCooldown.IsStopped())
        {
            _dashCooldown.Start();
            _isInvincible = true;
            _dashRemainingDistance = _dashDistance;
            _dashDirection = _velocity.Normalized();
            _sprite.Modulate = Colors.Cyan;
        }
    }

    private void InitialiseDash()
    {
        _dashCooldown = GetNode<Timer>("DashTimer");
        _dashCooldown.WaitTime = 0.5f;
        _dashCooldown.OneShot = true;
    }

    private void UpdateDash(float delta)
    {
        if (_dashRemainingDistance > 0)
        {
            float distanceToMove = Math.Min(_dashSpeed * delta, _dashRemainingDistance); // min to prevent overshooting
            Position += _dashDirection * distanceToMove;
            _dashRemainingDistance -= distanceToMove;
        }
        else
        {
            _isInvincible = false;
            _sprite.Modulate = Colors.White;
        }
    }

    #endregion // DASH

    #endregion // MOVEMENT


    #region ATTACKS

    private PackedScene _bullet;

    private Timer _bulletCooldown;

    [Export]
    private float _buletSpeed = 250f;
    [Export]
    private float _bulletCurveRadius = 50f;
    [Export]
    private float _bulletSpinSpeed = 50f;
    [Export]
    private int _bulletDamage = 1;

    private int _firedBulletCount = 0;

    #region BULLETS
    private enum PlayerBulletPatterns
    {
        STRAIGHT,
        CURVE,
        WIGGLE,
        HOMING,
        HOMING_DOUBLE
    }

    private PlayerBulletPatterns _bulletPattern;
    private PlayerBulletPatterns[] _bulletPatterns = new PlayerBulletPatterns[] {
        PlayerBulletPatterns.STRAIGHT, PlayerBulletPatterns.CURVE, PlayerBulletPatterns.WIGGLE,
        PlayerBulletPatterns.HOMING, PlayerBulletPatterns.HOMING_DOUBLE};

    /*
        private void Fire()
        {
            var bullet = BulletPool.Instance.GetBullet(); //todo change the bullet pool functionality, also unify the bullet creation, give entities the control of how the bullets behave and look, give them a pool of bullets of certain type to get bullets from, then bullets shouldnt reset too much, only what they actually need to reset
            bullet.Position = Position;
            Vector2 bulletDirection = GetGlobalMousePosition() - Position;
            bullet.SetDirection(bulletDirection.Normalized());
            bullet._bulletSource = this;
            bullet.Damage = _bulletDamage;
            bullet.Modulate = Colors.Green;

            bullet.BulletType = BulletType.Player;

            //bullet.Pattern = SetBulletPattern(PlayerBulletPatterns.CURVE, bullet);
            bullet.Pattern = SetBulletPattern(_bulletPattern, bullet);


            for (int i = 0; i < 2; i++)
            {
                var bullet2 = BulletPool.Instance.GetBullet();
                bullet2.Position = Position;
                bullet2.SetDirection(bulletDirection.Normalized());
                bullet2._bulletSource = this;
                bullet2.Damage = _bulletDamage;
                bullet2.Pattern = SetBulletPattern(PlayerBulletPatterns.CURVE, bullet);
                bullet2.Modulate = Colors.Green;
                _firedBulletCount++;
            }


            _bulletCooldown.Start();

            _firedBulletCount++;
        }*/

    private void Fire()
    {
        var bullet = BulletPool.Instance.GetEntityBullet(_bulletType, this); //todo change the bullet pool functionality, also unify the bullet creation, give entities the control of how the bullets behave and look, give them a pool of bullets of certain type to get bullets from, then bullets shouldnt reset too much, only what they actually need to reset
        bullet.Position = Position;
        Vector2 bulletDirection = GetGlobalMousePosition() - Position;
        bullet.SetDirection(bulletDirection.Normalized());
        bullet._bulletSource = this;
        bullet.Damage = _bulletDamage;
        bullet.Modulate = Colors.Green;

        bullet.BulletType = _bulletType;

        //bullet.Pattern = SetBulletPattern(PlayerBulletPatterns.CURVE, bullet);
        bullet.Pattern = SetBulletPattern(_bulletPattern, bullet);

        /*
                for (int i = 0; i < 2; i++)
                {
                    var bullet2 = BulletPool.Instance.GetEntityBullet(_bulletType, this);
                    bullet2.Position = Position;
                    bullet2.SetDirection(bulletDirection.Normalized());
                    bullet2._bulletSource = this;
                    bullet2.Damage = _bulletDamage;
                    bullet2.Pattern = SetBulletPattern(PlayerBulletPatterns.CURVE, bullet);
                    bullet2.Modulate = Colors.Green;

                    bullet2.BulletType = _bulletType;

                    _firedBulletCount++;
                }
        */

        _bulletCooldown.Start();

        _firedBulletCount++;
    }

    private BulletPattern SetBulletPattern(PlayerBulletPatterns pattern, Bullet bullet)
    {
        switch (pattern)
        {
            case PlayerBulletPatterns.STRAIGHT:
                /*return new StraightPattern
                {
                    BulletSpeed = _buletSpeed
                };*/
                /*
                return new CurvePatternDecorator(new CurvePattern(
                    Position + bullet.Direction.Normalized() * 250f,
                    _bulletCurveRadius,
                    _bulletSpinSpeed,
                    _buletSpeed,
                    2 * Mathf.Pi,
                    (_firedBulletCount % 2 == 0) ? CurvePattern.CURVE_DIRECTION_UP : CurvePattern.CURVE_DIRECTION_DOWN),
                    new CenterReturnDecorator(new StraightPattern() { BulletSpeed = _buletSpeed }, Position.X, 50f))
                {
                    BulletSpeed = _buletSpeed
                };*/
                return new CurvePatternDecorator(new CurvePattern(
                        Position + bullet.Direction.Normalized() * 250f,
                        _bulletCurveRadius,
                        _bulletSpinSpeed,
                        _buletSpeed,
                        2 * Mathf.Pi,
                        (_firedBulletCount % 2 == 0) ? CurvePattern.CURVE_DIRECTION_UP : CurvePattern.CURVE_DIRECTION_DOWN),
                    new HomingDecorator(
                        new StraightPattern() { BulletSpeed = _buletSpeed },
                        GetClosestEnemy(),
                        2.5f))
                {
                    BulletSpeed = _buletSpeed
                };
            case PlayerBulletPatterns.CURVE:
                return new CurvePattern
                {
                    AnchorPoint = Position + bullet.Direction.Normalized() * 250f,
                    CurveRadius = _bulletCurveRadius,
                    SpinSpeed = _bulletSpinSpeed,
                    BulletSpeed = _buletSpeed,
                    MaxRotation = 2 * Mathf.Pi,
                    CurveDirection = (_firedBulletCount % 2 == 0) ? CurvePattern.CURVE_DIRECTION_UP : CurvePattern.CURVE_DIRECTION_DOWN
                };
            case PlayerBulletPatterns.WIGGLE:
                return new WigglePattern
                {
                    Amplitude = 50f,
                    Angle = 45f,
                    InitialPosition = Position,
                    BulletSpeed = _buletSpeed,
                };
            case PlayerBulletPatterns.HOMING:
                return new HomingBullet
                {
                    Target = GetClosestEnemy(),
                    HomingSpeed = 100f,
                    BulletSpeed = _buletSpeed,
                };
            case PlayerBulletPatterns.HOMING_DOUBLE:
                /*
                    return new DoubleHomingBullet
                    {
                        Target = GetClosestEnemy(),
                        HomingSpeed = 100f,
                        BulletSpeed = _buletSpeed,

                        AnchorPoint = Position + bullet.Direction.Normalized() * 250f,
                        CurveRadius = _bulletCurveRadius,
                        SpinSpeed = _bulletSpinSpeed,
                        MaxRotation = 2 * Mathf.Pi,
                        CurveDirection = (_firedBulletCount % 2 == 0) ? CurvePattern.CURVE_DIRECTION_UP : CurvePattern.CURVE_DIRECTION_DOWN
                    };*/
                return new DoubleHomingBullet
                {
                    Target = GetClosestEnemy(),
                    HomingSpeed = 100f, // 100 for very snappy, about 25 is like isaac's tiny planet lmao
                    BulletSpeed = _buletSpeed,

                    AnchorPoint = Position + bullet.Direction.Normalized() * 250f,
                    CurveRadius = _bulletCurveRadius,
                    SpinSpeed = _bulletSpinSpeed,
                    MaxRotation = 2 * Mathf.Pi,
                    CurveDirection = (_firedBulletCount % 2 == 0) ? CurvePattern.CURVE_DIRECTION_UP : CurvePattern.CURVE_DIRECTION_DOWN
                };
            default:
                return null;
        }
    }

    #endregion

    #region LASERS

    private LaserPattern _laserType = LaserPattern.Single;
    private LaserPattern[] _laserTypes = new LaserPattern[] { LaserPattern.Single, LaserPattern.Triple,
        LaserPattern.Star, LaserPattern.StarWiggly };

    private void FireLaser()
    {
        //Laser laser = LaserPool.Instance.GetLaser(LaserPattern.Triple);

        Vector2 laserDirection = GetGlobalMousePosition() - Position;
        /*
        laser.SetLaser(
            this,
            Position,
            laserDirection.Normalized(),
            0f, // initial length
            500f, // max length
            2f, // width
            100f, // speed
            1 // damage
        );*/



        /*
                TripleLaser tripleLaser = (TripleLaser)laser;
                tripleLaser.ShootBeams(
                    this,
                    Position,
                    laserDirection.Normalized(),
                    0f, // initial length
                    500f, // max length
                    2f, // width
                    100f, // speed
                    1 // damage
                );*/

        Laser laser = LaserPool.Instance.GetEntityLaser(_laserType, this);
        laser.ShootBeams(
            this,
            Position,
            laserDirection.Normalized(),
            0f, // initial length
            500f, // max length
            2f, // width
            100f, // speed
            1 // damage
        );

        _bulletCooldown.Start();
    }

    #endregion


    private void ChangeBulletPattern()
    {
        _bulletPattern = _bulletPatterns[((int)_bulletPattern + 1) % _bulletPatterns.Length];
        _bulletPatternLabel.Text = _bulletPattern.ToString();
    }

    BulletType[] _bulletTypes = new BulletType[] { BulletType.Player, BulletType.Big };
    BulletType _bulletType = BulletType.Player;

    private void ChangeBulletType()
    {
        _bulletType = _bulletTypes[(System.Array.IndexOf(_bulletTypes, _bulletType) + 1) % _bulletTypes.Length];
        _bulletPatternLabel.Text = _bulletType.ToString();
    }

    private void ChangeLaserType()
    {
        _laserType = _laserTypes[(System.Array.IndexOf(_laserTypes, _laserType) + 1) % _laserTypes.Length];
        _bulletPatternLabel.Text = _laserType.ToString();
    }

    public override void CreateBulletPool()
    {
        BulletTypePool[] bulletTypePools = new BulletTypePool[]
        {
            new BulletTypePool { Type = BulletType.Player, PoolSize = 100 },
            new BulletTypePool { Type = BulletType.Big, PoolSize = 100 },
        };

        BulletPool.Instance.PopulateBulletPool(bulletTypePools, this);
    }

    public override void CreateLaserPool()
    {
        LaserPatternPool[] laserPatternPools = new LaserPatternPool[]
        {
            new LaserPatternPool { LaserPattern = LaserPattern.Single, PoolSize = 40 },
            new LaserPatternPool { LaserPattern = LaserPattern.Triple, PoolSize = 10 },
            new LaserPatternPool { LaserPattern = LaserPattern.Star, PoolSize = 10 },
            new LaserPatternPool { LaserPattern = LaserPattern.StarWiggly, PoolSize = 10 },
        };

        //LaserPool.Instance.PopulateEntityLaserPool(_laserTypes, this);
        LaserPool.Instance.PopulateEntityLaserPool(laserPatternPools, this);
    }


    public Enemy GetClosestEnemy()
    {
        var enemies = GetTree().GetNodesInGroup("Enemies");
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;
        foreach (Enemy enemy in enemies)
        {
            float distance = (enemy.Position - Position).Length();
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        GD.Print("closest enemy: " + closestEnemy.GetType());
        return closestEnemy;
    }

    #endregion


    #region MUSICTEST

    private AudioStreamPlayer2D _musicPlayer;
    private Timer _beatTimer;
    private Timer _beatToleranceTimer;
    private ColorRect _beatIndicator;
    [Export]
    public float _bpm;
    private float _beatInterval;
    private bool _onBeat;
    private bool _rhytmicMode;

    private PackedScene _rhytmBullet;

    /// <summary>
    /// Gather all requirements for the music test, set the beat timer and start the music.
    /// </summary>
    private void GatherMusicTestRequirements()
    {
        _musicPlayer = GetNode<AudioStreamPlayer2D>("MusicPlayer");
        _beatTimer = GetNode<Timer>("BeatTimer");
        _beatToleranceTimer = GetNode<Timer>("BeatTolerance");
        _beatInterval = 60f / _bpm; // time between beats in seconds
        _rhytmBullet = ResourceLoader.Load<PackedScene>("res://Player/Attacks/bullet.tscn");
        _beatIndicator = GetNode<ColorRect>("BeatIndicator");
        _onBeat = false;

        _beatTimer.Start(_beatInterval);
        _musicPlayer.Play();
        _beatIndicator.Visible = true;
    }

    /// <summary>
    /// if the player presses the fire button on beat, fire a bullet.
    /// </summary>
    private void FireOnBeat()
    {
        if (!_onBeat) return;

        var bullet = (Bullet)_rhytmBullet.Instantiate();
        GetTree().Root.AddChild(bullet);
        bullet.Position = Position;
        Vector2 bulletDirection = GetGlobalMousePosition() - Position;
        bullet.SetDirection(bulletDirection.Normalized());
    }

    /// <summary>
    /// Called when the beat timer times out.
    /// </summary>
    private void OnBeat()
    {
        _onBeat = true;
        _beatIndicator.Color = new Color(1, 1, 1, 1);
        _beatToleranceTimer.Start(0.05f);
        GD.Print("BEAT");
    }

    /// <summary>
    /// called when the beat tolerance timer times out.
    /// </summary>
    private void OnBeatToleranceTimeout()
    {
        _onBeat = false;
        _beatIndicator.Color = new Color(0, 0, 0, 1);
    }

    #endregion


    #region GUI

    private Control _gui;
    private Label _bulletPatternLabel;
    private Label _laserTypeLabel;
    private Label _powerLabel;
    private ProgressBar _expBar;

    private Control _levelUpTab;
    private Label _levelLabel;
    private Label _attributePointsLabel;

    private void UpdateGUI()
    {
        //_gui.GetNode<Label>("VBoxContainer/BulletPattern").Text = _bulletPattern.ToString();
        //_gui.GetNode<Label>("VBoxContainer/LaserType").Text = _laserType.ToString();
    }

    private void InitialiseGUI()
    {
        _gui = GetTree().Root.GetNode<Control>("Game/PlayerGUI");
        _bulletPatternLabel = _gui.GetNode<Label>("VBoxContainer/BulletPattern");
        _laserTypeLabel = _gui.GetNode<Label>("VBoxContainer/LaserType");
        _powerLabel = _gui.GetNode<Label>("VBoxContainer/Power");
        _expBar = _gui.GetNode<ProgressBar>("VBoxContainer/ExpBar");

        _levelUpTab = _gui.GetNode<Control>("LevelUpTab");
        _levelUpTab.Hide();
        _levelLabel = _levelUpTab.GetNode<Label>("PanelContainer/CenterContainer/Level");
        _attributePointsLabel = _levelUpTab.GetNode<Label>("PanelContainer/CenterContainer/AttributePoints");
        _levelLabel.Text = "Level: " + _level.ToString();
        _attributePointsLabel.Text = "Attribute Points: " + attributePoints.ToString();

        _bulletPatternLabel.Text = _bulletPattern.ToString();
        _laserTypeLabel.Text = _laserType.ToString();

        _powerLabel.Text = "Power: " + _power.ToString();

        UpdateLevelBar();


        InitialiseDialogueWindow();
    }

    private void ToggleLevelUpTab()
    {
        if (_levelUpTab.Visible)
        {
            _levelUpTab.Hide();
        }
        else
        {
            _levelUpTab.Show();
        }
    }

    private void UpdateLevelBar()
    {
        _expBar.MaxValue = _expReq;
        _expBar.Value = _exp;
    }

    #region DIALOGUE
    private DialogueWindow _dialogueWindow;
    private string _dialogueName;
    private Texture2D _dialoguePortrait;

    private void InitialiseDialogueWindow()
    {
        _dialogueWindow = GetTree().GetNodesInGroup("GUI")[0] as DialogueWindow;
        _dialogueName = "Player";
        _dialoguePortrait = ResourceLoader.Load<Texture2D>("res://GUI/Dialogue/Portraits/testcharacterface1.png");
    }

    private void MakeDialogue()
    {
        _dialogueWindow.CreateDialogue(_dialogueName, "test dialogue", _dialoguePortrait);
    }

    #endregion

    #endregion

    #region POWERUPS

    public float PickupDistanceModifier { get; set; } = 1f;
    private int _power = 0;

    public void IncreasePower(int power)
    {
        _power += power;
        PowerUpAttacks();
        _powerLabel.Text = "Power: " + _power.ToString();
    }

    private void PowerUpAttacks()
    {
        //todo powering up
    }

    #endregion

    #region EXPERIENCE
    private int _level = 1;
    private int _exp = 0;
    private int _expReq = 100;
    private int attributePoints = 0;

    public void IncreaseExp(int exp)
    {
        _exp += exp;
        if (_exp >= _expReq)
        {
            LevelUp();
        }

        UpdateLevelBar();
    }

    private void LevelUp()
    {
        _level++;
        attributePoints++;
        _levelLabel.Text = "Level: " + _level.ToString();
        _attributePointsLabel.Text = "Attribute Points: " + attributePoints.ToString();

        _exp -= _expReq;
        _expReq += 100;
    }

    #endregion

    #region INVINCIBILITY

    private bool _isInvincible = false;
    private Timer _invincibilityTimer;

    private void InitInvincibilityTimer()
    {
        _invincibilityTimer = GetNode<Timer>("InvincibilityTimer");
        _invincibilityTimer.WaitTime = 0.2f;
        _invincibilityTimer.OneShot = true;
    }

    private void StartInvincibilityTimer()
    {
        _isInvincible = true;
        _invincibilityTimer.Start();
    }

    private void UpdateInvincibility(float delta)
    {
    }

    private void OnInvincibilityTimeout()
    {
        _isInvincible = false;
    }

    #endregion

    #region DEATH

    /// <summary>
    /// for now, it will only reset player's health
    /// </summary>
    public override void Die()
    {
        _currentHealth = _maxHealth;
    }

    #endregion
}
