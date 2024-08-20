namespace Encounter.Model
{
    public class ActivateDangerPhase : IStateModifier
    {
        public EncounterState GetModifiedState(EncounterState state)
        {
            int newInsights = state.Insights - state.DangerPhaseInsightsCost;
            state = state with { Insights = newInsights, Phase = EncounterPhase.Danger };
            return state;
        }
    }
}