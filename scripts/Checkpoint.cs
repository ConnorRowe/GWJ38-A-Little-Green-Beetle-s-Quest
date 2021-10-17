using Godot;

namespace BeetleGame
{
    public class Checkpoint : Area2D
    {
        public bool IsActive { get { return isActive; } set { SetIsActive(value); } }
        public QuestObject @QuestObject { get; set; }
        private bool isActive = false;
        private AnimatedSprite animatedSprite;

        public override void _Ready()
        {
            QuestObject = GetNode<QuestObject>("QuestObject");
            QuestObject.Connect(nameof(QuestObject.Interacted), this, nameof(Interacted));
            animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        }

        private void Interacted(QuestObject questObject)
        {
            if (!IsActive)
            {
                IsActive = true;
            }
        }

        public void SetIsActive(bool isActive)
        {
            this.isActive = isActive;

            if (isActive)
            {
                EmitBurst();
                animatedSprite.Frame = 1;
            }
            else
            {
                animatedSprite.Frame = 0;
            }
        }

        public void EmitBurst()
        {
            GetNode<Particles2D>("Puff1").Emitting = true;
            GetNode<Particles2D>("Puff2").Emitting = true;
        }
    }
}