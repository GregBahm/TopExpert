namespace Encounter.Model
{
    public class HyperfocusEffector : PersistantEffector
    {
        protected override EncounterState ModifyState(EncounterState state)
        {
            return state with { Insights = state.Insights + 1 };
        }
    }
}