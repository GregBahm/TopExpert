namespace Investigation.Model
{
    public record CommuneWithSpiritsCard() : StandardPlayerCard()
    {
        public override int ActionCost => 1;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { SpiritsPower = state.SpiritsPower + 1 };
        }
    }
}