using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Add 1 witness to the board with 3 charges, dissolves
public record WitnessCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;
    public override bool DissolvesOnPlay => true;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        WitnessEffector witness = new WitnessEffector(new ElementIdentifier());
        return state.GetWithEffectorAdded(witness);
    }
}

// Starts with 3 charges
public record WitnessEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    public int Charges { get; init; } = 3;

    protected override EncounterState GetEffectedState(EncounterState state)
    {
        return state;
    }
}

// Remove 1 charge from each witness. Gain insight per remaining witness charges
public record InterviewWitnessesCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        IEnumerable<WitnessEffector> oldWitnesses = state.AllEffectors.OfType<WitnessEffector>();
        EncounterState newState = state;
        foreach(WitnessEffector witness in oldWitnesses)
        {
            WitnessEffector newWitness = witness with { Charges= witness.Charges - 1 };
            if(newWitness.Charges > 0)
            {
                newState = newState.GetWithEffectorReplaced(witness.Identifier, newWitness);
            }
            else
            {
                newState = newState.GetWithEffectorRemoved(witness.Identifier);
            }
        }
        return newState;
    }
}

// Gain 2 insight for each witness
public record InterpersonalSkillsCardCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        int witnesses = state.AllEffectors.OfType<WitnessCard>().Count();
        int newInsights = state.Insights + witnesses * 2;
        return state with { Insights = newInsights };
    }
}

// Add 1 archives if none exists, Gain 1 insight per archives searched level
public record ResearchTheArchivesCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        IEnumerable<TheArchivesEffector> archives = state.AllEffectors.OfType<TheArchivesEffector>().ToList();
        if(!archives.Any())
        {
            state = state.GetWithEffectorAdded(new TheArchivesEffector(new ElementIdentifier()));
        }
        int insight = 0;
        foreach (TheArchivesEffector item in archives)
        {
            insight += item.SearchedLevel;
        }
        return state with { Insights = insight };
    }
}

// Breakthrough: At “archives searched” 10, gain 10 advantage and remove archives
public record TheArchivesEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    public int SearchedLevel { get; init; }

    protected override EncounterState GetEffectedState(EncounterState state)
    {
        if(SearchedLevel >= 10)
        {
            state = state with { Advantage = state.Advantage + 10 };
            state = state.GetWithEffectorRemoved(Identifier); // Make sure this works. It might automatically add itself after the process
        }
        return state;
    }
}

// Increase archives searched by 1
public record StudiousResearchCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        IEnumerable<TheArchivesEffector> archives = state.AllEffectors.OfType<TheArchivesEffector>().ToList();
        foreach (TheArchivesEffector item in archives)
        {
            TheArchivesEffector newArchives = item with { SearchedLevel = item.SearchedLevel + 1 };
            state = state.GetWithEffectorReplaced(item.Identifier, newArchives);
        }
        return state;
    }
}

// Gain 1 Evidence for Study card, Spawn Still Moving
public record ForensicInvestigationCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

//Adds 1 Overthinker card to the deck, + 1 insight per “overthinker” in deck, 
public record OverthinkerCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        int overthinkers = state.AllCards.OfType<OverthinkerCard>().Count() + 1; // Adding the card below
        List<PlayerCard> discard = state.DiscardDeck.ToList();
        OverthinkerCard newOverthinker = new OverthinkerCard(new ElementIdentifier());
        discard.Insert(discard.Count - 1, newOverthinker);
        return state with { DiscardDeck = discard, Insights = state.Insights + overthinkers };

    }
}

// + 1 insight each turn, reduce draw size next turn
public record HyperfocusCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        HyperfocusEffector effector = state.AllEffectors.OfType<HyperfocusEffector>().FirstOrDefault();
        if(effector != null)
        {
            HyperfocusEffector newEffector = effector with { Intensity = effector.Intensity + 1 };
            return state.GetWithEffectorReplaced(effector.Identifier, newEffector);
        }
        else
        {
            HyperfocusEffector newEffector = new HyperfocusEffector(new ElementIdentifier());
            return state.GetWithEffectorAdded(newEffector);
        }
    }
}

public record HyperfocusEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    public int Intensity { get; init; } = 1;

    protected override EncounterState GetEffectedState(EncounterState state)
    {
        return state with { TemporaryDraws = state.TemporaryDraws + Intensity, Insights = state.Insights + Intensity };
    }
}

// Clear all Overthinker cards in deck
public record TherapeuticExerciseCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Next turn add 3 danger and remove this
public record StillMovingEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Gain research equal to danger gained at end of turn
public record HandsOnResearchCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Every turn, do 1 danger, Increase danger each time deck is shuffled
public record HauntedCoffeeMachineEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Costs 5 insight. Does 1 damage. Adds 1 ghost witness
public record SeanceCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Witness with 10 charges. 1 advantage and 1 danger each time a charge is removed
public record GhostWitnessEffector(ElementIdentifier Identifier) : PersistantEffector(Identifier)
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Costs 5 insight. Transforms into Unravel the Mystery
public record KeyEvidenceCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Gain 2 advantage
public record MysteryUnravelledCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        int newAdvantage = state.Advantage + 2;
        return state with { Advantage = newAdvantage };
    }
}

// Discard hand and draw that many cards
public record NewPlanCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 2;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        int handSize = state.Hand.Count;
        List<PlayerCard> hand = state.Hand.ToList();
        foreach (var card in hand)
        {
            state = state.GetWithCardDiscarded(card);
        }
        for (int i = 0; i < handSize; i++)
        {
            state = state.GetWithDraw();
        }
        return state;
    }
}

// Prevent 2 danger
public record CautionCard(ElementIdentifier Identifier) : StandardPlayerCard(Identifier)
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        // Maybe add a modifier that 
        throw new NotImplementedException("Prevent 2 danger");
    }
}

// Costs 5 insights, Prevents 1 danger each turn
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