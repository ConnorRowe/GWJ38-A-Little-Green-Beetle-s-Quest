using Godot;

namespace BeetleGame
{
    public class SmokePuff : Particles2D
    {
        [Export]
        private bool AutoPlay = false;

        public override void _Ready()
        {
            base._Ready();

            if (AutoPlay)
                Start();
        }

        public void Start()
        {
            Emitting = true;
            GetTree().CreateTimer(1.5f).Connect("timeout", this, nameof(Die));
            GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();
        }

        private void Die()
        {
            QueueFree();
        }
    }
}