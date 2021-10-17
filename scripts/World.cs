using Godot;
using System.Collections.Generic;
using System;

namespace BeetleGame
{
    public class World : Node2D
    {
        public Label DebugLabel { get; set; }
        public Node2D QuestPointer { get; set; }
        public Player @Player { get; set; }
        public SpeechBox @SpeechBox { get; set; }
        public YSort @YSort { get; set; }
        public Tween Tween { get; set; }
        public CanvasModulate GlobalLighting { get; set; }
        public Color DefaultLighting { get; set; }

        private QuestHandler questHandler;
        private PackedScene questPhaseScene;
        private PackedScene smokePuffScene;

        private AnimatedSprite heart1;
        private AnimatedSprite heart2;
        private Checkpoint lastCheckpoint;
        private Label popupLabel;
        private Label questLabel;

        public override void _Ready()
        {
            DebugLabel = GetNode<Label>("UI/DebugLabel");
            QuestPointer = GetNode<Node2D>("YSort/Pointer");
            Player = GetNode<Player>("YSort/Player");
            SpeechBox = GetNode<SpeechBox>("UI/SpeechBox");
            questHandler = GetNode<QuestHandler>("QuestHandler");
            YSort = GetNode<YSort>("YSort");
            heart1 = GetNode<AnimatedSprite>("UI/Heart_1");
            heart2 = GetNode<AnimatedSprite>("UI/Heart_2");
            Tween = GetNode<Tween>("UI/Tween");
            popupLabel = GetNode<Label>("UI/PopupBob/PopupLabel");
            questLabel = GetNode<Label>("UI/QuestLabel");
            GlobalLighting = GetNode<CanvasModulate>("CanvasModulate");
            DefaultLighting = GlobalLighting.Color;

            questPhaseScene = GD.Load<PackedScene>("res://scenes/QuestPhase.tscn");
            smokePuffScene = GD.Load<PackedScene>("res://scenes/SmokePuff.tscn");

            var elemFruit = GetNode<ElementalFruit>("YSort/ElementalFruit");
            elemFruit.Visible = false;
            elemFruit.SetActive(false);

            Player.Connect(nameof(Player.TookDamage), this, nameof(PlayerTookDamage));
            Player.Connect(nameof(Player.PlayerInteracted), this, nameof(PlayerInteracted));
            Player.Connect(nameof(Player.Died), this, nameof(PlayerDied));
            questHandler.Connect(nameof(QuestHandler.NewPhaseStarted), this, nameof(NewPhaseStarted));

            var settingsButton = GetNode<Control>("UI/SettingsButton");
            var closeSettingsButton = GetNode<Control>("UI/SettingsPopup/MarginContainer/VBoxContainer/CloseSettingsButton");
            var exitButton = GetNode<Control>("UI/SettingsPopup/MarginContainer/VBoxContainer/ExitButton");
            var exitConfirm = GetNode<Control>("UI/ExitConfirm/MarginContainer/VBoxContainer/Yes");
            var exitCancel = GetNode<Control>("UI/ExitConfirm/MarginContainer/VBoxContainer/No");

            settingsButton.Connect("pressed", this, nameof(OpenSettingsPopup));
            closeSettingsButton.Connect("pressed", this, nameof(CloseSettingsPopup));
            exitButton.Connect("pressed", this, nameof(OpenExitConfirm));
            exitConfirm.Connect("pressed", this, nameof(ExitToMainMenu));
            exitCancel.Connect("pressed", this, nameof(CloseExitConfirm));

            closeSettingsButton.FocusNeighbourTop = GetPathTo(GetNode<Settings>("UI/SettingsPopup/MarginContainer/VBoxContainer/Settings").CursorToggle);
            closeSettingsButton.FocusPrevious = closeSettingsButton.FocusNeighbourTop;

            Player.StartTimedInputLock(.5f);
            var circleThing = GetNode<Control>("CircleThingLayer/CircleThing");
            circleThing.Visible = true;
            Tween.InterpolateProperty(circleThing, "rect_scale", new Vector2(.05f, .05f), new Vector2(2.5f, 2.5f), 4f, Tween.TransitionType.Cubic);
            GetTree().CreateTimer(4f).Connect("timeout", this, nameof(FreeCircleThing));
            Tween.Start();
            Player.GetNode<AudioStreamPlayer>("Body/PlayerShooter/ChargePlayer").Play();

            //Set-up quests

            // Speak to Elder Makros
            NPC elderMakros = GetNode<NPC>("YSort/NPC_1");
            questHandler.AddQuestPhase(MakeQuestPhase(elderMakros.Position + new Vector2(0, -39), elderMakros, "Speak to Elder Makros.", (q) =>
            {
                List<(string speaker, string speech)> testConvo = new List<(string speaker, string speech)> {
                    ("Elder Makros", "[color=#a4dddb]Young one. I am glad you are here, the burrow needs your aid."),
                    ("You", "[color=#d0da91]Uhh, is everything okay?"),
                    ("Elder Makros", "[color=#a4dddb]You know full well we are in great danger, pupa."),
                    ("You", "[color=#d0da91][shake rate=10 level=6]I'm not a pupa anymore![/shake]"),
                    ("Elder Makros", "[color=#a4dddb]I know you have noticed our supplies have been running low, it has affected us all."),
                    ("Elder Makros", "[color=#a4dddb]It may look quite grassy here but there isn't enough nutrient variety. Our young are hatching with nutrient deficiencies more and more often."),
                    ("Elder Makros", "[color=#a4dddb]*The elder sighs.*"),
                    ("Elder Makros", "[color=#a4dddb]The humans may have experienced socio-economic enlightenment, but what good has that been for us? They will not cease their expansion."),
                    ("You", "[color=#d0da91]Yeah it's so dusty compared to when I was a larva. I even remember when we had different types of mushrooms!"),
                    ("Elder Makros", "[color=#a4dddb]That's their [shake rate=5 level=6]\"concrete\"[/shake], child."),
                    ("Elder Makros", "[color=#a4dddb]We need you to scout a better location for our new burrow."),
                    ("You", "[color=#d0da91]But we've been here for ten-thousand generations!"),
                    ("Elder Makros", "[color=#a4dddb]It matters not. Look around, there's nothing left for us here. Find us somewhere... lush, verdant, biodiverse. We owe it to our descendants."),
                    ("You", "[color=#d0da91]Okay! I'll find somewhere nice, I promise."),
                    ("Elder Makros", "[color=#a4dddb]Outstanding. Now, before you leave. Take this, it will allow you to absorb a few hits before perishing. You'll need it.")};

                SpeechBox.QueueSpeech(testConvo);

                elderMakros.SetActive(false);
            }
            ));

            // Finish speech with the Elder

            questHandler.AddQuestPhase(MakeQuestPhase(elderMakros.Position + new Vector2(0, -39), SpeechBox.QuestObject, "Speak to Elder Makros.", (q) =>
            {
                elemFruit.Visible = true;
                elemFruit.SetActive(true);

                CreateSmokePuff(elemFruit.GlobalPosition);
            }));

            // Interact with fruit
            questHandler.AddQuestPhase(MakeQuestPhase(elemFruit.GlobalPosition + new Vector2(0, -16), elemFruit, "Interact with the Elemental Fruit.", (q) =>
            {
                elemFruit.QueueFree();

                FreeChildrenWithSmoke(YSort.GetNode<Node2D>("StartingWalls"));

                // Reveal health

                heart1.Visible = true;
                heart2.Visible = true;
                GetNode<SmokePuff>("UI/HeartSmokePuff1").Start();
                GetNode<SmokePuff>("UI/HeartSmokePuff2").Start();

                Vector2 heart1EndPos = heart1.Position;
                Vector2 heart2EndPos = heart2.Position;

                Tween.InterpolateProperty(heart1, "position", new Vector2(480f / 2f, 270f / 2f), heart1EndPos, 1f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                Tween.InterpolateProperty(heart2, "position", new Vector2(480f / 2f, 270f / 2f), heart2EndPos, 1f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                Tween.InterpolateProperty(heart1, "scale", Vector2.Zero, Vector2.One, .75f, Tween.TransitionType.Back, Tween.EaseType.InOut);
                Tween.InterpolateProperty(heart2, "scale", Vector2.Zero, Vector2.One, .75f, Tween.TransitionType.Back, Tween.EaseType.InOut);
                Tween.Start();
            }));

            // Speak to Elder Poa

            NPC elderPoa = GetNode<NPC>("YSort/NPC_2");
            questHandler.AddQuestPhase(MakeQuestPhase(elderPoa.Position + new Vector2(0, -39), elderPoa, "Speak to Elder Poa.", (q) =>
            {
                List<(string speaker, string speech)> testConvo = new List<(string speaker, string speech)> {
                    ("Elder Poa", "[color=#df84a5]Ahh, greetings. I suppose you're here to learn about the Adephagans' Hellebore?"),
                    ("You", "[color=#d0da91]Uhh, what?"),
                    ("Elder Poa", "[color=#df84a5]The Adephagans' Hellebore can be found as a small, enclosed flower bud."),
                    ("Elder Poa", "[color=#df84a5]If you shoot one with your pheromones it will activate, revealing its bright pink petals."),
                    ("Elder Poa", "[color=#df84a5]The ancient magic contained within these plants will allow you to return to the last one you activated if you succumb to your wounds."),
                    ("Elder Poa", "[color=#df84a5]Ensure you activate every one of these 'checkpoint' flowers you find!"),
                    ("You", "[color=#d0da91]Thanks, that'll be useful on my quest!"),
                    ("Elder Poa", "[color=#df84a5]You're welcome child. Now, try activating the Adephagans' Hellebore nearby. Then speak to me again.")};

                SpeechBox.QueueSpeech(testConvo);

                elderMakros.SetActive(false);
            }
            ));

            // Finish speech with the Elder
            Checkpoint checkpoint = GetNode<Checkpoint>("YSort/Checkpoint");
            questHandler.AddQuestPhase(MakeQuestPhase(checkpoint.Position + new Vector2(0, -32), SpeechBox.QuestObject, "Speak to Elder Poa.", (q) =>
            {
                CreateSmokePuff(checkpoint.GlobalPosition);
                checkpoint.SetIsActive(false);
            }));

            // Activate checkpoint
            questHandler.AddQuestPhase(MakeQuestPhase(checkpoint.Position + new Vector2(0, -32), checkpoint.QuestObject, "Activate the checkpoint.", (q) => { }));

            questHandler.AddQuestPhase(MakeQuestPhase(elderPoa.Position + new Vector2(0, -39), elderPoa, "Speak to Elder Poa.", (q) =>
            {
                List<(string speaker, string speech)> testConvo = new List<(string speaker, string speech)> {
                    ("Elder Poa", "[color=#df84a5]Fantastic! Now, kill that demon beetle to the east!"),
                    ("You", "[color=#d0da91][shake rate=5 level=6]What?! How?[/shake]"),
                    ("Elder Poa", "[color=#df84a5]Shoot it, obviously, like every game.")};

                SpeechBox.QueueSpeech(testConvo);

                elderMakros.SetActive(false);

                FreeChildrenWithSmoke(YSort.GetNode<Node2D>("CheckpointWalls"));
            }
            ));

            // Kill demon beetle
            Enemy enemy = GetNode<Enemy>("YSort/Enemy");
            questHandler.AddQuestPhase(MakeQuestPhase(enemy.Position + new Vector2(0, -24), enemy.GetNode<QuestObject>("QuestObject"), "Kill the demon beetle.", (q) => { }));

            NPC mysteryBeetle = GetNode<NPC>("YSort/NPC_3");

            // "Explore" the rest of the Parched Pasture ~ speak to mystery beetle
            // Speak to a beetle who points you to a hidden cave entrance

            questHandler.AddQuestPhase(MakeQuestPhase(mysteryBeetle.Position + new Vector2(0, -39), mysteryBeetle, "Explore the rest of the Parched Pasture.", (q) =>
            {
                List<(string speaker, string speech)> testConvo = new List<(string speaker, string speech)> {
                    ("Mysterious Beetle", "[color=#752438]Greetings young one."),
                    ("You", "[color=#d0da91]Uhh hi... What are you doing all the way out here? There're a lot of those demon beetles out here."),
                    ("Mysterious Beetle", "[color=#752438]Yes, I'm aware. I'm guarding this ancient passage you see. My family have kept watch over it for 8 generations."),
                    ("Mysterious Beetle", "[color=#752438]Course, it's only me left now."),
                    ("You", "[color=#d0da91]I'm sorry."),
                    ("Mysterious Beetle", "[color=#752438]Theres no sense dwelling on it. Now, can I help you?"),
                    ("You", "[color=#d0da91]Well, I'm on a quest to find a place where we can make a new, better burrow. Somewhere teeming with life."),
                    ("Mysterious Beetle", "[color=#752438]Ah, I think I can help you with that. I shall unseal the passage and allow you through."),
                    ("Mysterious Beetle", "[color=#752438]I hope you can find what you're looking for on the other side. Farewell."),
                    ("You", "[color=#d0da91]Wow, thank you! Goodbye!")};

                SpeechBox.QueueSpeech(testConvo);

                mysteryBeetle.SetActive(false);
            }));

            CaveEntrance caveEntrance = GetNode<CaveEntrance>("YSort/CaveEntrance");
            // after speaking to them they disappear in a puff of smoke

            // Finish speaking to them
            questHandler.AddQuestPhase(MakeQuestPhase(mysteryBeetle.Position + new Vector2(0, -39), SpeechBox.QuestObject, "Speak to the mysterious beetle.", (q) =>
            {
                CreateSmokePuff(mysteryBeetle.GlobalPosition);
                mysteryBeetle.Visible = false;

                for (int i = 0; i < 6; i++)
                {
                    CreateSmokePuff(caveEntrance.GlobalPosition + new Vector2(GlobalNodes.RNG.RandfRange(-8f, 8f), GlobalNodes.RNG.RandfRange(-8f, 8f)));
                }

                caveEntrance.ToggleOpen(true);
            }));

            var otherCave = GetNode<CaveEntrance>("YSort/CaveEntrance2");

            // Enter the cave
            questHandler.AddQuestPhase(MakeQuestPhase(caveEntrance.Position + new Vector2(0, -30), caveEntrance.QuestObject, "Enter the mysterious cave.", (q) =>
            {
                otherCave.TempDisableTP();
            }));

            // In new zone, there are a few demon beetles, and a new npc who has lost their grandma's jewellery.
            // if you find it for them they will open the way to the clearing

            var sadBeetle = GetNode<NPC>("YSort/NPC_4");
            questHandler.AddQuestPhase(MakeQuestPhase(sadBeetle.Position + new Vector2(0, -24), sadBeetle, "Explore the Thriving Woodland.", (q) =>
            {
                List<(string speaker, string speech)> testConvo = new List<(string speaker, string speech)> {
                    ("Sad Looking Beetle", "[color=#3c5e8b]*sniffles*"),
                    ("You", "[color=#d0da91]Are you okay?"),
                    ("Sad Looking Beetle", "[color=#3c5e8b]I was attacked by a horde of demon beetles, I barely survived!"),
                    ("You", "[color=#d0da91]Oh no! Should I get help?"),
                    ("Sad Looking Beetle", "[color=#3c5e8b]No it's fine, just a few scratches. The real problem is that I lost my necklace in the scuffle."),
                    ("Sad Looking Beetle", "[color=#3c5e8b]It was given to me by my great grandbeetle. They said it was given to them by a lost love, and that it was crafted from broken bits of the humans' \"solar panels\"."),
                    ("You", "[color=#d0da91]Wow, could it be nearby?"),
                    ("Sad Looking Beetle", "[color=#3c5e8b]Maybe. If you can retrieve it, I'll open up the way to the secluded grove, it's part of an ancient woodland, far away from any humans."),
                    ("You", "[color=#d0da91]I'll be back soon with your necklace!")};

                SpeechBox.QueueSpeech(testConvo);

                mysteryBeetle.SetActive(false);
            }));

            // Finish speaking to them
            questHandler.AddQuestPhase(MakeQuestPhase(sadBeetle.Position + new Vector2(0, -24), SpeechBox.QuestObject, "Speak to the sad looking beetle.", (q) => { }));

            var jewellery = GetNode<Jewellery>("YSort/Jewellery");

            // Find jewellery
            questHandler.AddQuestPhase(MakeQuestPhase(jewellery.Position + new Vector2(0, -8), jewellery.QuestObject, "Find the lost necklace.", (q) =>
            {
                ShowPopupText("You found the lost necklace!");

                mysteryBeetle.SetActive(true);
            }));


            // return the jewellery and get to the clearing
            questHandler.AddQuestPhase(MakeQuestPhase(sadBeetle.Position + new Vector2(0, -24), sadBeetle, "Return the necklace.", (q) =>
            {
                List<(string speaker, string speech)> testConvo = new List<(string speaker, string speech)> {
                    ("Sad Looking Beetle", "[color=#3c5e8b]*sniffles*"),
                    ("Sad Looking Beetle", "[color=#3c5e8b]You've found it?!"),
                    ("You", "[color=#d0da91]*You hand over the necklace*"),
                    ("Happy Looking Beetle", "[color=#3c5e8b]Wow! Thank you! I'll open up the way for you now."),
                    ("You", "[color=#d0da91]I'm always happy to help someone in need! Bye!")};

                SpeechBox.QueueSpeech(testConvo);

                mysteryBeetle.SetActive(false);
            }));

            // Finish speaking to them
            questHandler.AddQuestPhase(MakeQuestPhase(sadBeetle.Position + new Vector2(0, -24), SpeechBox.QuestObject, "Speak to the sad looking beetle.", (q) =>
            {
                FreeChildrenWithSmoke(GetNode<Node2D>("YSort/JewelleryWalls"));
            }));

            // Find the new burrow
            var finalCave = GetNode<CaveEntrance>("YSort/CaveEntrance3");

            questHandler.AddQuestPhase(MakeQuestPhase(finalCave.Position + new Vector2(0, -30), finalCave.QuestObject, "Find the new burrow.", (q) =>
            {
                // open end screen
                GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://scenes/EndScreen.tscn"));
                QueueFree();
            }));
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt.IsActionReleased("g_respawn") && Player.IsDead)
            {
                Respawn();
            }
        }

        public void SetQuestPoint(Vector2 globalPosition)
        {
            QuestPointer.GlobalPosition = globalPosition;
            Player.QuestTargetPos = globalPosition;
        }

        public QuestPhase MakeQuestPhase(Vector2 phaseLocation, Node nodeToInteract, string phaseDesc, Action<QuestObject> phaseCompleteAction)
        {
            QuestPhase phase = questPhaseScene.Instance<QuestPhase>();
            phase.Init(phaseLocation, nodeToInteract, phaseCompleteAction, questHandler, phaseDesc);

            return phase;
        }

        public void CreateSmokePuff(Vector2 globalPos)
        {
            SmokePuff smokePuff = smokePuffScene.Instance<SmokePuff>();
            YSort.AddChild(smokePuff);
            smokePuff.GlobalPosition = globalPos;
            smokePuff.Start();
        }

        public void FreeNodeWithSmoke(Node2D node)
        {
            CreateSmokePuff(node.GlobalPosition);

            node.QueueFree();
        }

        private void PlayerTookDamage()
        {
            UpdateHealthSprites();
        }

        private void UpdateHealthSprites()
        {
            switch (Player.Health)
            {
                case 3:
                    heart2.Frame = 1;
                    break;
                case 2:
                    heart2.Visible = false;
                    break;
                case 1:
                    heart1.Frame = 1;
                    break;
                case 0:
                    heart1.Visible = false;
                    break;
                default:
                    heart1.Visible = true;
                    heart2.Visible = true;
                    heart1.Frame = 0;
                    heart2.Frame = 0;
                    break;
            }
        }

        private void PlayerInteracted(QuestObject questObject)
        {
            if (questObject.Owner is Checkpoint checkpoint)
            {
                if (lastCheckpoint != null && lastCheckpoint != checkpoint)
                    lastCheckpoint.SetIsActive(false);

                lastCheckpoint = checkpoint;

                ShowPopupText("Checkpoint activated!");
            }
        }

        public void ShowPopupText(string text)
        {
            popupLabel.Visible = true;
            popupLabel.Text = text;

            Tween.InterpolateProperty(popupLabel, "rect_scale", Vector2.Zero, Vector2.One, .35f, Tween.TransitionType.Back);
            Tween.InterpolateProperty(popupLabel, "rect_scale", Vector2.One, Vector2.Zero, .25f, Tween.TransitionType.Back, delay: 4.35f);
            Tween.InterpolateProperty(popupLabel, "visible", true, false, .01f, Tween.TransitionType.Back, delay: 4.6f);

            Tween.Start();
        }

        private void FreeChildrenWithSmoke(Node2D parent)
        {
            // Remove walls
            var children = parent.GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                Node2D wall = (Node2D)children[i];

                GetTree().CreateTimer(.25f * i).Connect("timeout", this, nameof(FreeNodeWithSmoke), new Godot.Collections.Array(wall));
            }
        }

