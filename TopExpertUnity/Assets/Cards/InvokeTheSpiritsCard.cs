namespace Investigation.Model
{
    public record InvokeTheSpiritsCard(CardIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 1;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Advantage = state.SpiritsPower };
        }
    }
}