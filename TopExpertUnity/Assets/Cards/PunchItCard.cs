namespace Investigation.Model
{
    public record PunchItCard(ElementIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 0;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Advantage = state.Advantage + 1 };
        }
    }
}