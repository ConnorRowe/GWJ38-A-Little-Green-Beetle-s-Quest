using Godot;

namespace BeetleGame
{
    public class EndScreen : MenuScreen
    {
        private Tween tween;
        private CanvasModulate canvasModulate;
        private Label label;
        private AudioStreamPlayer blipPlayer;

        private float currentCharacters = 0;
        private float maxCharacters;
        private bool canIncreaseText = false;

        public override void _Ready()
        {
            base._Ready();

            tween = GetNode<Tween>("Tween");
            canvasModulate = GetNode<CanvasModulate>("CanvasModulate");
            label = GetNode<Label>("Panel/MarginContainer/Label");
            blipPlayer = GetNode<AudioStreamPlayer>("BlipPlayer");

            maxCharacters = label.GetTotalCharacterCount();
            label.VisibleCharacters = Mathf.RoundToInt(currentCharacters);
        
            tween.InterpolateProperty(canvasModulate, "color", Colors.Black, Colors.White, 3f, Tween.TransitionType.Back);
            tween.InterpolateProperty(GetNode<Panel>("Panel"), "rect_scale", Vector2.Zero, Vector2.One, 1.5f, Tween.TransitionType.Back, delay: 1f);
            tween.Start();

            GetTree().CreateTimer(2f).Connect("timeout", this, nameof(ResetCanIncreaseText));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (!canIncreaseText)
                return;

            int startChars = label.VisibleCharacters;

            if (currentCharacters < maxCharacters)
            {
                currentCharacters += delta * 15.0f;
            }

            if (currentCharacters > maxCharacters)
            {
                currentCharacters = maxCharacters;
            }

            label.VisibleCharacters = Mathf.RoundToInt(currentCharacters);

            if (label.VisibleCharacters > startChars)
            {
                blipPlayer.Play();
            }
        }

        private void ResetCanIncreaseText()
        {
            canIncreaseText = true;
        }
    }
}