using Godot;

namespace BeetleGame
{
    public class GlobalInputEvents : Node
    {
        public override void _Ready()
        {
            base._Ready();
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt is InputEventKey ek && !ek.Pressed && (ek.Scancode == (uint)KeyList.F11 || (ek.Scancode == (uint)KeyList.Enter && ek.Alt)))
            {
                OS.WindowFullscreen = !OS.WindowFullscreen;
            }
        }
    }
}