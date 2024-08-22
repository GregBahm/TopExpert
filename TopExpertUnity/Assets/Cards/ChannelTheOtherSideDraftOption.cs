using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public abstract record ChannelTheOtherSideDraftOption : DraftOption
    {
        public int DraftCost => 5;

        public override bool CanDraft(EncounterState state)
        {
            return state.Insights > DraftCost;
        }

        public override EncounterState DraftCard(EncounterState state)
        {
            List<PlayerCard> hand = state.Hand.ToList();
            hand.Add(new CommuneWithSpiritsCard());
            hand.Add(new CommuneWithSpiritsCard());
            hand.Add(new CommuneWithSpiritsCard());
            hand.Add(new InvokeTheSpiritsCard());
            return state with { Hand = hand };
        }
    }
}