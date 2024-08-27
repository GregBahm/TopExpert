namespace Investigation.Model
{
    public abstract record StandardPlayerCard(ElementIdentifier Identifier) 
        : PlayerCard(Identifier)
    {
        public abstract int ActionCost { get; }
        public virtual int InsightCost => 0;

        public override bool CanPlay(EncounterState state)
        {
            return state.Actions >= ActionCost && state.Insights >= InsightCost;
        }

        public override EncounterState Play(EncounterState state)
        {
            state = GetWithCardMovedFromHand(state);
            state = state with { Actions = state.Actions - ActionCost , Insights = state.Insights - InsightCost };
            state = GetModifiedState(state);
            return state;
        }

        protected abstract EncounterState GetModifiedState(EncounterState state);
    }
}