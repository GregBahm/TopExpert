using Investigation.Behaviors;
using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UIElements;


namespace Investigation.Behaviors
{

    public class EffectorVisualManager : ElementVisualManager<PersistantEffector, EffectorVisualController, EffectorUiState>
    {
        protected override Dictionary<ElementIdentifier, EffectorUiState> GetEffectorStates(EncounterState previousState, EncounterState nextState)
        {
            Dictionary<ElementIdentifier, EffectorUiState> ret = new Dictionary<ElementIdentifier, EffectorUiState>();

            ProcessPreviousEffectors(ret, previousState.UnappliedEffectors, previousState, EffectorExistenceLocation.Unapplied);
            ProcessPreviousEffectors(ret, previousState.AppliedEffectors, previousState, EffectorExistenceLocation.Applied);


            return ret;
        }

        private void ProcessPreviousEffectors(Dictionary<ElementIdentifier, EffectorUiState> dicationary, IReadOnlyList<PersistantEffector> cardSet, EncounterState state, EffectorExistenceLocation location)
        {
            for (int i = 0; i < cardSet.Count; i++)
            {
                PersistantEffector effector = cardSet[i];
                EffectorUiState uiState = CreateEffectorUiState(effector, i, state, location);
                dicationary.Add(effector.Identifier, uiState);
            }
        }

        private EffectorUiState CreateEffectorUiState(PersistantEffector effector, int i, EncounterState state, EffectorExistenceLocation location)
        {
            return new EffectorUiState()
            {
                Identifier = effector.Identifier,
                StartElementState = effector,
                StartLocation = location,
                StartState = state,
            };
        }

        protected override EffectorVisualController InstantiateController(PersistantEffector effector)
        {
            return EncounterVisualsManager.Instance.InstantiateEffectorUi(effector);
        }
    }
}

public enum EffectorExistenceLocation
{
    Inexistant,
    Unapplied,
    Applied,
    Removed
}