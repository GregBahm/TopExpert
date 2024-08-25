namespace Investigation.Model
{
    public abstract record DraftOption(DraftOptionIdentifier Identifier) : IStateModifier
    {
        public abstract bool CanDraft(EncounterState state);
        public abstract EncounterState DraftCard(EncounterState state);
    }

    public class DraftOptionIdentifier { }
}