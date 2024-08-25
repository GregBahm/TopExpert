using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public abstract record PlayerCard(CardIdentifier Identifier) : IStateModifier
    {
        public virtual bool DissolvesOnPlay { get; }
        public virtual bool DissolvesIfNotPlayed { get; }
        public virtual bool Persists { get; }

        public abstract EncounterState Play(EncounterState state);
        public abstract bool CanPlay(EncounterState state);

        protected EncounterState GetWithCardMovedFromHand(EncounterState state)
        {
            List<PlayerCard> hand = state.Hand.ToList();
            List<PlayerCard> dissolveDeck = state.DissolveDeck.ToList();
            List<PlayerCard> discardDeck = state.DiscardDeck.ToList();
            hand.Remove(this);
            if (DissolvesOnPlay)
                dissolveDeck.Add(this);
            else
                discardDeck.Add(this);

            return state with { Hand = hand, DissolveDeck = dissolveDeck, DiscardDeck = discardDeck };
        }
    }

    public class CardIdentifier { }
}