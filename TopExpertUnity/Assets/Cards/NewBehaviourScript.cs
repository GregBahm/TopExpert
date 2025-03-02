using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public record WitnessCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;
    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Add 1 witness to the board with 3 charges, dissolves");
    }
}

public record WitnessEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("Starts with 3 charges");
    }
}

public record InterviewWitnessesCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Remove 1 charge from each witness. Gain insight per remaining witness charges");
    }
}

public record InterpersonalSkillsCardCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Gain 2 insight for each witness");
    }
}
public record ResearchTheArchivesCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Add 1 archives if none exists, Gain 1 insight per archives searched level");
    }
}

public record TheArchivesEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("Breakthrough: At “archives searched” 10, gain 10 advantage and remove archives");
    }
}

public record StudiousResearchCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Increase archives searched by 1");
    }
}

public record ForensicInvestigationCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Gain 1 Evidence for Study card, Spawn Still Moving");
    }
}

public record OverthinkerCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("+ 1 insight per “overthinker” in deck, Adds 1 Overthinker card to the deck");
    }
}

public record HyperfocusCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("+ 1 insight each turn, reduce draw size next turn");
    }
}

public record TherapeuticExerciseCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Clear all Overthinker cards in deck");
    }
}

public record StillMovingEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("Next turn add 3 danger and remove this");
    }
}

public record HandsOnResearchCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Gain research equal to danger gained at end of turn");
    }
}

public record HauntedCoffeeMachineEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("Every turn, do 1 danger, Increase danger each time deck is shuffled");
    }
}

public record SeanceCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Costs 5 insight. Does 1 damage. Adds 1 ghost witness");
    }
}

public record GhostWitnessEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("Witness with 10 charges. 1 advantage and 1 danger each time a charge is removed");
    }
}

public record KeyEvidenceCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Costs 5 insight. Transforms into Unravel the Mystery");
    }
}

public record MysteryUnravelledCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Gain 2 advantage");
    }
}

public record NewPlanCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Discard hand and draw that many cards");
    }
}

public record CautionCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Prevent 2 danger");
    }
}

public record BuildDefensesCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Costs 5 insights, Prevents 1 danger each turn");
    }
}

public record NewLeadsCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Costs 3 insights, Draw 3 cards");
    }
}

public record LayTheTrapCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Adds Lay the Trap modifier, becomes Spring the Trap card");
    }
}
public record SpringTheTrapCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Gain advantage equal to charges on “lay the trap” modifiers");
    }
}
public record MomentumCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Costs 4 insight, Gain 1 energy");
    }
}
public record RecuperateCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("Remove 1 danger");
    }
}