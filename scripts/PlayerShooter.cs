using Godot;

namespace BeetleGame
{
    public class PlayerShooter : Node2D
    {
        public Vector2 ShootDir { get; set; } = Vector2.Zero;

        private Player player;
        private float range = 80f;
        private float charge = 0f;
        private float chargeSpeed = .75f;
        private Color startChargeColour = new Color("4b1d52");
        private Color endChargeColour = new Color("e64539");
        private Color aimColour = Colors.Black;
        private PackedScene projScene;
        private AudioStreamPlayer shootPlayer;
        private AudioStreamPlayer chargePlayer;
        private bool canShoot = true;

        private float count = 0;

        public override void _Ready()
        {
            player = GetParent().GetParent<Player>();
            projScene = GD.Load<PackedScene>("res://scenes/Projectile.tscn");
            shootPlayer = GetNode<AudioStreamPlayer>("ShootPlayer");
            chargePlayer = GetNode<AudioStreamPlayer>("ChargePlayer");
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            count += delta;
            if (count > 1f)
                count--;

            ShootDir = GetLocalMousePosition().Normalized();

            if (Input.IsActionPressed("g_shoot") && !player.IsInputLocked)
            {
                charge += chargeSpeed * delta;

                charge = Mathf.Clamp(charge, 0f, 1f);

                aimColour = startChargeColour.LinearInterpolate(endChargeColour, charge);
            }

            if (Input.IsActionJustPressed("g_shoot") && !player.IsInputLocked)
            {
                chargePlayer.Play();
            }

            if (Input.IsActionJustReleased("g_shoot") && !player.IsInputLocked && canShoot)
            {
                player.ApplyExternalImpulse(ShootDir * -200f * charge);

                var projDir = ShootDir * 200f;

                if (charge < 1f)
                {
                    float angle = (Mathf.Pi / 4f) * (1f - charge);
                    projDir = projDir.Rotated(GlobalNodes.RNG.RandfRange(-angle, angle));
                }

                var proj = projScene.Instance<Projectile>();
                player.World.AddChild(proj);
                proj.Source = player;
                proj.GlobalPosition = GlobalPosition + (ShootDir * 2f);
                proj.SetVelocity(projDir);

                charge = 0;

                chargePlayer.Stop();
                shootPlayer.Play();

                canShoot = false;
                GetTree().CreateTimer(.5f).Connect("timeout", this, nameof(ResetCanShoot));
            }

            Update();
        }

        public override void _Draw()
        {
            base._Draw();

            if (Input.IsActionPressed("g_shoot") && !player.IsInputLocked)
            {
                if (charge < 1f)
                {
                    float angle = (Mathf.Pi / 4f) * (1f - charge);

                    DrawLine(Vector2.Zero, (ShootDir.Rotated(angle)) * range, aimColour, 1f);
                    DrawLine(Vector2.Zero, (ShootDir.Rotated(-angle)) * range, aimColour, 1f);
                }
                else
                {
                    // perfect line
                    DrawLine(Vector2.Zero, ShootDir * range * .75f, aimColour, 2f);
                    DrawLine(ShootDir * range * .75f, ShootDir * range, aimColour, 1f);
                }
            }
        }

        private void ResetCanShoot()
        {
            canShoot = true;
        }
    }
}