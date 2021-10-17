using Godot;

namespace BeetleGame
{
    public class ElementalFruit : QuestObject
    {
        public override void _Ready()
        {
            base._Ready();

            GetNode<AnimationPlayer>("AnimationPlayer").Play("ColourChange");
        }
    }
}
