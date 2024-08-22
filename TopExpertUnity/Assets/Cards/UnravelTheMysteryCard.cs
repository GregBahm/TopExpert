namespace Encounter.Model
{
    public record UnravelTheMysteryCard : StandardPlayerCard
    {
        public override int ActionCost => 1;
        public override int InsightCost => 2;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Advantage = state.Advantage + 3 };
        }
    }
}