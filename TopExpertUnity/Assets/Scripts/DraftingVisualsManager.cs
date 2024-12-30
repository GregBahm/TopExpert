using Investigation.Behaviors;
using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;


namespace Investigation.Behaviors
{
    public record DraftUiState : ElementUIState<DraftOption>
    {
        public DraftExistenceLocation StartLocation { get; init; }
        public int StartOrder { get; init; }

        public DraftExistenceLocation EndLocation { get; init; }
        public int EndOrder { get; init; }
    }

    public enum DraftExistenceLocation
    {
        Inexistant,
        DraftDeck,
        DraftOptions,
    }

    public class DraftingVisualsManager : ElementVisualManager<DraftOption, DraftOptionController, DraftUiState>
    {
        protected override Dictionary<ElementIdentifier, DraftUiState> GetEffectorStates(EncounterState previousState, EncounterState nextState)
        {
            Dictionary<ElementIdentifier, DraftUiState> ret = new Dictionary<ElementIdentifier, DraftUiState>();

            ProcessPreviousDrafts(ret, previousState.DraftDeck, previousState, DraftExistenceLocation.DraftDeck);
            ProcessPreviousDrafts(ret, previousState.DraftOptions, previousState, DraftExistenceLocation.DraftOptions);

            ProcessNextDrafts(ret, nextState.DraftDeck, DraftExistenceLocation.DraftDeck, nextState, previousState);
            ProcessNextDrafts(ret, nextState.DraftOptions, DraftExistenceLocation.DraftOptions, nextState, previousState);

            return ret;
        }

        private void ProcessNextDrafts(Dictionary<ElementIdentifier, DraftUiState> dictionary, 
            IReadOnlyList<DraftOption> draftOptions,
            DraftExistenceLocation location, 
            EncounterState nextState, 
            EncounterState previousState)
        {
            for (int i = 0; i < draftOptions.Count; i++)
            {
                DraftOption draftOption = draftOptions[i];
                if (dictionary.ContainsKey(draftOption.Identifier))
                    dictionary[draftOption.Identifier] = FinishDraftUiState(draftOption, i, location, dictionary[draftOption.Identifier], nextState);
                else
                {
                    DraftUiState uiState = CreateDraftStateFromInexistence(draftOption, i, nextState, previousState, location);
                    dictionary.Add(draftOption.Identifier, uiState);
                }
            }
        }

        private DraftUiState StartDraftUiState(DraftOption draftOption, 
            int order, 
            EncounterState startState,
            DraftExistenceLocation location)
        {
            return new DraftUiState()
            {
                Identifier = draftOption.Identifier,
                StartElementState = draftOption,
                StartLocation = location,
                StartState = startState,
                StartOrder = order,
            };
        }

        private DraftUiState CreateDraftStateFromInexistence(DraftOption endDraftable, 
            int order, 
            EncounterState startState, 
            EncounterState endState,
            DraftExistenceLocation endLocation)
        {
            return new DraftUiState()
            {
                Identifier = endDraftable.Identifier,
                StartElementState = null,
                StartState = startState,
                StartLocation = DraftExistenceLocation.Inexistant,
                StartOrder = 0,
                EndLocation = endLocation,
                EndState = endState,
                EndElementState = endDraftable,
                EndOrder = order,
            };
        }

        private DraftUiState FinishDraftUiState(DraftOption draftOption, 
            int endOrder,
            DraftExistenceLocation endLocation, 
            DraftUiState uiState, 
            EncounterState endState)
        {
            return uiState with
            {
                EndElementState = draftOption,
                EndLocation = endLocation,
                EndState = endState,
                EndOrder = endOrder
            };
        }

        private void ProcessPreviousDrafts(Dictionary<ElementIdentifier, DraftUiState> dictionary, IReadOnlyList<DraftOption> cardSet, EncounterState state, DraftExistenceLocation location)
        {
            for (int i = 0; i < cardSet.Count; i++)
            {
                DraftOption draftOption = cardSet[i];
                DraftUiState uiState = StartDraftUiState(draftOption, i, state, location);
                dictionary.Add(draftOption.Identifier, uiState);
            }
        }

        protected override DraftOptionController InstantiateController(DraftOption draftOption)
        {
            return EncounterVisualsManager.Instance.InstantiateDraftUi(draftOption);
        }
    }
}