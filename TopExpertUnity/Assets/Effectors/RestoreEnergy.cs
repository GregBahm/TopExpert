namespace Encounter.Model
{
    public class RestoreEnergy : PersistantEffector
    {
        protected override EncounterState ModifyState(EncounterState state)
        {
            return state with { Actions = state.ActionsPerTurn };
        }
    }
}