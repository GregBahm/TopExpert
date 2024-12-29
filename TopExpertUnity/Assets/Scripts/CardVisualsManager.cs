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

            ProcessPreviousCardSet(ret, previousState.Hand, CardExistenceLocation.Hand, previousState);
            ProcessPreviousCardSet(ret, previousState.DrawDeck, CardExistenceLocation.DrawDeck, previousState);
            ProcessPreviousCardSet(ret, previousState.DiscardDeck, CardExistenceLocation.Discard, previousState);
            ProcessPreviousCardSet(ret, previousState.DissolveDeck, CardExistenceLocation.Dissolve, previousState);

            ProcessNextCardSet(ret, nextState.Hand, CardExistenceLocation.Hand, nextState, previousState);
            ProcessNextCardSet(ret, nextState.DrawDeck, CardExistenceLocation.DrawDeck, nextState, previousState);
            ProcessNextCardSet(ret, nextState.DiscardDeck, CardExistenceLocation.Discard, nextState, previousState);
            ProcessNextCardSet(ret, nextState.DissolveDeck, CardExistenceLocation.Dissolve, nextState, previousState);

            return ret;
        }

        protected override CardVisualController InstantiateController(PlayerCard card)
        {
            return EncounterVisualsManager.Instance.InstantiateCardUi(card);
        }

        private void ProcessPreviousCardSet(Dictionary<ElementIdentifier, CardUiState> dictionary,
            IReadOnlyList<PlayerCard> cardSet,
            CardExistenceLocation location,
            EncounterState state)
        {
            for (int i = 0; i < cardSet.Count; i++)
            {
                PlayerCard card = cardSet[i];
                CardUiState uiState = StartCardUiState(card, i, location, state);
                dictionary.Add(card.Identifier, uiState);
            }
        }

        private void ProcessNextCardSet(Dictionary<ElementIdentifier, CardUiState> dictionary,
            IReadOnlyList<PlayerCard> cardSet,
            CardExistenceLocation endLocation,
            EncounterState endState,
            EncounterState startState)
        {
            for (int i = 0; i < cardSet.Count; i++)
            {
                PlayerCard card = cardSet[i];
                if (dictionary.ContainsKey(card.Identifier))
                    dictionary[card.Identifier] = FinishCardUiState(card, i, endLocation, dictionary[card.Identifier], endState);
                else
                    dictionary.Add(card.Identifier, CreateCardUiStateFromInexistence(card, i, endLocation, endState, startState));
            }
        }

        private CardUiState StartCardUiState(PlayerCard card, int cardOrder, CardExistenceLocation location, EncounterState state)
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
        private CardUiState CreateCardUiStateFromInexistence(PlayerCard endCard, int endCardOrder, CardExistenceLocation endLocation, EncounterState endState, EncounterState startState)
        {
            return new CardUiState()
            {
                Identifier = endCard.Identifier,
                StartElementState = null,
                StartLocation = CardExistenceLocation.Inexistant,
                StartState = startState,
                StartOrder = 0,
                EndElementState = endCard,
                EndLocation = endLocation,
                EndState = endState,
                EndOrder = endCardOrder,
            };
        }
        private CardUiState FinishCardUiState(PlayerCard endCard, int endCardOrder, CardExistenceLocation endLocation, CardUiState previousState, EncounterState endState)
        {
            return previousState with
            {
                EndElementState = endCard,
                EndOrder = endCardOrder,
                EndLocation = endLocation,
                EndState = endState
            };
        }
    }
}