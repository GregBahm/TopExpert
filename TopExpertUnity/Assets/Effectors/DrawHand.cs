using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public class DrawHand : PersistantEffector
    {
        protected override EncounterState ModifyState(EncounterState state)
        {

            List<PlayerCard> oldHand = state.Hand.ToList();
            List<PlayerCard> dissolvedCards = state.DissolvedCardsDeck.ToList();
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
            state = state with { Hand = newHand, DissolvedCardsDeck = dissolvedCards, DiscardDeck = discardedCards };
            for (int i = 0; i < state.Draws; i++)
            {
                state = state.GetWithDraw();
            }
            return state;
        }
    }
}