using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record ChannelTheOtherSideDraftOption(ElementIdentifier Identifier) 
        : DraftOption(Identifier)
    {
        public int DraftCost => 5;

        public override bool CanDraft(EncounterState state)
        {
            return state.Insights > DraftCost;
        }

        public override EncounterState DraftCard(EncounterState state)
        {
            List<PlayerCard> hand = state.Hand.ToList();
            hand.Add(new CommuneWithSpiritsCard(new ElementIdentifier()));
            hand.Add(new CommuneWithSpiritsCard(new ElementIdentifier()));
            hand.Add(new CommuneWithSpiritsCard(new ElementIdentifier()));
            hand.Add(new InvokeTheSpiritsCard(new ElementIdentifier()));
            return state with { Hand = hand };
        }
    }
}