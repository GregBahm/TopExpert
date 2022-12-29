using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.ObjectChangeEventStream;
using static UnityEditor.Progress;

namespace TopExpert.Combat
{
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
            builder.MaxActionPoints = 4;
            builder.MaxHandSize = 7;

            EntityStateBuilder investigators = new EntityStateBuilder(new EntityId());
            investigators.MaxHP = 100;
            investigators.CurrentHP = 100;

            EntityId enemyIdA = new EntityId();
            EntityId enemyIdB = new EntityId();

            BasicEnemyAttackPattern enemyAAttack = new BasicEnemyAttackPattern(enemyIdA);
            BasicEnemyAttackPattern enemyBAttack = new BasicEnemyAttackPattern(enemyIdB);

            EntityStateBuilder firstEnemy = new EntityStateBuilder(enemyIdA);
            firstEnemy.MaxHP = 50;
            firstEnemy.CurrentHP = 50;
            firstEnemy.EnemyAction = enemyAAttack;
            EntityStateBuilder secondEnemy = new EntityStateBuilder(enemyIdB);
            secondEnemy.MaxHP = 50;
            secondEnemy.CurrentHP = 50;
            secondEnemy.EnemyAction = enemyBAttack;

            builder.Investigators = investigators;
            builder.Enemies = new List<EntityStateBuilder> { firstEnemy, secondEnemy };

            builder.DrawDeck = GetTestDeck().OrderBy(item => UnityEngine.Random.value).ToList();
            BattleState initialState = builder.ToState().StartNewRound();

