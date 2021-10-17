using Godot;

namespace BeetleGame
{
    public class SettingsMenu : MenuScreen
    {
        public override void _Ready()
        {
            base._Ready();

            var settings = GetNode<Settings>("Panel/MarginContainer/Settings");

            var returnButton = GetNode<Control>("ReturnButton");

            returnButton.FocusNeighbourTop = GetPathTo(settings.CursorToggle);
            returnButton.FocusPrevious = returnButton.FocusNeighbourTop;
        }
    }
}
