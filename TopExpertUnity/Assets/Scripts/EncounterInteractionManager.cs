using Investigation.Behaviors;
using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

[RequireComponent(typeof(EncounterManager))]
public class EncounterInteractionManager : MonoBehaviour
{
    private EncounterManager manager;

    public CardVisualController HoveredCard { get; private set; }

    public static EncounterInteractionManager Instance { get; private set; }

    private PointerEventData pointerData;
    List<RaycastResult> hoveredElements;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        manager = GetComponent<EncounterManager>();
        pointerData = new PointerEventData(EventSystem.current);
        hoveredElements = new List<RaycastResult>();
    }
    private void LateUpdate()
    {
        pointerData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointerData, hoveredElements);
        UpdateHoveredCard();
    }
    private void UpdateHoveredCard()
    {
        if (hoveredElements.Any())
        {
            CardVisualController card = hoveredElements[0].gameObject.GetComponent<CardVisualController>();
            if (card != null)
                HoveredCard = card;
            else
                HoveredCard = null;
        }
        else
        {
            HoveredCard = null;
        }
    }

    public void EndTurn()
    {
        manager.Encounter.EndRound();
    }

    public void ExposeDanger()
    {
        if (manager.Encounter.CanActiateDangerPhase())
            manager.Encounter.ActivateDangerPhase();
    }

    public void PlayCard(ElementIdentifier cardId)
    {
        PlayerCard card = GetCard(cardId);
        manager.Encounter.PlayCard(card);
    }

    private PlayerCard GetCard(ElementIdentifier cardId)
    {
        EncounterState state = manager.Encounter.CurrentState;
        return state.AllCards.First(item => item.Identifier == cardId);
    }

    internal bool GetIsPlayable(ElementIdentifier cardId)
    {
        PlayerCard card = GetCard(cardId);
        EncounterState state = manager.Encounter.CurrentState;
        return card.CanPlay(state);
    }
}
