using Godot;
using System;

namespace BeetleGame
{
    public class QuestPhase : Node
    {
        [Signal]
        public delegate void PhaseComplete();

        public Vector2 PhaseLocation { get; set; }
        public QuestHandler @QuestHandler { get; set; }
        public string PhaseDesc { get; set; }
        private Action<QuestObject> phaseCompleteAction;

        public void Init(Vector2 phaseLocation, Node nodeToInteract, Action<QuestObject> phaseCompleteAction, QuestHandler questHandler, string phaseDesc)
        {
            PhaseLocation = phaseLocation;
            this.phaseCompleteAction = phaseCompleteAction;
            QuestHandler = questHandler;
            PhaseDesc = phaseDesc;

            nodeToInteract.Connect(nameof(QuestObject.Interacted), this, nameof(TargetInteracted));
        }

        private void TargetInteracted(QuestObject node)
        {
            if (QuestHandler.CurrentPhase == this)
            {
                EmitSignal(nameof(PhaseComplete));

                phaseCompleteAction(node);
            }
        }
    }
}