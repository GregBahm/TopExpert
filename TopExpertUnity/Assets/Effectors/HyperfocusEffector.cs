namespace Investigation.Model
{
    public record HyperfocusEffector(ElementIdentifier Identifier) 
        : PersistantEffector(Identifier)
    {
        protected override EncounterState GetEffectedState(EncounterState state)
        {
            return state with { Insights = state.Insights + 1 };
        }
    }
}