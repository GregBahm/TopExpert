using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public abstract record StandardDraftOption<T> : DraftOption
        where T : PlayerCard, new()
    {
        public abstract int DraftCost { get; }

        public override bool CanDraft(EncounterState state)
        {
            return state.Insights > DraftCost;
        }

        public override EncounterState DraftCard(EncounterState state)
        {
            List<PlayerCard> hand = state.Hand.ToList();
            hand.Add(new T());
            return state with { Hand = hand };
        }
    }
}