using Godot;

namespace BeetleGame
{
    public class Player : KinematicBody2D
    {
        [Signal]
        public delegate void TookDamage();
        [Signal]
        public delegate void PlayerInteracted();
        [Signal]
        public delegate void Died();

        public World @World { get; set; }
        public Vector2 QuestTargetPos { get; set; } = Vector2.Zero;
        public bool IsInputLocked { get; set; } = false;
        public int Health { get; set; } = 4;
        public bool IsDead { get { return isDead; } }

        private Sprite eyeGlints;
        private AnimatedSprite head;
        private AnimationPlayer animationPlayer;
        private AnimatedSprite questArrow;
        private Area2D hitbox;
        private AudioStreamPlayer hitPlayer;
        private AudioStreamPlayer footstepPlayer;

        private Vector2 inputDir = Vector2.Zero;
        private float acceleration = 250f;
        private float maxSpeed = 300f;
        private float movementDampening = 3f;
        private Vector2 velocity = Vector2.Zero;
        private Vector2 externalVelocity = Vector2.Zero;
        private bool canTakeDamage = true;
        private bool isDead = false;

        public override void _Ready()
        {
            eyeGlints = GetNode<Sprite>("Body/Head/EyeGlints");
            head = GetNode<AnimatedSprite>("Body/Head");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animationPlayer.Play("Walk");
            World = (World)GetTree().CurrentScene;
            questArrow = GetNode<AnimatedSprite>("QuestArrow");
            hitbox = GetNode<Area2D>("Body/Hitbox");
            hitbox.Connect("area_entered", this, nameof(HitboxAreaEntered));
            hitbox.Connect("area_exited", this, nameof(HitboxAreaExited));
            hitPlayer = GetNode<AudioStreamPlayer>("HitPlayer");
            footstepPlayer = GetNode<AudioStreamPlayer>("FootstepPlayer");

            QuestTargetPos = new Vector2(506, 217);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            var mouseDir = head.GetLocalMousePosition().Normalized();

            eyeGlints.Position = new Vector2(0, 1f) + mouseDir * .5f;

            bool lookUp = mouseDir.y < 0;
            bool lookLeft = mouseDir.x < 0;

            head.Animation = lookUp ? "bounce_behind" : "bounce";
            head.FlipH = lookLeft;

            if (lookUp && lookLeft)
            {
                head.Offset = new Vector2(-2f, -5.5f);
            }
            else if (!lookUp && lookLeft)
            {
                head.Offset = new Vector2(-2f, -5.5f);
            }
            else if (lookUp && !lookLeft)
            {
                head.Offset = new Vector2(2f, -5.5f);
            }
            else if (!lookUp && !lookLeft)
            {
                head.Offset = new Vector2(2.5f, -5.5f);
            }

            head.ShowBehindParent = lookUp;
            eyeGlints.Visible = !lookUp;
            eyeGlints.Position = new Vector2(lookLeft ? -1.5f : 1.5f, -2.5f);

            if (QuestTargetPos != Vector2.Zero)
            {
                float angle = QuestTargetPos.AngleToPoint(GlobalPosition);
                int frame = Mathf.FloorToInt(Mathf.Rad2Deg(angle + Mathf.Pi) / 45f);

                questArrow.Frame = frame;
                questArrow.Offset = GlobalPosition.DirectionTo(QuestTargetPos) * 24f;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            inputDir = Vector2.Zero;

            if (!IsInputLocked && !isDead)
            {
                if (Input.IsActionPressed("g_move_up"))
                {
                    inputDir.y -= 1;
                }
                if (Input.IsActionPressed("g_move_down"))
                {
                    inputDir.y += 1;
                }
                if (Input.IsActionPressed("g_move_left"))
                {
                    inputDir.x -= 1;
                }
                if (Input.IsActionPressed("g_move_right"))
                {
                    inputDir.x += 1;
                }
            }

            inputDir = inputDir.Normalized();

            velocity -= (velocity * movementDampening * delta);

            velocity += inputDir * acceleration * delta;

            externalVelocity -= (externalVelocity * 2f * delta);

            if (velocity.Length() > maxSpeed)
            {
                velocity = velocity.Normalized() * maxSpeed;
            }

            animationPlayer.PlaybackSpeed = isDead ? 0f : ((velocity.LengthSquared() + externalVelocity.LengthSquared()) * delta * .015f);

            MoveAndSlide(velocity + externalVelocity);
        }

        public void ApplyExternalImpulse(Vector2 impulse)
        {
            externalVelocity += impulse;
        }

        private void HitboxAreaEntered(Area2D area)
        {
            if (area.Owner is Enemy enemy)
            {
                enemy.DetectPlayer(this);
            }

            GD.Print($"{area.Owner.Name} detected player.");
        }

        private void HitboxAreaExited(Area2D area)
        {
            if (area.Owner is Enemy enemy)
            {
                enemy.ForgetPlayer();
            }

            GD.Print($"Player exited {area.Owner.Name} detection area.");
        }

        public void TakeDamage()
        {
            if (canTakeDamage && Health > 0)
            {
                Health--;

                GD.Print("Player was hit.");
                hitPlayer.Play();

                canTakeDamage = false;
                GetTree().CreateTimer(.25f).Connect("timeout", this, nameof(ResetCanTakeDamage));

                if (Health <= 0)
                {
                    Die();
                }

                EmitSignal(nameof(TookDamage));
            }
        }

        private void ResetCanTakeDamage()
        {
            canTakeDamage = true;
        }

        private void Die()
        {
            GD.Print("Player dead.");
            isDead = true;

            EmitSignal(nameof(Died));

            Rotation = Mathf.Pi;
            questArrow.Visible = false;
        }

        public void Interacted(QuestObject questObject)
        {
            EmitSignal(nameof(PlayerInteracted), questObject);
        }

        public void Respawn()
        {
            Rotation = 0;
            questArrow.Visible = true;
            Health = 4;
            isDead = false;
        }

        public void ClearVelocity()
        {
            velocity = Vector2.Zero;
            externalVelocity = Vector2.Zero;
        }

        public void StartTimedInputLock(float timeSec)
        {
            IsInputLocked = true;

            GetTree().CreateTimer(timeSec).Connect("timeout", this, nameof(ResetInputLock));
        }

        private void ResetInputLock()
        {
            IsInputLocked = false;
        }
    }
}