using Godot;

namespace BeetleGame
{
    public class MainMenu : Node2D
    {
        public override void _Ready()
        {
            base._Ready();

            var playButton = GetNode<Control>("CanvasLayer/PlayButton");
            playButton.GrabFocus();
            playButton.Connect("pressed", this, nameof(GoToScene), new Godot.Collections.Array(GD.Load<PackedScene>("res://scenes/World.tscn")));

            GetNode<Control>("CanvasLayer/SettingsButton").Connect("pressed", this, nameof(GoToScene), new Godot.Collections.Array(GD.Load<PackedScene>("res://scenes/SettingsMenu.tscn")));
            GetNode<Control>("CanvasLayer/ControlsButton").Connect("pressed", this, nameof(GoToScene), new Godot.Collections.Array(GD.Load<PackedScene>("res://scenes/ControlsMenu.tscn")));
        }

        private void GoToScene(PackedScene scene)
        {
            GetTree().ChangeSceneTo(scene);

            QueueFree();
        }
    }
}
