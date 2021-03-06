using Godot;
using System;
using MD;

[MDAutoRegister]
public class Player : KinematicBody2D
{
    public class PlayerSettings
    {
        [MDReplicated]
        public Color PlayerColor { get; set; }

        [MDReplicated]
        public int PlayerShotCounter { get; set; }

        [MDReplicated]
        public String PlayerString { get; set; }
    }

    public const string PLAYER_GROUP = "PLAYERS";

    [Export]
    public float MaxSpeed = 150f;

    [Export]
    public float Acceleration = 2000f;

    [Export]
    public float WeaponCooldown = 1f;

    [MDBindNode("Camera2D")]
    protected Camera2D Camera;

    [MDBindNode("HitCounter")]
    protected Label HitCounter;

    protected bool IsLocalPlayer = false;

    protected Vector2 MovementAxis = Vector2.Zero;
    protected Vector2 Motion = Vector2.Zero;

    protected float WeaponActiveCooldown = 0f;

    protected float RsetActiveCooldown = 0f;

    protected PackedScene BulletScene = null;

    protected int HitCounterValue = 0;

    [MDReplicated(MDReliability.Unreliable, MDReplicatedType.Interval)]
    [MDReplicatedSetting(MDReplicator.Settings.GroupName, "PlayerPositions")]
    [MDReplicatedSetting(MDReplicator.Settings.ProcessWhilePaused, false)]
    [MDReplicatedSetting(MDReplicatedMember.Settings.OnValueChangedEvent, nameof(OnPositionChanged))]
    protected Vector2 NetworkedPosition;

    [MDReplicated(MDReliability.Reliable, MDReplicatedType.OnChange)]
    [MDReplicatedSetting(MDReplicatedMember.Settings.OnValueChangedEvent, nameof(UpdateColor))]
    protected PlayerSettings NetworkedPlayerSettings { get; set; }

    [Puppet]
    protected String RsetTest = "";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AddToGroup(PLAYER_GROUP);
        SetupPlayer(GetNetworkMaster());

        if (IsLocalPlayer)
        {
            RandomNumberGenerator rnd = new RandomNumberGenerator();
            rnd.Randomize();

            // Let's set our color
            NetworkedPlayerSettings = new PlayerSettings();
            NetworkedPlayerSettings.PlayerColor = new Color(rnd.Randf(), rnd.Randf(), rnd.Randf());
            NetworkedPlayerSettings.PlayerShotCounter = 0;
            NetworkedPlayerSettings.PlayerString = $"Some string {rnd.RandiRange(0, 10000)}";
            Modulate = NetworkedPlayerSettings.PlayerColor;
        }
        else
        {
            MDOnScreenDebug.AddOnScreenDebugInfo("RsetTest " + GetNetworkMaster().ToString(),
                () => { return RsetTest; });
        }
    }

    protected void UpdateColor()
    {
        if (NetworkedPlayerSettings == null || NetworkedPlayerSettings.PlayerColor == null)
        {
            return;
        }
        GD.Print($"NetworkedPlayerSettings Update, Counter: {NetworkedPlayerSettings.PlayerShotCounter}");
        Modulate = NetworkedPlayerSettings.PlayerColor;
    }

    public void Hit()
    {
        HitCounterValue++;
        HitCounter.Text = HitCounterValue.ToString();
    }

    protected void OnPositionChanged()
    {
        if (!IsLocalPlayer)
        {
            Position = NetworkedPosition;
        }
    }

    [Remote]
    protected void OnShoot(Vector2 Target)
    {
        if (Target != Vector2.Zero)
        {
            Bullet bullet = (Bullet) GetBulletScene().Instance();
            bullet.GlobalPosition = GlobalPosition;
            bullet.SetOwner(GetNetworkMaster());
            GetParent().AddChild(bullet);
            bullet.SetTarget(Target);
        }
    }

    [Remote]
    protected void OnShoot()
    {
        GD.Print("OnShoot called");
    }

    [Remote]
    protected void OnShoot(String val, String val2)
    {
        GD.Print("OnShoot<String, String> called");
    }

    private PackedScene GetBulletScene()
    {
        if (BulletScene == null)
        {
            BulletScene = (PackedScene) ResourceLoader.Load(Filename.GetBaseDir() + "/Bullet.tscn");
        }

        return BulletScene;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsLocalPlayer)
        {
            WeaponActiveCooldown -= delta;
            RsetActiveCooldown -= delta;
            // Get input
            if (Input.IsMouseButtonPressed(1) && WeaponActiveCooldown <= 0f)
            {
                // Shoot towards mouse position
                this.MDRpc(nameof(OnShoot), GetGlobalMousePosition());
                this.MDRpc(nameof(OnShoot));
                this.MDRpc(nameof(OnShoot), "test", "test2");

                // Call it on local client, could do with RemoteSynch as well but then it won't work in standalone
                OnShoot(GetGlobalMousePosition());
                NetworkedPlayerSettings.PlayerShotCounter++;
                WeaponActiveCooldown = WeaponCooldown;
            }
            else if (Input.IsMouseButtonPressed(2) && RsetActiveCooldown <= 0f)
            {
                RandomNumberGenerator rnd = new RandomNumberGenerator();
                rnd.Randomize();
                this.MDRset(nameof(RsetTest), rnd.RandiRange(0, 100000).ToString());
                RsetActiveCooldown = 0.1f;
            }

            MovementAxis = GetInputAxis();

            // Move
            if (MovementAxis == Vector2.Zero)
            {
                ApplyFriction(Acceleration * delta);
            }
            else
            {
                ApplyMovement(MovementAxis * Acceleration * delta, MaxSpeed);
            }

            Motion = MoveAndSlide(Motion);
            NetworkedPosition = Position;
        }
    }

    protected virtual void ApplyMovement(Vector2 MovementSpeed, float Max)
    {
        this.Motion += MovementSpeed;
        this.Motion = Motion.Clamped(Max);
    }

    protected void ApplyFriction(float Amount)
    {
        if (Motion.Length() > Amount)
        {
            Motion -= Motion.Normalized() * Amount;
        }
        else
        {
            Motion = Vector2.Zero;
        }
    }

    protected Vector2 GetInputAxis()
    {
        Vector2 axis = Vector2.Zero;
        axis.x = IsActionPressed("ui_right") - IsActionPressed("ui_left");
        axis.y = IsActionPressed("ui_down") - IsActionPressed("ui_up");
        return axis.Normalized();
    }

    protected int IsActionPressed(String Action)
    {
        if (Input.IsActionPressed(Action))
        {
            return 1;
        }

        return 0;
    }

    public void SetupPlayer(int PeerId)
    {
        if (PeerId == MDStatics.GetPeerId())
        {
            Camera.Current = true;
            IsLocalPlayer = true;
        }
    }
}