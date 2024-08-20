namespace Encounter.Model
{
    public abstract class StandardPlayerCard : PlayerCard
    {
        public abstract int ActionCost { get; }

        public override bool CanPlay(EncounterState state)
        {
            return state.Actions >= ActionCost;
        }

        public override EncounterState Play(EncounterState state)
        {
            state = GetWithCardMovedFromHand(state);
            state = state with { Actions = state.Actions - ActionCost };
            state = GetModifiedState(state);
            return state;
        }

        protected abstract EncounterState GetModifiedState(EncounterState state);
    }
}