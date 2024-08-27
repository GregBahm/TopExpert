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
    private Dictionary<ElementIdentifier, CardViewModel> cardUi;

    public void DrawEncounter(int turn, float progression)
    {
        EncounterStep previousStep = EncounterManager.Instance.Encounter.GetStep(turn);
        EncounterStep nextStep = EncounterManager.Instance.Encounter.GetStep(turn + 1);

        DrawEncounter(previousStep.State, nextStep.State, progression);
    }

    public void DrawEncounter(EncounterState previousState, EncounterState nextState, float progression)
    {
        HandleCards(previousState, nextState, progression);
    }

    private void HandleCards(EncounterState previousState, EncounterState nextState, float progression)
    {
        Dictionary<ElementIdentifier, CardUiState> cardStates = GetCardStates(previousState, nextState);
        foreach (var item in cardStates)
        {
            if (!cardUi.ContainsKey(item.Key))
            {
                PlayerCard card = item.Value.StartCardState ?? item.Value.EndCardState;
                cardUi.Add(item.Key, InstantiateCardUi(card));
            }
        }
        List<ElementIdentifier> toDelete = new List<ElementIdentifier>();
        foreach (var item in cardUi)
        {
            if (cardStates.ContainsKey(item.Key))
            {
                CardViewModel model = cardUi[item.Key];
                CardUiState state = cardStates[item.Key];
                model.DrawState(state, progression);
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
            CardViewModel behavior = cardUi[item];
            GameObject.Destroy(behavior.gameObject);
            cardUi.Remove(item);
        }
    }

    private CardViewModel InstantiateCardUi(PlayerCard card)
    {
        GameObject cardPrefab = CardVisualBindings.Instance.GetPrefabFor(card);
        GameObject obj = GameObject.Instantiate(cardPrefab);
        CardViewModel cardViewModel = obj.GetComponent<CardViewModel>();
        return cardViewModel;
    }

    private Dictionary<ElementIdentifier, CardUiState> GetCardStates(EncounterState previousState, EncounterState nextState)
    {
        Dictionary<ElementIdentifier, CardUiState> ret = new Dictionary<ElementIdentifier, CardUiState>();

        ProcessPreviousCardSet(ret, previousState.Hand, CardUiLocation.Hand, previousState);
        ProcessPreviousCardSet(ret, previousState.DrawDeck, CardUiLocation.DrawDeck, previousState);
        ProcessPreviousCardSet(ret, previousState.DiscardDeck, CardUiLocation.Discard, previousState);
        ProcessPreviousCardSet(ret, previousState.DissolveDeck, CardUiLocation.Dissolve, previousState);

        ProcessNextCardSet(ret, previousState.Hand, CardUiLocation.Hand, nextState);
        ProcessNextCardSet(ret, previousState.DrawDeck, CardUiLocation.DrawDeck, nextState);
        ProcessNextCardSet(ret, previousState.DiscardDeck, CardUiLocation.Discard, nextState);
        ProcessNextCardSet(ret, previousState.DissolveDeck, CardUiLocation.Dissolve, nextState);

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
        CardUiLocation location, 
        EncounterState state)
    {
        for (int i = 0; i < cardSet.Count; i++)
        {
            PlayerCard card = cardSet[i];
            if (dictionary.ContainsKey(card.Identifier))
                dictionary[card.Identifier] = CompleteCardUiState(card, i, location, dictionary[card.Identifier], state);
            else
                dictionary.Add(card.Identifier, CompleteCardUiState(card, i, location, state));
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
    private CardUiState CompleteCardUiState(PlayerCard card, int cardOrder, CardUiLocation location, EncounterState state)
    {
        return new CardUiState()
        {
            StartLocation = CardUiLocation.Inexistant,
            EndCardState = card,
            EndOrder = cardOrder,
            EndLocation = location,
            EndState = state
        };
    }

    private CardUiState CompleteCardUiState(PlayerCard card, int cardOrder, CardUiLocation location, CardUiState previousState, EncounterState state)
    {
        return previousState with
        {
            EndCardState = card,
            EndOrder = cardOrder,
            EndLocation = location,
            EndState = state
        };
    }
}

public class EncounterVisualsManager : MonoBehaviour
{
    private CardVisualsManager cardVisualsManager;

    public static EncounterVisualsManager Instance { get; private set; }

    [SerializeField]
    private RectTransform handLeftPoint;
    public RectTransform HandLeftPoint => handLeftPoint;
    [SerializeField]
    private RectTransform handRightPoint;
    public RectTransform HandsRightPoint => handRightPoint;
    [SerializeField]
    private RectTransform drawDeckPoint;
    public RectTransform DrawDeckPoint => drawDeckPoint;
    [SerializeField]
    private RectTransform dissolvePoint;
    public RectTransform DissolvePoint => dissolvePoint;
    [SerializeField]
    private RectTransform discardPoint;
    public RectTransform DiscardPoint => discardPoint;


    private void Awake()
    {
        Instance = this;
    }
}

public class CardViewModel : MonoBehaviour
{
    public void DrawState(CardUiState state, float progression)
    {
        Vector3 cardLocation = GetLocation(state, progression);
        gameObject.transform.position = cardLocation;
    }

    private Vector3 GetLocation(CardUiState state, float progression)
    {
        Vector3 startLocation = GetStartLocation(state);
        Vector3 endLocation = GetEndLocation(state);
        return Vector3.Lerp(startLocation, endLocation, progression);
    }

    private Vector3 GetStartLocation(CardUiState state)
    {
        switch (state.StartLocation)
        {
            case CardUiLocation.Hand:
                return GetHandPosition(state.StartState, state.StartOrder);
            case CardUiLocation.DrawDeck:
                return GetDrawDeckPosition();
            case CardUiLocation.Discard:
                return GetDiscardPosition();
            case CardUiLocation.Dissolve:
                return GetDissovleDeckPosition();
            case CardUiLocation.Inexistant:
            default:
                return GetEndLocation(state);
        }
    }

    private Vector3 GetEndLocation(CardUiState state)
    {
        switch (state.StartLocation)
        {
            case CardUiLocation.Hand:
            return GetHandPosition(state.StartState, state.StartOrder);
        case CardUiLocation.DrawDeck:
            return GetDrawDeckPosition();
        case CardUiLocation.Discard:
            return GetDiscardPosition();
        case CardUiLocation.Dissolve:
            return GetDissovleDeckPosition();
        case CardUiLocation.Inexistant:
        default:
            return GetStartLocation(state);
        }
    }

    private Vector3 GetHandPosition(EncounterState state, int order)
    {
        EncounterVisualsManager encounterVisuals = EncounterVisualsManager.Instance;
        int cardsInHand = state.Hand.Count;
        float param = (float)order / cardsInHand;
        Vector3 pos = Vector3.Lerp(encounterVisuals.HandLeftPoint.position, encounterVisuals.HandsRightPoint.position, param);
        return pos;
    }

    public Vector3 GetDrawDeckPosition()
    {
        // Later this method will need to be expanded
        return EncounterVisualsManager.Instance.DrawDeckPoint.position;
    }

    public Vector3 GetDissovleDeckPosition()
    {
        // Later this method will need to be expanded
        return EncounterVisualsManager.Instance.DissolvePoint.position;
    }

    public Vector3 GetDiscardPosition()
    {
        // Later this method will need to be expanded
        return EncounterVisualsManager.Instance.DiscardPoint.position;
    }
}
