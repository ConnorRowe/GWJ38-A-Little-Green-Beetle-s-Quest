using Godot;
using System;

namespace BeetleGame
{
    public class TwitterLink : HBoxContainer
    {
        private bool isMouseOver = false;

        [Export]
        private Color textColor = new Color(.3f, .17f, .2f);

        public override void _Ready()
        {
            Connect("mouse_entered", this, nameof(MouseEntered));
            Connect("mouse_exited", this, nameof(MouseExited));

            GetNode<Label>("Label").SetDeferred("custom_colors/font_color", textColor);
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (isMouseOver && evt is InputEventMouseButton emb && !emb.Pressed)
            {
                OS.ShellOpen("https://twitter.com/MagsonConnor");
            }
        }

        private void MouseEntered()
        {
            isMouseOver = true;
        }

        private void MouseExited()
        {
            isMouseOver = false;
        }
    }
}