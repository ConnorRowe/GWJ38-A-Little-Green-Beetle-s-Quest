using Godot;

namespace BeetleGame
{
    public class MenuScreen : Node2D
    {
        private PackedScene mainMenuScene;

        [Export]
        private NodePath returnButtonPath;

        public override void _Ready()
        {
            base._Ready();

            mainMenuScene = GD.Load<PackedScene>("res://scenes/MainMenu.tscn");

            var button = GetNode<Control>(returnButtonPath);

            if (Input.GetConnectedJoypads().Count > 0)
                button.GrabFocus();

            button.Connect("pressed", this, nameof(ReturnToMainMenu));
        }

        private void ReturnToMainMenu()
        {
            GetTree().ChangeSceneTo(mainMenuScene);

            QueueFree();
        }
    }
}