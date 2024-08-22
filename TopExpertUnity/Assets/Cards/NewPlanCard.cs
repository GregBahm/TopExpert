using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public record NewPlanCard : StandardPlayerCard
    {
        public override int ActionCost => 2;

        protected override EncounterState GetModifiedState(EncounterState state)
        {
            int handSize = state.Hand.Count;
            List<PlayerCard> hand = state.Hand.ToList();
            foreach (var card in hand)
            {
                state = state.GetWithCardDiscarded(card);
            }
            for (int i = 0; i < handSize; i++)
            {
                state = state.GetWithDraw();
            }
            return state;
        }
    }
}