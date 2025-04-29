using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Add 1 witness to the board with 3 charges, dissolves
public record WitnessCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;
    public override bool DissolvesOnPlay => true;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        WitnessEffector witness = new WitnessEffector();
        return state.GetWithEffectorAdded(witness);
    }
}

// Starts with 3 charges
public record WitnessEffector() : PersistantEffector()
{
    public int Charges { get; init; } = 3;

    protected override EncounterState GetEffectedState(EncounterState state)
    {
        return state;
    }
}

// Remove 1 charge from each witness. Gain insight per remaining witness charges
public record InterviewWitnessesCard() : StandardPlayerCard()
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
public record InterpersonalSkillsCardCard() : StandardPlayerCard()
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
public record ResearchTheArchivesCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        IEnumerable<TheArchivesEffector> archives = state.AllEffectors.OfType<TheArchivesEffector>().ToList();
        if(!archives.Any())
        {
            state = state.GetWithEffectorAdded(new TheArchivesEffector());
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
public record TheArchivesEffector() : PersistantEffector()
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
public record StudiousResearchCard() : StandardPlayerCard()
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

// Gain 1 KeyEvidence card, Spawn Still Moving
public record ForensicInvestigationCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        state = state.GetWithEffectorAdded(new StillMovingEffector());
        state = state.GetWithCardAdded(new KeyEvidenceCard(), CardExistenceLocation.Discard);
        return state;
    }
}

//Adds 1 Overthinker card to the deck, + 1 insight per “overthinker” in deck, 
public record OverthinkerCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        int overthinkers = state.AllCards.OfType<OverthinkerCard>().Count() + 1; // Adding the card below
        OverthinkerCard newOverthinker = new OverthinkerCard();
        EncounterState withCard = state.GetWithCardDiscarded(newOverthinker);
        return withCard with { Insights = state.Insights + overthinkers };

    }
}

// Adds Hyperfocus effector (+ 1 insight each turn) reduce draw size next turn
public record HyperfocusCard() : StandardPlayerCard()
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
            HyperfocusEffector newEffector = new HyperfocusEffector();
            return state.GetWithEffectorAdded(newEffector);
        }
    }
}

// + 1 insight each turn
public record HyperfocusEffector() : PersistantEffector()
{
    public int Intensity { get; init; } = 1;

    protected override EncounterState GetEffectedState(EncounterState state)
    {
        return state with { TemporaryDraws = state.TemporaryDraws + Intensity, Insights = state.Insights + Intensity };
    }
}

// Clear all Overthinker cards in deck
public record TherapeuticExerciseCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        List<OverthinkerCard> overthinkers = state.AllCards.OfType<OverthinkerCard>().ToList();
        foreach(OverthinkerCard overthinker in overthinkers)
        {
            state = state.GetWithCardDissolved(overthinker.Identifier);
        }
        return state;
    }
}

// Next turn add 3 danger and remove this
public record StillMovingEffector() : PersistantEffector()
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Gain research equal to danger gained at end of turn
public record HandsOnResearchCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Costs 5 insight. Does 1 damage. Adds 1 ghost witness
public record SeanceCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Witness with 10 charges. 1 advantage and 1 danger each time a charge is removed
public record GhostWitnessEffector() : PersistantEffector()
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Costs 5 insight. Transforms into Unravel the Mystery
public record KeyEvidenceCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Gain 2 advantage
public record MysteryUnravelledCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        int newAdvantage = state.Advantage + 2;
        return state with { Advantage = newAdvantage };
    }
}

// Discard hand and draw that many cards
public record NewPlanCard() : StandardPlayerCard()
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
public record CautionCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        // Maybe add a modifier that 
        throw new NotImplementedException("Prevent 2 danger");
    }
}

// Costs 5 insights, Prevents 1 danger each turn
public record BuildDefensesCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;
    public override int InsightCost => 5;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Prevents 1 danger each turn
public record BuiltDefensesEffector() : PersistantEffector()
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        return state with { Defenses = state.Defenses + 1 };
    }
}

// Costs 3 insights, Draw 3 cards
public record NewLeadsCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;
    public override int InsightCost => 3;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        state = state.GetWithDraw();
        state = state.GetWithDraw();
        state = state.GetWithDraw();
        return state;
    }
}

// Adds Lay the Trap modifier, becomes Spring the Trap card
public record LayTheTrapCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Gain advantage equal to charges on “lay the trap” modifiers
public record SpringTheTrapCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}

// Costs 4 insight, Gain 1 energy
public record MomentumCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        throw new NotImplementedException();
    }
}

// Remove 1 danger
public record RecuperateCard() : StandardPlayerCard()
{
    public override int ActionCost => 0;

    protected override EncounterState GetModifiedState(EncounterState state)
    {
        return state with { Danger = state.Danger - 1 };
    }
}

// Every turn, apply 1 danger, Increase danger each time deck is shuffled
public record HauntedCoffeeMachineEffector() : PersistantEffector()
{
    protected override EncounterState GetEffectedState(EncounterState state)
    {
        throw new NotImplementedException("");
    }
}