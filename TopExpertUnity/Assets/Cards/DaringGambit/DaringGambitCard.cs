using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record DaringGambitCard() 
        : StandardPlayerCard()
    {
        public override int ActionCost => 0;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            List<PlayerCard> hand = state.Hand.ToList();
            List<PlayerCard> dissolveDeck = state.DissolveDeck.ToList();
            if (hand.Count > 0)
            {
                PlayerCard lastCard = hand.Last();
                hand.Remove(lastCard);
                dissolveDeck.Add(lastCard);
                if(hand.Count > 1)
                {
                    PlayerCard firstCard = hand.First();
                    hand.Remove(firstCard);
                    dissolveDeck.Add(firstCard);
                }
            }
            return state with { Advantage = state.Advantage + 3, Hand = hand, DissolveDeck = dissolveDeck };
        }
    }
}