using Godot;

namespace BeetleGame
{
    public class NPC : QuestObject
    {
        private AnimatedSprite animatedSprite;
        private Timer timer;

        [Export]
        private SpriteFrames spriteFramesOverride;

        public override void _Ready()
        {
            base._Ready();

            animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
            timer = GetNode<Timer>("Timer");
            timer.Connect("timeout", this, nameof(Timeout));

            if (IsInstanceValid(spriteFramesOverride))
                animatedSprite.Frames = spriteFramesOverride;
        }

        private void Timeout()
        {
            animatedSprite.Frame = 0;
            animatedSprite.Play();

            timer.WaitTime = GlobalNodes.RNG.RandfRange(1f, 6f);
        }
    }
}