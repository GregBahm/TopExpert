namespace Investigation.Model
{
    public record RestoreEnergy(EffectorIdentifier Identifier) 
        : PersistantEffector(Identifier)
    {
        protected override EncounterState GetEffectedState(EncounterState state)
        {
            return state with { Actions = state.ActionsPerTurn };
        }
    }
}