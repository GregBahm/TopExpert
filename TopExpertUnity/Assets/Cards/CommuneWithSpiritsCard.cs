namespace Investigation.Model
{
    public record CommuneWithSpiritsCard(CardIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 1;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { SpiritsPower = state.SpiritsPower + 1 };
        }
    }
}