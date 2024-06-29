using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterKeyframe
{
    private readonly Dictionary<CardVisual, CardVisualState> cardStates;
    private readonly Dictionary<DangerVisual, DangerVisualState> dangerStates;
    private readonly Dictionary<HudVisual, HudVisualState> hudStates;


    public DangerVisualState TryGetStateFor(DangerVisual danger)
    {
        if (dangerStates.ContainsKey(danger))
            return dangerStates[danger];
        return null;
    }

    public CardVisualState TryGetStateFor(CardVisual card)
    {
        if (cardStates.ContainsKey(card))
            return cardStates[card];
        return null;
    }

    public HudVisualState TryGetStateFor(HudVisual card)
    {
        if (hudStates.ContainsKey(card))
            return hudStates[card];
        return null;
    }
}