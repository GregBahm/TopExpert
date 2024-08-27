namespace Investigation.Model
{
    public abstract record DraftOption(ElementIdentifier Identifier) : IStateModifier, ITrackedElement
    {
        public abstract bool CanDraft(EncounterState state);
        public abstract EncounterState DraftCard(EncounterState state);
    }
}