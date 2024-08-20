namespace Encounter.Model
{
    public class GetAfterIt : StandardPlayerCard
    {
        public override int ActionCost => 0;
        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Actions = state.Actions + 1 };
        }
    }
}