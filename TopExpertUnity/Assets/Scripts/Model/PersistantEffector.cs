﻿using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public abstract record PersistantEffector() : IStateModifier, ITrackedElement
    {
        public ElementIdentifier Identifier { get; init; } = new ElementIdentifier();

        public virtual bool IsEnemyEffect => false;

        /// <summary>
        /// The amount of danger the effector will try to inflict on the player
        /// </summary>
        public virtual int GetIntendedDanger(EncounterState state) { return 0; }

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
}