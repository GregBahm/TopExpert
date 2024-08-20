namespace Encounter.Model
{
    public abstract class DraftOption : IStateModifier
    {
        public abstract bool CanDraft(EncounterState state);
        public abstract EncounterState DraftCard(EncounterState state);
    }
}