using Godot;

namespace BeetleGame
{
    public class Settings : VBoxContainer
    {
        public static bool IsCursorCustom { get; set; } = true;

        [Export]
        private NodePath lastFocusPath;

        private HSlider masterSlider;
        private HSlider musicSlider;
        private HSlider sfxSlider;
        private CheckButton fullscreenToggle;
        public CheckButton CursorToggle { get; set; }

        private int masterBusId;
        private int musicBusId;
        private int sfxBusId;

        private Texture customCursor;

        public override void _Ready()
        {
            base._Ready();

            masterSlider = GetNode<HSlider>("MasterSlider");
            musicSlider = GetNode<HSlider>("MusicSlider");
            sfxSlider = GetNode<HSlider>("SFXSlider");
            fullscreenToggle = GetNode<CheckButton>("FullscreenToggle");
            CursorToggle = GetNode<CheckButton>("CursorToggle");

            masterBusId = AudioServer.GetBusIndex("Master");
            musicBusId = AudioServer.GetBusIndex("Music");
            sfxBusId = AudioServer.GetBusIndex("SFX");

            customCursor = GD.Load<Texture>("res://textures/customcursor.png");

            masterSlider.Value = GetBusVol(masterBusId);
            musicSlider.Value = GetBusVol(musicBusId);
            sfxSlider.Value = GetBusVol(sfxBusId);
            fullscreenToggle.Pressed = OS.WindowFullscreen;
            CursorToggle.Pressed = IsCursorCustom;

            masterSlider.Connect("value_changed", this, nameof(MasterChanged));
            musicSlider.Connect("value_changed", this, nameof(MusicChanged));
            sfxSlider.Connect("value_changed", this, nameof(SFXChanged));
            fullscreenToggle.Connect("toggled", this, nameof(FullscreenToggled));
            CursorToggle.Connect("toggled", this, nameof(CursorToggled));

            if (lastFocusPath != null && !lastFocusPath.IsEmpty())
            {
                CursorToggle.FocusNeighbourBottom = lastFocusPath;
                CursorToggle.FocusNext = lastFocusPath;
            }
        }

        private float GetBusVol(int busId)
        {
            return GD.Db2Linear(AudioServer.GetBusVolumeDb(busId));
        }

        private void SetBusVol(int busId, float vol)
        {
            AudioServer.SetBusVolumeDb(busId, GD.Linear2Db(vol));
        }

        private void MasterChanged(float vol)
        {
            SetBusVol(masterBusId, vol);
        }

        private void MusicChanged(float vol)
        {
            SetBusVol(musicBusId, vol);
        }

        private void SFXChanged(float vol)
        {
            SetBusVol(sfxBusId, vol);
        }

        private void FullscreenToggled(bool toggle)
        {
            OS.WindowFullscreen = toggle;
        }

        private void CursorToggled(bool toggle)
        {
            if (toggle)
            {
                Input.SetCustomMouseCursor(customCursor, hotspot: new Vector2(7, 7));
            }
            else
            {
                Input.SetCustomMouseCursor(null);
            }

            IsCursorCustom = toggle;
        }
    }
}