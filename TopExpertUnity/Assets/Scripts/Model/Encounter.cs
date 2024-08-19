using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public class Encounter
    {
        private static readonly ActivateDangerPhase dangerPhaseActivator = new ActivateDangerPhase();

        private readonly List<EncounterStep> encounterProgression;
        public EncounterState CurrentState { get { return encounterProgression.Last().State; } }

        public Encounter(EncounterState initialState)
        {
            EncounterStep firstStep = new EncounterStep(null, initialState);
            encounterProgression = new List<EncounterStep>() { firstStep };
        }

        public void PlayCard(PlayerCard card)
        {
            EncounterState nextState = card.Play(CurrentState);
            EncounterStep step = new EncounterStep(card, nextState);
            encounterProgression.Add(step);
        }

        public void EndRound()
        {
            EncounterState state = CurrentState;
            if (state.UnappliedEffectors.Any())
            {
                PersistantEffector effector = state.UnappliedEffectors.Last();
                state = effector.GetModifiedState(state);
                EncounterStep step = new EncounterStep(effector, state);
                encounterProgression.Add(step);
                EndRound();
            }
        }

        public bool CanActiateDangerPhase()
        {
            EncounterState state = CurrentState;
            return state.Insights > state.DangerPhaseInsightsCost;
        }

        public void ActivateDangerPhase()
        {
            EncounterState state = CurrentState;
            EncounterState modifiedState = dangerPhaseActivator.GetModifiedState(state);
            EncounterStep step = new EncounterStep(dangerPhaseActivator, modifiedState);
            encounterProgression.Add(step);
        }

        public void DraftCard(DraftOption option)
        {
            EncounterState nextState = option.DraftCard(CurrentState);
            EncounterStep step = new EncounterStep(option, nextState);
            encounterProgression.Add(step);
        }
    }

    public class EncounterStep
    {
        public IStateModifier Modifier { get; }
        public EncounterState State { get; }
        public EncounterStep(IStateModifier modifier, EncounterState state)
        {
            Modifier = modifier;
            State = state;
        }
    }

}