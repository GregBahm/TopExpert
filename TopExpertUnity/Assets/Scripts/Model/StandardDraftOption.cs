using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public abstract record StandardDraftOption : DraftOption
    {
        public abstract int DraftCost { get; }
        public abstract PlayerCard Card { get; }

        public override bool CanDraft(EncounterState state)
        {
            return state.Insights > DraftCost;
        }

        public override EncounterState DraftCard(EncounterState state)
        {
            List<PlayerCard> hand = state.Hand.ToList();
            hand.Add(Card);
            return state with { Hand = hand };
        }
    }
}