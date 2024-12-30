using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record EncounterState
    {
        public EncounterStatus Status { get; init; }

        public int Insights { get; init; }
        public int Actions { get; init; }
        public int ActionsPerTurn { get; init; }
        public int Advantage { get; init; }
        public int AdvantageToWin { get; init; }
        public int AdvantageToLose { get; init; }

        public EncounterPhase Phase { get; init; }

        public int DangerPhaseInsightsCost { get; init; }

        public IReadOnlyList<PersistantEffector> UnappliedEffectors { get; init; }
        public IReadOnlyList<PersistantEffector> AppliedEffectors { get; init; }

        public IReadOnlyList<PlayerCard> Hand { get; init; }
        public IReadOnlyList<PlayerCard> DrawDeck { get; init; }
        public IReadOnlyList<PlayerCard> DiscardDeck { get; init; }
        public IReadOnlyList<PlayerCard> DissolveDeck { get; init; }

        public int Draws { get; init; }
        public int MaxHandSize { get; init; }

        public IReadOnlyList<DraftOption> DraftDeck { get; init; }
        public IReadOnlyList<DraftOption> DraftOptions { get; init; }
        public int AvailableDrafts { get; init; }

        public int SpiritsPower { get; init; }

        public IEnumerable<PlayerCard> AllCards
        {
            get
            {
                foreach (var card in Hand)
                {
                    yield return card;
                }
                foreach (var card in DrawDeck)
                {
                    yield return card;
                }
                foreach (var card in DiscardDeck)
                {
                    yield return card;
                }
                foreach (var card in DissolveDeck)
                {
                    yield return card;
                }
            }
        }

        public EncounterState GetWithDraw()
        {
            return GetWithDraw(this);
        }

        private static EncounterState GetWithDraw(EncounterState state)
        {
            if (state.DrawDeck.Any())
            {
                List<PlayerCard> newDrawDeck = state.DrawDeck.ToList();
                PlayerCard card = newDrawDeck.Last();
                newDrawDeck.RemoveAt(state.DrawDeck.Count - 1);

                List<PlayerCard> newHand = state.Hand.ToList();
                newHand.Add(card);

                state = state with { Hand = newHand, DrawDeck = newDrawDeck };
            }
            else
            {
                if (state.DiscardDeck.Any())
                {
                    state = ShuffleDiscardsIntoDraw(state);
                    state = GetWithDraw(state);
                }
            }
            return state;
        }

        private static EncounterState ShuffleDiscardsIntoDraw(EncounterState state)
        {
            List<PlayerCard> oldDiscardDeck = state.DiscardDeck.ToList();
            List<PlayerCard> newDrawDeck = new List<PlayerCard>();
            int originalCount = oldDiscardDeck.Count;
            for (int i = 0; i < originalCount; i++)
            {
                int nextCard = UnityEngine.Random.Range(0, oldDiscardDeck.Count);
                PlayerCard card = oldDiscardDeck[nextCard];
                oldDiscardDeck.RemoveAt(nextCard);
                newDrawDeck.Add(card);
            }

            return state with { DiscardDeck = new List<PlayerCard>(), DrawDeck = newDrawDeck };
        }

        public EncounterState GetWithCardDiscarded(PlayerCard card)
        {
            List<PlayerCard> hand = Hand.ToList();
            List<PlayerCard> dissolveDeck = DissolveDeck.ToList();
            List<PlayerCard> discardDeck = DiscardDeck.ToList();

            hand.Remove(card);
            if (card.DissolvesIfNotPlayed)
                dissolveDeck.Add(card);
            else
                discardDeck.Add(card);

            return this with { Hand = hand, DissolveDeck = dissolveDeck, DiscardDeck = discardDeck };
        }
    }
}