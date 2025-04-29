namespace Investigation.Model
{
    public record InvestigateCard() 
        : StandardPlayerCard()
    {
        public override int ActionCost => 1;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            return state with { Insights = state.Insights + 1 };
        }
    }
}