namespace Encounter.Model
{
    public record PunchItCard : StandardPlayerCard
    {
        public override int ActionCost => 0;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Advantage = state.Advantage + 1 };
        }
    }
}