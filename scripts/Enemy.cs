using Godot;

namespace BeetleGame
{
    public class Enemy : KinematicBody2D
    {
        [Export]
        private Shape2D attackShape;

        private enum EnemyState
        {
            Roaming,
            Chasing,
            Attacking,
            Returning,
            Dead
        }

        private AnimatedSprite enemySprite;
        private AnimatedSprite alertSprite;
        private AnimatedSprite slashSprite;
        private AnimationPlayer animPlayer;
        private Timer roamTimer;
        private Physics2DShapeQueryParameters attackShapeQuery;
        private AudioStreamPlayer hitPlayer;
        private Area2D hitbox;

        private Vector2 inputDir = Vector2.Zero;
        private float acceleration = 150f;
        private float maxSpeed = 400f;
        private float movementDampening = 1.5f;
        private Vector2 velocity = Vector2.Zero;
        private Vector2 externalVelocity = Vector2.Zero;
        private Player player = null;
        private EnemyState state = EnemyState.Roaming;
        private Vector2 homePosition;
        private Vector2 roamPosition;
        private const float roamRange = 64f;
        private const float attackRange = 32f;
        private const float maxChaseRange = 512f;
        private bool hasHitPlayer = false;
        private bool canAttack = true;
        private int health = 3;
        private bool isDead = false;

        public override void _Ready()
        {
            base._Ready();

            enemySprite = GetNode<AnimatedSprite>("EnemySprite");
            alertSprite = GetNode<AnimatedSprite>("AlertSprite");
            slashSprite = GetNode<AnimatedSprite>("SlashSprite");
            animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            roamTimer = GetNode<Timer>("RoamTimer");
            hitPlayer = GetNode<AudioStreamPlayer>("HitPlayer");

            roamTimer.Connect("timeout", this, nameof(PickNewRoamPoint));
            homePosition = Position;
            roamPosition = Position;

            attackShapeQuery = new Physics2DShapeQueryParameters();
            attackShapeQuery.SetShape(attackShape);
            attackShapeQuery.Transform = new Transform2D(0.0f, Position);
            attackShapeQuery.CollideWithAreas = true;
            attackShapeQuery.CollisionLayer = 2;

            hitbox = GetNode<Area2D>("Hitbox");
            hitbox.Connect("body_entered", this, nameof(HitboxBodyEntered));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            switch (state)
            {
                case EnemyState.Roaming:
                    {
                        if (player != null && Position.DistanceTo(player.Position) <= 128f)
                            SetState(EnemyState.Chasing);

                        break;
                    }
                case EnemyState.Chasing:
                    {
                        if (canAttack && Position.DistanceTo(player.Position) <= attackRange)
                        {
                            SetState(EnemyState.Attacking);
                            StartAttack();
                        }
                        else if (Position.DistanceTo(homePosition) > maxChaseRange)
                            SetState(EnemyState.Returning);

                        break;
                    }
                case EnemyState.Returning:
                    {
                        if (Position.DistanceTo(homePosition) <= roamRange)
                            SetState(EnemyState.Roaming);

                        break;
                    }
                case EnemyState.Attacking:
                    {
                        if (slashSprite.Frame == 8)
                        {
                            state = EnemyState.Chasing;
                        }

                        break;
                    }
                case EnemyState.Dead:
                    break;
            }

            inputDir = GetAxis();

            GetNode<Label>("DebugLabel").Text = StateToStr(state);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (state == EnemyState.Attacking && !hasHitPlayer && slashSprite.Frame >= 2 && slashSprite.Frame <= 5)
            {
                attackShapeQuery.Transform = new Transform2D(0f, Position);
                var intersect = GetWorld2d().DirectSpaceState.IntersectShape(attackShapeQuery, 1);

                foreach (Godot.Collections.Dictionary hit in intersect)
                {
                    if (((Node)hit["collider"]).Owner is Player p)
                    {
                        hasHitPlayer = true;
                        player.TakeDamage();
                        player.ApplyExternalImpulse(Position.DirectionTo(p.Position) * 200f);
                    }
                }
            }

            velocity -= (velocity * movementDampening * delta);

            velocity += inputDir * acceleration * delta;

            externalVelocity -= (externalVelocity * 2f * delta);

            if (velocity.Length() > GetMaxSpeed())
            {
                velocity = velocity.Normalized() * GetMaxSpeed();
            }

            if (!isDead)
            {
                if (velocity.Length() <= .5f)
                {
                    enemySprite.Animation = "idle";
                    enemySprite.SpeedScale = 1f;
                }
                else
                {
                    enemySprite.Animation = "moving";
                    enemySprite.SpeedScale = ((velocity.LengthSquared() + externalVelocity.LengthSquared()) * delta * .03f);
                }
            }

            enemySprite.FlipH = velocity.x > 0;

            MoveAndSlide(velocity + externalVelocity);
        }