        private void NewPhaseStarted(QuestPhase questPhase)
        {
            questLabel.Text = $"~ {questPhase.PhaseDesc}";

            GD.Print($"New phase started: {questPhase.PhaseDesc}");
        }

        private void PlayerDied()
        {
            Tween.InterpolateProperty(GlobalLighting, "color", DefaultLighting, new Color(0.1372f, 0.1372f, 0.1372f), 1f, Tween.TransitionType.Back);
            Tween.InterpolateProperty(questLabel, "modulate", Colors.White, Colors.Transparent, .5f, Tween.TransitionType.Cubic);
            Tween.InterpolateProperty(GetNode<Node2D>("UI/DeathUI"), "scale", Vector2.Zero, Vector2.One, 1f, Tween.TransitionType.Back);

            Tween.Start();
        }

        private void Respawn()
        {
            Player.Respawn();
            Player.Position = lastCheckpoint.Position + (new Vector2(GlobalNodes.RNG.Randf(), GlobalNodes.RNG.Randf()) * GlobalNodes.RNG.RandfRange(4f, 12f));
            CreateSmokePuff(Player.Position);
            lastCheckpoint.EmitBurst();
            UpdateHealthSprites();

            Tween.InterpolateProperty(GlobalLighting, "color", GlobalLighting.Color, DefaultLighting, 1f, Tween.TransitionType.Back);
            Tween.InterpolateProperty(questLabel, "modulate", Colors.Transparent, Colors.White, .5f, Tween.TransitionType.Cubic);
            Tween.InterpolateProperty(GetNode<Node2D>("UI/DeathUI"), "scale", Vector2.One, Vector2.Zero, 1f, Tween.TransitionType.Back);

            Tween.Start();
        }

        private void OpenSettingsPopup()
        {
            GetNode<PopupPanel>("UI/SettingsPopup").PopupCentered();

            GetTree().Paused = true;
        }


        private void CloseSettingsPopup()
        {
            GetNode<PopupPanel>("UI/SettingsPopup").Hide();

            GetTree().Paused = false;
        }

        private void OpenExitConfirm()
        {
            CloseSettingsPopup();

            GetTree().Paused = true;
            GetNode<PopupPanel>("UI/ExitConfirm").PopupCentered();
        }

        private void CloseExitConfirm()
        {
            GetNode<PopupPanel>("UI/ExitConfirm").Hide();

            GetTree().Paused = false;
        }

        private void ExitToMainMenu()
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://scenes/MainMenu.tscn"));
            QueueFree();
        }

        private void FreeCircleThing()
        {
            GetNode("CircleThingLayer").QueueFree();
        }
    }
}