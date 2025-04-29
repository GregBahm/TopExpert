namespace Investigation.Model
{
    public record InvokeTheSpiritsCard() 
        : StandardPlayerCard()
    {
        public override int ActionCost => 1;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Advantage = state.SpiritsPower };
        }
    }
}