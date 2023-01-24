using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace Combat.Model
{
    public class BattleState
    {
        public BattleStatus Status { get; }

        public EntityState Investigators { get; }
        public EntityState Enemy { get; }

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
            EntityState enemy,
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
            Enemy = enemy;
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

        public BattleState PlayCard(ICard cardToPlay)
        {
            BattleStateBuilder builder = new BattleStateBuilder(this);
            builder.Hand.Remove(cardToPlay);
            BattleState asState = builder.ToState();
            BattleState afterCard = cardToPlay.Apply(asState);
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
            currentState = currentState.Enemy.EnemyAction.TakeAction(currentState);
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
                if (card.Holds)
                    remainingHand.Add(card);
                else
                    builder.DiscardDeck.Add(card);
            }
            builder.Hand = remainingHand;
            return builder.ToState();
        }
    }
}