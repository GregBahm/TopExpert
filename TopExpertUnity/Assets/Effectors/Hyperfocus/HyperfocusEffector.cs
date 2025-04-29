namespace Investigation.Model
{
    public record HyperfocusEffector() 
        : PersistantEffector()
    {
        protected override EncounterState GetEffectedState(EncounterState state)
        {
            return state with { Insights = state.Insights + 1 };
        }
    }
}