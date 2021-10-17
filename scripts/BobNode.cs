using Godot;

namespace BeetleGame
{
    public class BobNode : Node2D
    {
        private Vector2 startPos;
        private float count = 0;

        [Export]
        private float bobAmount = 20f;

        public override void _Ready()
        {
            startPos = Position;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            count += delta * .25f;
            if (count > 1f)
                count--;

            Position = startPos + new Vector2(0, Mathf.Sin(count * Mathf.Tau) * bobAmount);
        }
    }
}
