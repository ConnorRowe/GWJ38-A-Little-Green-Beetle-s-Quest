using Godot;
using System.Collections.Generic;

namespace BeetleGame
{
    public class QuestHandler : Node
    {
        [Signal]
        public delegate void NewPhaseStarted(QuestPhase questPhase);

        public QuestPhase CurrentPhase
        {
            get
            {
                if (questPhases.Count > 0)
                    return questPhases[0];
                else
                    return null;
            }
        }

        private List<QuestPhase> questPhases = new List<QuestPhase>();
        private World world;

        public override void _Ready()
        {
            world = (World)GetTree().CurrentScene;
        }

        public void AddQuestPhase(QuestPhase questPhase)
        {
            if (questPhases.Count == 0)
            {
                StartPhase(questPhase);
            }

            questPhases.Add(questPhase);

            AddChild(questPhase);
        }

        private void StartPhase(QuestPhase questPhase)
        {
            questPhase.Connect(nameof(QuestPhase.PhaseComplete), this, nameof(PhaseCompleted));
            world.SetQuestPoint(questPhase.PhaseLocation);

            EmitSignal(nameof(NewPhaseStarted), questPhase);
        }

        private void PhaseCompleted()
        {
            questPhases[0].Disconnect(nameof(QuestPhase.PhaseComplete), this, nameof(PhaseCompleted));
            RemoveChild(questPhases[0]);
            questPhases[0].QueueFree();
            questPhases.RemoveAt(0);

            if (questPhases.Count > 0)
                StartPhase(questPhases[0]);
        }
    }
}