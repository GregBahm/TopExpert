
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Investigation.Model
{
    public record OverthinkerCard(ElementIdentifier Identifier) 
        : StandardPlayerCard(Identifier)
    {
        public override int ActionCost => 0;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            List<PlayerCard> discard = state.DiscardDeck.ToList();
            OverthinkerCard newOverthinker = new OverthinkerCard(new ElementIdentifier());
            discard.Insert(discard.Count - 1, newOverthinker);

            int count = GetNumberOfOverthinkerCards(state) + 1;

            return state with { Insights = state.Insights + count, DiscardDeck = discard };
        }

        private int GetNumberOfOverthinkerCards(EncounterState state)
        {
            int count = 0;

            foreach (var card in state.Hand
                .Concat(state.DrawDeck)
                .Concat(state.DiscardDeck))
            {
                if (card.GetType() == typeof(OverthinkerCard))
                {
                    count++;
                }
            }
            return count;
        }
    }
}