            return new Encounter(initialState);
        }

        private IEnumerable<ICard> GetTestDeck()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new BasicDefense();
            }
            for (int i = 0; i < 10; i++)
            {
                yield return new BasicAttack();
            }
            for (int i = 0; i < 10; i++)
            {
                yield return new BasicResearch();
            }
        }
    }

    public class BasicEnemyAttackPattern : IEnemyAction
    {
        private readonly EntityId selfId;
        private bool attackTurn;

        private readonly int attack = 5;
        private readonly int shield = 5;

        public BasicEnemyAttackPattern(EntityId selfId, bool attackTurn = false)
        {
            this.selfId = selfId;
            this.attackTurn = attackTurn;
        }

        public BattleState TakeAction(BattleState state)
        {
            EntityState selfState = state.Enemies.First(item => item.Id == selfId);
            if (selfState.CurrentHP <= 0)
            {
                return state; // This enemy is dead
            }

            BattleStateBuilder builder = new BattleStateBuilder(state);
            EntityStateBuilder selfBuilder = builder.GetEnemy(selfId);
            if (attackTurn)
            {
                builder.Investigators.ApplyAttackDamage(attack);
            }
            else
            {
                selfBuilder.Shield += shield;
            }
            selfBuilder.EnemyAction = new BasicEnemyAttackPattern(selfId, !attackTurn);
            return builder.ToState();
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
        public BattleStatus Status { get; }

        public EntityState Investigators { get; }
        public IReadOnlyList<EntityState> Enemies { get; }

        public IReadOnlyList<ICard> Hand { get; }
        public IReadOnlyList<ICard> DrawDeck { get; }
        public IReadOnlyList<ICard> DiscardDeck { get; }
        public IReadOnlyList<ICard> BurnedCardsDeck { get; }

        public int MaxResearchPoints { get; }
        public int RemainingResearchPoints { get; }

        public int MaxActionPoints { get; }
        public int RemainingActionPoints { get; }

        public int Draws { get; }
        public int MaxHandSize { get; }

        public BattleState(BattleStatus status,
            EntityState investigators,
            List<EntityState> enemies,
            List<ICard> hand,
            List<ICard> drawDeck,
            List<ICard> discardDeck,
            List<ICard> burnedCardsDeck,
            int maxResearchPoints,
            int remainingResearchPoints,
            int maxActionPoints,
            int remainingActionPoints,
            int draws,
            int maxHandSize)
        {
            Status = status;
            Investigators = investigators;
            Enemies = enemies;
            Hand = hand ?? new List<ICard>();
            DrawDeck = drawDeck ?? new List<ICard>();
            DiscardDeck = discardDeck ?? new List<ICard>();
            BurnedCardsDeck = burnedCardsDeck ?? new List<ICard>();
            MaxResearchPoints = maxResearchPoints;
            RemainingResearchPoints = remainingResearchPoints;
            MaxActionPoints = maxActionPoints;
            RemainingActionPoints = remainingActionPoints;
            Draws = draws;
            MaxHandSize = maxHandSize;
        }

        public BattleState PlayCard(ICard cardToPlay, EntityId potentialTarget)
        {
            BattleStateBuilder builder = new BattleStateBuilder(this);
            builder.Hand.Remove(cardToPlay);
            BattleState asState = builder.ToState();
            BattleState afterCard = cardToPlay.Apply(asState, potentialTarget);
            BattleStateBuilder afterCardBuilder = new BattleStateBuilder(afterCard);
            if (cardToPlay.Consumeable)
            {
                afterCardBuilder.BurnedCardsDeck.Add(cardToPlay);
            }
            else
            {
                afterCardBuilder.DiscardDeck.Add(cardToPlay);
            }
            return afterCardBuilder.ToState();
        }

        public BattleState EndRound()
        {
            return EndRound(this);
        }

        private static BattleState EndRound(BattleState state)
        {
            // The enemies do their effects
            BattleState currentState = state;
            foreach (var item in state.Enemies)
            {
                if (item.EnemyAction != null)
                {
                    currentState = item.EnemyAction.TakeAction(currentState);
                }
            }
            // The battle potentially ends
            if (currentState.Status != BattleStatus.Ongoing)
                return currentState;
            // If not, start next round
            BattleState handDiscarded = GetWithDiscards(currentState);
            return GetWithNewRoundStarted(handDiscarded);
        }

        private static BattleState GetWithNewRoundStarted(BattleState currentState)
        {
            BattleStateBuilder builder = new BattleStateBuilder(currentState);
            builder.RemainingActionpoints = builder.MaxActionPoints;
            int effectiveDraws = Mathf.Max(builder.MaxHandSize, builder.Hand.Count + builder.Draws);
            for (int i = 0; i < effectiveDraws; i++)
            {
                builder.TryDrawCardToHand();
            }

            return builder.ToState();
        }

        public BattleState StartNewRound()
        {
            return GetWithNewRoundStarted(this);
        }

        private static BattleState GetWithDiscards(BattleState currentState)
        {
            BattleStateBuilder builder = new BattleStateBuilder(currentState);
            List<ICard> remainingHand = new List<ICard>();
            foreach (ICard card in builder.Hand)
            {
                if (card.Hold)
                    remainingHand.Add(card);
                else
                    builder.DiscardDeck.Add(card);
            }
            builder.Hand = remainingHand;
            return builder.ToState();
        }
    }

    public class BattleStateBuilder
    {
        public BattleStatus Status { get; set; }

        public EntityStateBuilder Investigators { get; set; }
        public List<EntityStateBuilder> Enemies { get; set; }

        public List<ICard> Hand { get; set; }
        public List<ICard> DrawDeck { get; set; }
        public List<ICard> DiscardDeck { get; set; }
        public List<ICard> BurnedCardsDeck { get; set; }

        public int MaxResearchPoints { get; set; }
        public int RemainingResearchPoints { get; set; }

        public int MaxActionPoints { get; set; }
        public int RemainingActionpoints { get; set; }

        public int Draws { get; set; }
        public int MaxHandSize { get; set; }

        public BattleStateBuilder()
        {
        }

        public BattleStateBuilder(BattleState state)
        {
            Status = state.Status;
            Investigators = new EntityStateBuilder(state.Investigators);
            Enemies = state.Enemies.Select(item => new EntityStateBuilder(item)).ToList();

            Hand = state.Hand.ToList();
            DrawDeck = state.DrawDeck.ToList();
            DiscardDeck = state.DiscardDeck.ToList();
            BurnedCardsDeck = state.BurnedCardsDeck.ToList();

            MaxResearchPoints = state.MaxResearchPoints;
            RemainingResearchPoints = state.RemainingResearchPoints;
            MaxActionPoints = state.MaxActionPoints;
            RemainingActionpoints = state.RemainingActionPoints;
            Draws = state.Draws;
            MaxHandSize = state.MaxHandSize;
        }

        public EntityStateBuilder GetEnemy(EntityId id)
        {
            return Enemies.First(item => item.Id == id);
        }

        public BattleState ToState()
        {
            List<EntityState> enemies = Enemies.Select(item => item.ToState()).ToList();
            return new BattleState(Status,
                Investigators.ToState(),
                enemies,
                Hand,
                DrawDeck,
                DiscardDeck,
                BurnedCardsDeck,
                MaxResearchPoints,
                RemainingResearchPoints,
                MaxActionPoints,
                RemainingActionpoints,
                Draws,
                MaxHandSize);
        }

        internal void TryDrawCardToHand()
        {
            if (DrawDeck.Any())
            {
                ICard card = DrawDeck[0];
                DrawDeck.RemoveAt(0);
                Hand.Add(card);
            }
            else if (DiscardDeck.Any())
            {
                ShuffleDiscardsIntoDrawDeck();
                TryDrawCardToHand();
            }
        }

        private void ShuffleDiscardsIntoDrawDeck()
        {
            List<ICard> newDrawDeck = DrawDeck != null ? DrawDeck.ToList() : new List<ICard>();
            if (DiscardDeck != null)
            {
                newDrawDeck.AddRange(DiscardDeck);
                DiscardDeck.Clear();
            }
            List<ICard> shuffledCards = newDrawDeck.OrderBy(item => UnityEngine.Random.value).ToList(); // Todo: make this random value deterministic
            DrawDeck = shuffledCards;
        }
    }

    public class EntityState
    {
        public EntityId Id { get; }
        public IEnemyAction EnemyAction { get; }
        public int MaxHP { get; }
        public int CurrentHP { get; }
        public int Shield { get; }

        // List status effect states here

        public EntityState(EntityId id,
            IEnemyAction attack,
            int maxHP,
            int currentHP,
            int shield)
        {
            Id = id;
            EnemyAction = attack;
            MaxHP = maxHP;
            CurrentHP = currentHP;
            Shield = shield;
        }
    }

    public class EntityStateBuilder
    {
        public EntityId Id { get; set; }
        public IEnemyAction EnemyAction { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Shield { get; set; }

        public EntityStateBuilder(EntityId id)
        {
            Id = id;
        }
        public EntityStateBuilder(EntityState state)
        {
            Id = state.Id;
            EnemyAction = state.EnemyAction;
            MaxHP = state.MaxHP;
            CurrentHP = state.CurrentHP;
            Shield = state.Shield;
        }

        public EntityState ToState()
        {
            return new EntityState(
                Id,
                EnemyAction,
                MaxHP,
                CurrentHP,
                Shield);
        }

        public void ApplyAttackDamage(int damage)
        {
            int shieldedDamage = Mathf.Max(0, damage - Shield);
            CurrentHP -= damage;
        }
    }

    public interface IEnemyAction
    {
        BattleState TakeAction(BattleState state);
    }

    public interface ICard
    {
        string Name { get; }
        bool Consumeable { get; }
        bool Hold { get; }
        CardPlayability GetPlayability(BattleState state);
        BattleState Apply(BattleState state, EntityId potentialTarget);
    }

    public class EntityId { }

    public class CardPlayability
    {
        public bool IsPlayable { get; set; }
        public bool NeedsTarget { get; set; }
        public List<EntityId> PotentialTargets { get; set; }
    }

    public class BasicResearch : ICard
    {
        public string Name => "Basic Research";

        public int ActionPointCost => 1;
        public int ResearchPower => 5;

        public bool Consumeable => false;

        public bool Hold => false;

        public CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            ret.IsPlayable = state.RemainingActionPoints >= ActionPointCost;
            return ret;
        }

        public BattleState Apply(BattleState state, EntityId potentialTarget)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            builder.RemainingResearchPoints -= ResearchPower;
            return builder.ToState();
        }
    }

    public class BasicAttack : ICard
    {
        public string Name => "Basic Attack";

        public int ActionPointCost => 1;
        public int AttackPower => 5;

        public bool Consumeable => false;

        public bool Hold => false;

        public CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            bool hasActionPoints = state.RemainingActionPoints >= ActionPointCost;
            if (hasActionPoints)
            {
                var aliveEnemies = state.Enemies.Where(item => item.CurrentHP > 0).Select(item => item.Id);
                if (aliveEnemies.Any())
                {
                    ret.IsPlayable = true;
                    ret.NeedsTarget = true;
                    ret.PotentialTargets = aliveEnemies.ToList();
                }
            }
            return ret;
        }

        public BattleState Apply(BattleState state, EntityId target)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            EntityStateBuilder enemy = builder.GetEnemy(target);
            enemy.ApplyAttackDamage(AttackPower);
            return builder.ToState();
        }
    }

    public class BasicDefense : ICard
    {
        public string Name => "Basic Defense";

        public int ActionPointCost => 1;
        public int DefensePower => 5;

        public bool Consumeable => false;

        public bool Hold => false;

        public CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            ret.IsPlayable = state.RemainingActionPoints >= ActionPointCost;
            return ret;
        }

        public BattleState Apply(BattleState state, EntityId target)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            builder.Investigators.Shield += DefensePower;
            return builder.ToState();
        }
    }
}