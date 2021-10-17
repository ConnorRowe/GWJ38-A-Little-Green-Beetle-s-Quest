using Godot;

namespace BeetleGame
{
    public class QuestObject : Node2D
    {
        [Signal]
        public delegate void Interacted(QuestObject questObject);

        [Export]
        private NodePath _area2DPath;

        private Area2D area;

        public override void _Ready()
        {
            if (_area2DPath != null && !_area2DPath.IsEmpty())
            {
                area = GetNode<Area2D>(_area2DPath);
                area.Connect("body_entered", this, nameof(BodyEntered));
            }
        }

        private void BodyEntered(Node body)
        {
            if (body is Projectile proj && proj.IsPlayers)
            {
                proj.Hit();

                ((Player)proj.Source).Interacted(this);
                EmitSignal(nameof(Interacted), this);

                GD.Print("Interact");
            }
        }

        public void SetActive(bool isActive)
        {
            area.SetDeferred("monitorable", isActive);
            area.SetDeferred("monitoring", isActive);
        }

        public void ForceEmitInteracted()
        {
            EmitSignal(nameof(Interacted), this);
        }
    }
}