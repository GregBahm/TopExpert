using Investigation.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Investigation.Model;
using System.Linq;
using TMPro;
using System.Resources;

public class CardVisualsManager
{
    private Dictionary<ElementIdentifier, CardBehavior> cardUi = new Dictionary<ElementIdentifier, CardBehavior>();

    public void VisualizeEncounter(EncounterState previousState, EncounterState nextState)
    {
        Dictionary<ElementIdentifier, CardUiState> cardStates = GetCardStates(previousState, nextState);
        foreach (var item in cardStates)
        {
            if (!cardUi.ContainsKey(item.Key))
            {
                PlayerCard card = item.Value.StartCardState ?? item.Value.EndCardState;
                CardBehavior viewModel = EncounterVisualsManager.Instance.InstantiateCardUi(card);
                cardUi.Add(item.Key, viewModel);
            }
        }
        List<ElementIdentifier> toDelete = new List<ElementIdentifier>();
        foreach (var item in cardUi)
        {
            if (cardStates.ContainsKey(item.Key))
            {
                CardBehavior viewModel = cardUi[item.Key];
                CardUiState state = cardStates[item.Key];
                viewModel.SetDrawState(state);
            }
            else
            {
                toDelete.Add(item.Key);
            }
        }
        DeleteObseleteUi(toDelete);
    }

    private void DeleteObseleteUi(List<ElementIdentifier> toDelete)
    {
        foreach (var item in toDelete)
        {
            CardBehavior behavior = cardUi[item];
            GameObject.Destroy(behavior.gameObject);
            cardUi.Remove(item);
        }
    }

    private Dictionary<ElementIdentifier, CardUiState> GetCardStates(EncounterState previousState, EncounterState nextState)
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
            StartCardState = card, 
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
            EndCardState = endCard,
            EndOrder = endCardOrder,
            EndLocation = endLocation,
            EndState = endState
        };
    }
    private CardUiState CompleteCardUiState(PlayerCard endCard, int endCardOrder, CardUiLocation endLocation, CardUiState previousState, EncounterState endState)
    {
        return previousState with
        {
            EndCardState = endCard,
            EndOrder = endCardOrder,
            EndLocation = endLocation,
            EndState = endState
        };
    }
}
