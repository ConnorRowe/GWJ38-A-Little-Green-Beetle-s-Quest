using Godot;
using System.Collections.Generic;

namespace BeetleGame
{
    public class SpeechBox : VBoxContainer
    {
        public QuestObject @QuestObject { get; set; }

        private Label speakerName;
        private RichTextLabel speechRichText;
        private Tween tween;
        private AudioStreamPlayer blipPlayer;
        private World world;

        private float maxCharacters = 0;
        private float currentCharacters = 0;
        private List<(string speaker, string speech)> queuedSpeech = new List<(string speaker, string speech)>();

        public override void _Ready()
        {
            speakerName = GetNode<Label>("SpeakerPanel/MarginContainer/MarginContainer/SpeakerName");
            speechRichText = GetNode<RichTextLabel>("TextPanel/MarginContainer/SpeechText");
            tween = GetNode<Tween>("Tween");
            blipPlayer = GetNode<AudioStreamPlayer>("BlipPlayer");
            QuestObject = GetNode<QuestObject>("QuestObject");
            world = (World)GetTree().CurrentScene;

            Visible = false;
            RectScale = new Vector2(0, 1);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            int startChars = speechRichText.VisibleCharacters;

            if (currentCharacters < maxCharacters)
            {
                currentCharacters += delta * 20.0f;
            }

            if (currentCharacters > maxCharacters)
            {
                currentCharacters = maxCharacters;
            }

            speechRichText.VisibleCharacters = Mathf.RoundToInt(currentCharacters);

            if (speechRichText.VisibleCharacters > startChars)
            {
                blipPlayer.Play();
            }
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            base._UnhandledInput(evt);

            if (evt.IsActionReleased("g_next_speech"))
            {
                if (currentCharacters < maxCharacters)
                {
                    currentCharacters = maxCharacters;
                    speechRichText.VisibleCharacters = Mathf.RoundToInt(currentCharacters);
                }
                else
                {
                    if (queuedSpeech.Count > 0)
                    {
                        NextText();
                    }
                    else if (Visible == true)
                    {
                        // Finished conversation

                        ToggleDisplay(true);

                        QuestObject.ForceEmitInteracted();
                    }

                    GetTree().SetInputAsHandled();
                }
            }
        }

        public void QueueSpeech(List<(string speaker, string speech)> conversation)
        {
            foreach (var s in conversation)
            {
                queuedSpeech.Add(s);
            }

            ToggleDisplay(false);

            NextText();
        }

        public void ToggleDisplay(bool hide)
        {
            tween.InterpolateProperty(this, "rect_scale", hide ? new Vector2(1, 1) : new Vector2(0, 1), hide ? new Vector2(0, 1) : new Vector2(1, 1), 0.8f, Tween.TransitionType.Circ, hide ? Tween.EaseType.Out : Tween.EaseType.In);

            if (hide)
                tween.InterpolateProperty(this, "visible", true, false, 0.01f, delay: 0.8f);
            else
                Visible = true;

            tween.Start();

            world.Player.IsInputLocked = !hide;
        }

        public void NextText()
        {
            var nextText = queuedSpeech[0];

            speakerName.Text = nextText.speaker;
            speechRichText.BbcodeText = nextText.speech;

            CallDeferred(nameof(UpdateMaxCharacters));
            speechRichText.VisibleCharacters = 0;
            currentCharacters = 0;

            queuedSpeech.RemoveAt(0);
        }

        private void UpdateMaxCharacters()
        {
            maxCharacters = speechRichText.GetTotalCharacterCount();
        }

    }
}