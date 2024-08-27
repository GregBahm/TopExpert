namespace Investigation.Model
{
    public record SpringTheTrapCard(ElementIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 1;
        public override bool DissolvesOnPlay => true;
        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Advantage = state.Advantage + 10 };
        }
    }
}