        private Vector2 GetAxis()
        {
            switch (state)
            {
                case EnemyState.Roaming:
                    {
                        return Position.DirectionTo(roamPosition);
                    }
                case EnemyState.Chasing:
                    {
                        return Position.DirectionTo(player.Position);
                    }
                case EnemyState.Returning:
                    {
                        return Position.DirectionTo(homePosition);
                    }
                case EnemyState.Attacking:
                case EnemyState.Dead:
                    break;
            }

            return Vector2.Zero;
        }

        public void DetectPlayer(Player p)
        {
            if(isDead)
                return;

            player = p;

            if (Position.DistanceTo(player.Position) <= attackRange)
                SetState(EnemyState.Attacking);
            else
                SetState(EnemyState.Chasing);

            alertSprite.Play("default");
            GetTree().CreateTimer(2f).Connect("timeout", this, nameof(HideAlert));
        }

        private void HideAlert()
        {
            alertSprite.Play("default", backwards: true);
        }

        public void ForgetPlayer()
        {
            if(isDead)
                return;

            if (GlobalPosition.DistanceTo(homePosition) > roamRange)
            {
                SetState(EnemyState.Returning);
            }
            else
            {
                SetState(EnemyState.Roaming);
            }
        }

        private void PickNewRoamPoint()
        {
            roamPosition = homePosition + (new Vector2(GlobalNodes.RNG.Randf(), GlobalNodes.RNG.Randf()).Normalized() * GlobalNodes.RNG.Randf() * roamRange);

            roamTimer.WaitTime = GlobalNodes.RNG.RandfRange(1f, 3f);
        }

        private void SetState(EnemyState newState)
        {
            GD.Print($"enemy state changed to {StateToStr(newState)}");

            state = newState;
        }

        private string StateToStr(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Roaming:
                    return "Roaming";

                case EnemyState.Chasing:
                    return $"Chasing ~ {Position.DistanceTo(player.Position)} <= {attackRange}";

                case EnemyState.Returning:
                    return "Returning";

                case EnemyState.Attacking:
                    return "Attacking";

                case EnemyState.Dead:
                    return "Dead";


            }

            return "error";
        }

        private float GetMaxSpeed()
        {
            return state == EnemyState.Roaming ? maxSpeed / 3f : maxSpeed;
        }

        private void StartAttack()
        {
            slashSprite.Rotation = Position.AngleToPoint(player.Position);
            hasHitPlayer = false;
            slashSprite.Frame = 0;
            slashSprite.Play("default");
            canAttack = false;
            GetTree().CreateTimer(1f).Connect("timeout", this, nameof(ResetCanAttack));
        }

        private void ResetCanAttack()
        {
            canAttack = true;
        }

        public void TakeDamage()
        {
            if (health > 0)
            {
                health--;

                hitPlayer.Play();

                if (health <= 0)
                {
                    Die();
                }
            }
        }

        private void Die()
        {
            isDead = true;

            SetState(EnemyState.Dead);

            enemySprite.Animation = "idle";
            enemySprite.SpeedScale = 0f;
            enemySprite.FlipV = true;

            hitbox.CollisionLayer = 0;
            hitbox.CollisionMask = 0;
            CollisionLayer = 0;
            CollisionMask = 0;
            GetNode<Area2D>("PlayerDetection").CollisionLayer = 0;
            roamTimer.Stop();

            movementDampening *= 4f;

            GetNode<QuestObject>("QuestObject").ForceEmitInteracted();
        }

        private void HitboxBodyEntered(Node body)
        {
            if (body is Projectile proj && proj.IsPlayers)
            {
                TakeDamage();

                proj.Hit();
            }
        }
    }
}
