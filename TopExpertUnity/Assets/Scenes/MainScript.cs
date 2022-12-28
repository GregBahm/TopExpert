using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public Encounter Encounter;

    private void Start()
    {
        Encounter = GetTestEncounter();
    }

    private Encounter GetTestEncounter()
    {
        BattleStateBuilder builder = new BattleStateBuilder();

        EntityStateBuilder firstInspector = new EntityStateBuilder(new EntityId());
        EntityStateBuilder secondInspector = new EntityStateBuilder(new EntityId());

        EntityStateBuilder firstEnemy = new EntityStateBuilder(new EntityId());
        EntityStateBuilder secondEnemy = new EntityStateBuilder(new EntityId());

        builder.Allies.Add(firstInspector);
        builder.Allies.Add(secondInspector);
        builder.Enemies.Add(firstEnemy);
        builder.Enemies.Add(secondEnemy);

        return new Encounter(builder.ToState());
    }
}

public class Encounter
{
    private readonly List<BattleState> battleProgression;
    public BattleState CurrentState { get { return battleProgression.Last(); } }

    public Encounter(BattleState initialState)
    {
        battleProgression = new List<BattleState>() { initialState };
    }
}

public enum BattleStatus
{
    Ongoing,
    PlayersWon,
    PlayersLost
}

public class BattleState
{
    public BattleStatus Status;

    public PlayerHand Hand;

    public IReadOnlyList<EntityState> Allies { get; }
    public IReadOnlyList<EntityState> Enemies { get; }

    public IReadOnlyList<Card> DrawDeck { get; }
    public IReadOnlyList<Card> DiscardDeck { get; }
    public IReadOnlyList<Card> BurnedCardsDeck { get; }

    public BattleState(BattleStatus status, 
        PlayerHand hand, 
        List<EntityState> allies, 
        List<EntityState> enemies,
        List<Card> drawDeck,
        List<Card> discardDeck,
        List<Card> burnedCardsDeck)
    {
        Status = status;
        Hand = hand;
        Allies = allies;
        Enemies = enemies;

        DrawDeck = drawDeck;
        DiscardDeck = discardDeck;
        BurnedCardsDeck= burnedCardsDeck;
    }

    public BattleState PlayCard(Card cardToPlay, EntityState potentialTarget)
    {
        BattleStateBuilder builder = new BattleStateBuilder(this);
        builder.Hand.Cards.Remove(cardToPlay);

    }

    public BattleState EndRound()
    {
    }
}

public class BattleStateBuilder
{
    public BattleStatus Status { get; set; }
    public PlayerHandBuilder Hand { get; }

    public List<EntityStateBuilder> Allies { get; }
    public List<EntityStateBuilder> Enemies { get; }

    public List<Card> DrawDeck { get; }
    public List<Card> DiscardDeck { get; }
    public List<Card> BurnedCardsDeck { get; }

    public BattleStateBuilder()
    {
        Hand = new PlayerHandBuilder();
    }

    public BattleStateBuilder(BattleState state)
    {
        Status = state.Status;
        Hand = new PlayerHandBuilder(state.Hand);
        Allies = state.Allies.Select(item => new EntityStateBuilder(item)).ToList();
        Enemies = state.Enemies.Select(item => new EntityStateBuilder(item)).ToList();

        DrawDeck = state.DrawDeck.ToList();
        DiscardDeck = state.DiscardDeck.ToList();
        BurnedCardsDeck= state.BurnedCardsDeck.ToList();
    }

    public BattleState ToState()
    {
        PlayerHand hand = Hand.ToHand();
        List<EntityState> allies = Allies.Select(item => item.ToState()).ToList();
        List<EntityState> enemies = Enemies.Select(item => item.ToState()).ToList();
        return new BattleState(Status,
            hand,
            allies,
            enemies,
            DrawDeck,
            DiscardDeck,
            BurnedCardsDeck);
    }
}

public class EntityState
{
    public EntityId Id { get; }
    public int MaxHP { get; }
    public int CurrentHP { get; }

    // List status effect states here

    public EntityState(EntityId id,
        int maxHP,
        int currentHP)
    {
        Id = id;
        MaxHP = maxHP;
        CurrentHP = currentHP;
    }
}

public class EntityStateBuilder
{
    public EntityId Id { get; }
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }

    public EntityStateBuilder(EntityId id)
    {
        Id = id;
    }
    public EntityStateBuilder(EntityState state)
    {
        Id = state.Id;
        MaxHP = state.MaxHP;
        CurrentHP = state.CurrentHP;
    }

    public EntityState ToState()
    {
        return new EntityState(
            Id,
            MaxHP, 
            CurrentHP);
    }
}

public class PlayerHandBuilder
{
    public List<Card> Cards { get; set; }

    public PlayerHandBuilder()
    {
        Cards = new List<Card>();
    }

    public PlayerHandBuilder(PlayerHand hand)
    {
        Cards = hand.Cards.ToList();
    }

    public PlayerHand ToHand()
    {
        return new PlayerHand(Cards);
    }
}

public class PlayerHand
{
    public IReadOnlyList<Card> Cards { get; }

    public PlayerHand(List<Card> cards)
    {
        Cards = cards;
    }
}

public abstract class Card
{
    public string Name { get; }
    public abstract CardPlayability GetPlayability(BattleState state);
    public abstract BattleState Apply(BattleState state, EntityState potentialTarget);
}

public class EntityId { }

public class CardPlayability
{
    public bool IsPlayable { get; }
    public bool NeedsTarget { get; }
    public IReadOnlyList<EntityId> TargetableAllies { get; }
    public IReadOnlyList<EntityId> TargetableEnemies { get; }
}