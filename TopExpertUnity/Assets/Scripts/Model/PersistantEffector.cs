using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public abstract record PersistantEffector(EffectorIdentifier Identifier) : IStateModifier
    {
        public virtual bool IsEnemyEffect => false;

        public EncounterState GetModifiedState(EncounterState state)
        {
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            unappliedEffectors.Remove(this);
            state = state with { UnappliedEffectors = unappliedEffectors };

            List<PersistantEffector> appliedEffectors = state.AppliedEffectors.ToList();
            appliedEffectors.Add(GetSelfAfterEffect(state));
            state = state with { AppliedEffectors= appliedEffectors };

            return GetEffectedState(state);
        }

        protected virtual PersistantEffector GetSelfAfterEffect(EncounterState state)
        {
            return this;
        }

        protected abstract EncounterState GetEffectedState(EncounterState state);
    }

    public class EffectorIdentifier { }
}