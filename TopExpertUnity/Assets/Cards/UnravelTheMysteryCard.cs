namespace Investigation.Model
{
    public record UnravelTheMysteryCard(ElementIdentifier Identifier)
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 1;
        public override int InsightCost => 2;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Advantage = state.Advantage + 3 };
        }
    }
}