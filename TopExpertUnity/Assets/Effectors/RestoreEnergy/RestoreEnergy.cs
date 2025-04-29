namespace Investigation.Model
{
    public record RestoreEnergy() 
        : PersistantEffector()
    {
        protected override EncounterState GetEffectedState(EncounterState state)
        {
            return state with { Actions = state.ActionsPerTurn };
        }
    }
}