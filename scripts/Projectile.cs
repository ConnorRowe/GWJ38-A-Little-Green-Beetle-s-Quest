using Godot;

namespace BeetleGame
{
    public class Projectile : KinematicBody2D
    {
        public bool IsPlayers { get; set; } = true;
        public Node2D Source { get; set; }
        private Vector2 velocity;

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            var tryMove = MoveAndCollide(velocity * delta, false);

            if (tryMove != null)
            {
                if (tryMove.Collider is Player player)
                {
                    //hit
                }

                Hit();
            }
        }

        public void SetVelocity(Vector2 velocity)
        {
            this.velocity = velocity;
        }

        public void Hit()
        {
            QueueFree();
        }
    }
}