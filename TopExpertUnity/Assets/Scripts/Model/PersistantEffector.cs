using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public abstract class PersistantEffector : IStateModifier
    {
        public EncounterState GetModifiedState(EncounterState state)
        {
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            unappliedEffectors.Remove(this);
            state = state with { UnappliedEffectors = unappliedEffectors };

            List<PersistantEffector> appliedEffectors = state.AppliedEffectors.ToList();
            appliedEffectors.Add(this);
            state = state with { AppliedEffectors= appliedEffectors };
            return ModifyState(state);
        }

        protected abstract EncounterState ModifyState(EncounterState builder);
    }
}