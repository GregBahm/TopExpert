using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEditor.Timeline;
using UnityEngine.UIElements.Experimental;

namespace Encounter.Model
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

        public EncounterState GetWithDraw()
        {
            EncounterState state = this;
            if (DrawDeck.Any())
            {
                List<PlayerCard> newDrawDeck = DrawDeck.ToList();
                PlayerCard card = newDrawDeck.Last();
                newDrawDeck.RemoveAt(DrawDeck.Count - 1);

                List<PlayerCard> newHand = Hand.ToList();
                newHand.Add(card);

                state = state with { Hand = newHand, DrawDeck = newDrawDeck };
            }
            else
            {
                if (DiscardDeck.Any())
                {
                    state = ShuffleDiscardsIntoDraw();
                    state = GetWithDraw();
                }
            }
            return state;
        }

        private EncounterState ShuffleDiscardsIntoDraw()
        {
            List<PlayerCard> oldDiscardDeck = DiscardDeck.ToList();
            List<PlayerCard> newDrawDeck = new List<PlayerCard>();

            for (int i = 0; i < oldDiscardDeck.Count; i++)
            {
                int nextCard = UnityEngine.Random.Range(0, oldDiscardDeck.Count - 1);
                PlayerCard card = oldDiscardDeck[nextCard];
                oldDiscardDeck.RemoveAt(nextCard);
                newDrawDeck.Add(card);
            }

            return this with { DiscardDeck = new List<PlayerCard>(), DrawDeck = newDrawDeck };
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