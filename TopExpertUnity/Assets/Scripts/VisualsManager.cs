using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualsManager
{
    private readonly List<EncounterKeyframe> encounterKeyframes = new List<EncounterKeyframe>();
    private HashSet<CardVisual> cardVisuals;
    private HashSet<DangerVisual> dangerVisuals;
    private HashSet<HudVisual> hudVisuals;

    public void AddKeys(List<EncounterKeyframe> keyframes)
    {
        encounterKeyframes.AddRange(keyframes);
    }

    private void Display(EncounterKeyframe frameA,  EncounterKeyframe frameB, float param)
    {
        UpdateHudVisuals(frameA, frameB, param);
        UpdateCards(frameA, frameB, param);
        UpdateDangers(frameA, frameB, param);
    }

    private void UpdateHudVisuals(EncounterKeyframe frameA, EncounterKeyframe frameB, float param)
    {
        foreach (HudVisual visual in hudVisuals)
        {
            HudVisualState stateA = frameA.TryGetStateFor(visual);
            HudVisualState stateB = frameB.TryGetStateFor(visual);
            visual.Display(stateA, stateB, param);
        }
    }

    private void UpdateDangers(EncounterKeyframe frameA, EncounterKeyframe frameB, float param)
    {
        foreach (DangerVisual visual in dangerVisuals)
        {
            DangerVisualState stateA = frameA.TryGetStateFor(visual);
            DangerVisualState stateB = frameB.TryGetStateFor(visual);
            visual.Display(stateA, stateB, param);
        }
    }

    private void UpdateCards(EncounterKeyframe frameA, EncounterKeyframe frameB, float param)
    {
        foreach (CardVisual visual in cardVisuals)
        {
            CardVisualState stateA = frameA.TryGetStateFor(visual);
            CardVisualState stateB = frameB.TryGetStateFor(visual);
            visual.Display(stateA, stateB, param);
        }
    }
}

public abstract class TrackedVisual<T>
{
    public abstract void Display(T stateA, T stateB, float param);
}

public class HudVisualState { }

public class HudVisual : TrackedVisual<HudVisualState>
{
    public override void Display(HudVisualState stateA, HudVisualState stateB, float param)
    {

    }
}

public class DangerVisualState { }

public class DangerVisual : TrackedVisual<DangerVisualState>
{
    public override void Display(DangerVisualState stateA, DangerVisualState stateB, float param)
    {

    }
}

public class CardVisualState { }

public class CardVisual : TrackedVisual<CardVisualState>
{
    public override void Display(CardVisualState stateA, CardVisualState stateB, float param)
    {

    }
}