namespace Investigation.Model
{
    public abstract record DraftOption : IStateModifier
    {
        public abstract bool CanDraft(EncounterState state);
        public abstract EncounterState DraftCard(EncounterState state);
    }
}