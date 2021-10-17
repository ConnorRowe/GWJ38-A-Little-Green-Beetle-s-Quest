using Godot;

namespace BeetleGame
{
    public class GlobalNodes : Node
    {
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();

        public AudioStreamPlayer BackgroundMusic { get; set; }

        static GlobalNodes()
        {
            RNG.Randomize();
        }

        public override void _Ready()
        {
            BackgroundMusic = GetNode<AudioStreamPlayer>("BackgroundMusic");
            BackgroundMusic.Play();

            Input.SetCustomMouseCursor(GD.Load<Texture>("res://textures/customcursor.png"), hotspot: new Vector2(21, 21));
        }

    }
}