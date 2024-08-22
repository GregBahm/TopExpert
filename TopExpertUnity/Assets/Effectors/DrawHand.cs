using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public record DrawHand : PersistantEffector
    {
        protected override EncounterState GetEffectedState(EncounterState state)
        {

            List<PlayerCard> oldHand = state.Hand.ToList();
            List<PlayerCard> dissolvedCards = state.DissolveDeck.ToList();
            List<PlayerCard> discardedCards = state.DiscardDeck.ToList();

            List<PlayerCard> newHand = new List<PlayerCard>();

            foreach (PlayerCard card in oldHand)
            {
                if (card.Persists)
                {
                    newHand.Add(card);
                }
                else if (card.DissolvesIfNotPlayed)
                {
                    dissolvedCards.Add(card);
                }
                else
                {
                    discardedCards.Add(card);
                }
            }
            state = state with { Hand = newHand, DissolveDeck = dissolvedCards, DiscardDeck = discardedCards };
            for (int i = 0; i < state.Draws; i++)
            {
                state = state.GetWithDraw();
            }
            return state;
        }
    }
}