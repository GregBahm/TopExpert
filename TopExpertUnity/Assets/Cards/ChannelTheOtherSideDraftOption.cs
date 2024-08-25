using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record ChannelTheOtherSideDraftOption(DraftOptionIdentifier Identifier) 
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
            hand.Add(new CommuneWithSpiritsCard(new CardIdentifier()));
            hand.Add(new CommuneWithSpiritsCard(new CardIdentifier()));
            hand.Add(new CommuneWithSpiritsCard(new CardIdentifier()));
            hand.Add(new InvokeTheSpiritsCard(new CardIdentifier()));
            return state with { Hand = hand };
        }
    }
}