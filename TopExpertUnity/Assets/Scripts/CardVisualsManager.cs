using Investigation.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Investigation.Model;
using System.Linq;
using TMPro;
using System.Resources;

namespace Investigation.Behaviors
{
    public class CardVisualsManager : ElementVisualManager<PlayerCard, CardVisualController, CardUiState>
    {
        protected override Dictionary<ElementIdentifier, CardUiState> GetEffectorStates(EncounterState previousState, EncounterState nextState)
        {
            Dictionary<ElementIdentifier, CardUiState> ret = new Dictionary<ElementIdentifier, CardUiState>();

            ProcessPreviousCardSet(ret, previousState.Hand, CardUiLocation.Hand, previousState);
            ProcessPreviousCardSet(ret, previousState.DrawDeck, CardUiLocation.DrawDeck, previousState);
            ProcessPreviousCardSet(ret, previousState.DiscardDeck, CardUiLocation.Discard, previousState);
            ProcessPreviousCardSet(ret, previousState.DissolveDeck, CardUiLocation.Dissolve, previousState);

            ProcessNextCardSet(ret, nextState.Hand, CardUiLocation.Hand, nextState, previousState);
            ProcessNextCardSet(ret, nextState.DrawDeck, CardUiLocation.DrawDeck, nextState, previousState);
            ProcessNextCardSet(ret, nextState.DiscardDeck, CardUiLocation.Discard, nextState, previousState);
            ProcessNextCardSet(ret, nextState.DissolveDeck, CardUiLocation.Dissolve, nextState, previousState);

            return ret;
        }

        protected override CardVisualController InstantiateController(PlayerCard card)
        {
            return EncounterVisualsManager.Instance.InstantiateCardUi(card);
        }

        private void ProcessPreviousCardSet(Dictionary<ElementIdentifier, CardUiState> dictionary,
            IReadOnlyList<PlayerCard> cardSet,
            CardUiLocation location,
            EncounterState state)
        {
            for (int i = 0; i < cardSet.Count; i++)
            {
                PlayerCard card = cardSet[i];
                CardUiState uiState = CreateCardUiState(card, i, location, state);
                dictionary.Add(card.Identifier, uiState);
            }
        }

        private void ProcessNextCardSet(Dictionary<ElementIdentifier, CardUiState> dictionary,
            IReadOnlyList<PlayerCard> cardSet,
            CardUiLocation endLocation,
            EncounterState endState,
            EncounterState startState)
        {
            for (int i = 0; i < cardSet.Count; i++)
            {
                PlayerCard card = cardSet[i];
                if (dictionary.ContainsKey(card.Identifier))
                    dictionary[card.Identifier] = CompleteCardUiState(card, i, endLocation, dictionary[card.Identifier], endState);
                else
                    dictionary.Add(card.Identifier, CompleteCardUiState(card, i, endLocation, endState, startState));
            }
        }

        private CardUiState CreateCardUiState(PlayerCard card, int cardOrder, CardUiLocation location, EncounterState state)
        {
            return new CardUiState()
            {
                Identifier = card.Identifier,
                StartElementState = card,
                StartOrder = cardOrder,
                StartLocation = location,
                StartState = state
            };
        }
        private CardUiState CompleteCardUiState(PlayerCard endCard, int endCardOrder, CardUiLocation endLocation, EncounterState endState, EncounterState startState)
        {
            return new CardUiState()
            {
                StartLocation = CardUiLocation.Inexistant,
                StartState = startState,
                StartOrder = 0,
                StartElementState = endCard,
                EndOrder = endCardOrder,
                EndLocation = endLocation,
                EndState = endState
            };
        }
        private CardUiState CompleteCardUiState(PlayerCard endCard, int endCardOrder, CardUiLocation endLocation, CardUiState previousState, EncounterState endState)
        {
            return previousState with
            {
                StartElementState = endCard,
                EndOrder = endCardOrder,
                EndLocation = endLocation,
                EndState = endState
            };
        }
    }
}