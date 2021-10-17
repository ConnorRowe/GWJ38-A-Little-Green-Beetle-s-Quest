using Godot;

namespace BeetleGame
{
    public class CaveEntrance : StaticBody2D
    {
        public QuestObject @QuestObject { get; set; }

        private AnimatedSprite sprite;
        private CollisionPolygon2D openCollision;
        private CollisionPolygon2D closedCollision;
        private Area2D teleportArea;

        [Export]
        private bool isOpen = false;
        [Export]
        private Vector2 teleportLocation = Vector2.Zero;
        [Export]
        private string teleportName = "";
        [Export]
        private int cameraLimitLeft = 0;
        [Export]
        private int cameraLimitTop = 0;
        [Export]
        private int cameraLimitRight = 0;
        [Export]
        private int cameraLimitBottom = 0;

        private bool canTp = true;

        public override void _Ready()
        {
            base._Ready();

            QuestObject = GetNode<QuestObject>("QuestObject");
            sprite = GetNode<AnimatedSprite>("AnimatedSprite");
            openCollision = GetNode<CollisionPolygon2D>("OpenCollision");
            closedCollision = GetNode<CollisionPolygon2D>("ClosedCollision");

            teleportArea = GetNode<Area2D>("TeleportArea");
            teleportArea.Connect("body_entered", this, nameof(BodyEntered));

            ToggleOpen(isOpen);
        }

        public void ToggleOpen(bool toggle)
        {
            if (toggle)
            {
                sprite.Frame = 0;
                openCollision.Disabled = false;
                closedCollision.Disabled = true;
                teleportArea.Monitoring = true;
            }
            else
            {
                sprite.Frame = 1;
                openCollision.Disabled = true;
                closedCollision.Disabled = false;
                teleportArea.Monitoring = false;
            }

            isOpen = toggle;
        }

        private void BodyEntered(Node body)
        {
            if (isOpen && body is Player p && !p.IsDead)
            {
                p.Position = teleportLocation;
                p.ClearVelocity();
                p.StartTimedInputLock(2f);

                var tween = p.World.Tween;

                tween.InterpolateProperty(p.World.GlobalLighting, "color", Colors.Black, p.World.DefaultLighting, 3f, Tween.TransitionType.Back);

                tween.Start();

                p.World.ShowPopupText($"Travelling to {teleportName}...");

                QuestObject.ForceEmitInteracted();

                var camera = p.GetNode<Camera2D>("Camera2D");

                camera.LimitLeft = cameraLimitLeft;
                camera.LimitTop = cameraLimitTop;
                camera.LimitRight = cameraLimitRight;
                camera.LimitBottom = cameraLimitBottom;

                TempDisableTP();
            }
        }

        public void TempDisableTP()
        {
            canTp = false;

            GetTree().CreateTimer(1f).Connect("timeout", this, nameof(ResetCanTp));
        }

        private void ResetCanTp()
        {
            canTp = true;
        }
    }
}