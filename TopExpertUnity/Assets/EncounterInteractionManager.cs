using Investigation.Behaviors;
using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(EncounterManager))]
public class EncounterInteractionManager : MonoBehaviour
{
    private EncounterManager manager;

    public static EncounterInteractionManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        manager = GetComponent<EncounterManager>();
    }

    public void EndTurn()
    {
        manager.Encounter.EndRound();
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
}
