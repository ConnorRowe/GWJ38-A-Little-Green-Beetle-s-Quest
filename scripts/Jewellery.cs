using Godot;

namespace BeetleGame
{
    public class Jewellery : Sprite
    {
        public QuestObject @QuestObject { get; set; }

        private Area2D area;

        public override void _Ready()
        {
            base._Ready();

            QuestObject = GetNode<QuestObject>("QuestObject");

            area = GetNode<Area2D>("Area2D");
            area.Connect("body_entered", this, nameof(BodyEntered));
        }

        private void BodyEntered(Node body)
        {
            if (body is Player player && !player.IsDead)
            {
                if (player.World.GetNode<QuestHandler>("QuestHandler").CurrentPhase.PhaseLocation == this.Position + new Vector2(0, -8))
                {
                    QuestObject.ForceEmitInteracted();

                    area.SetDeferred("monitoring", false);
                    Visible = false;
                }
            }
        }
    }
}
