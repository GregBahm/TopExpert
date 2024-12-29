using Investigation.Behaviors;
using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UIElements;


namespace Investigation.Behaviors
{

    public class EffectorVisualsManager : ElementVisualManager<PersistantEffector, EffectorVisualController, EffectorUiState>
    {
        protected override Dictionary<ElementIdentifier, EffectorUiState> GetEffectorStates(EncounterState previousState, EncounterState nextState)
        {
            Dictionary<ElementIdentifier, EffectorUiState> ret = new Dictionary<ElementIdentifier, EffectorUiState>();

            ProcessPreviousEffectors(ret, previousState.UnappliedEffectors, previousState, EffectorExistenceLocation.Unapplied);
            ProcessPreviousEffectors(ret, previousState.AppliedEffectors, previousState, EffectorExistenceLocation.Applied);

            ProcessNextEffectors(ret, nextState.UnappliedEffectors, EffectorExistenceLocation.Unapplied, nextState, previousState);
            ProcessNextEffectors(ret, nextState.AppliedEffectors, EffectorExistenceLocation.Applied, nextState, previousState);

            return ret;
        }

        private void ProcessNextEffectors(Dictionary<ElementIdentifier, EffectorUiState> dictionary, 
            IReadOnlyList<PersistantEffector> effectors, 
            EffectorExistenceLocation location, 
            EncounterState nextState, 
            EncounterState previousState)
        {
            for (int i = 0; i < effectors.Count; i++)
            {
                PersistantEffector effector = effectors[i];
                if (dictionary.ContainsKey(effector.Identifier))
                    dictionary[effector.Identifier] = FinishEffectorUiState(effector, i, location, dictionary[effector.Identifier], nextState);
                else
                {
                    EffectorUiState uiState = CreateEffectorStateFromInexistence(effector, i, nextState, previousState, location);
                    dictionary.Add(effector.Identifier, uiState);
                }
            }
        }

        private EffectorUiState StartEffectorUiState(PersistantEffector effector, 
            int order, 
            EncounterState startState, 
            EffectorExistenceLocation location)
        {
            return new EffectorUiState()
            {
                Identifier = effector.Identifier,
                StartElementState = effector,
                StartLocation = location,
                StartState = startState,
                StartOrder = order,
            };
        }

        private EffectorUiState CreateEffectorStateFromInexistence(PersistantEffector endEffector, 
            int order, 
            EncounterState startState, 
            EncounterState endState, 
            EffectorExistenceLocation endLocation)
        {
            return new EffectorUiState()
            {
                Identifier = endEffector.Identifier,
                StartElementState = null,
                StartState = startState,
                StartLocation = EffectorExistenceLocation.Inexistant,
                StartOrder = 0,
                EndLocation = endLocation,
                EndState = endState,
                EndElementState = endEffector,
                EndOrder = order,
            };
        }

        private EffectorUiState FinishEffectorUiState(PersistantEffector effector, 
            int endOrder, 
            EffectorExistenceLocation endLocation, 
            EffectorUiState uiState, 
            EncounterState endState)
        {
            return uiState with
            {
                EndElementState = effector,
                EndLocation = endLocation,
                EndState = endState,
                EndOrder = endOrder
            };
        }

        private void ProcessPreviousEffectors(Dictionary<ElementIdentifier, EffectorUiState> dicationary, IReadOnlyList<PersistantEffector> cardSet, EncounterState state, EffectorExistenceLocation location)
        {
            for (int i = 0; i < cardSet.Count; i++)
            {
                PersistantEffector effector = cardSet[i];
                EffectorUiState uiState = StartEffectorUiState(effector, i, state, location);
                dicationary.Add(effector.Identifier, uiState);
            }
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