using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public record NoWay : PlayerCard
    {
        public int ActionCost => 3;
        public int InsightCost => 5;

        public override bool CanPlay(EncounterState state)
        {
            bool actions = state.Actions >= ActionCost;
            bool insights = state.Insights >= InsightCost;
            return actions && insights;
        }

        public override EncounterState Play(EncounterState state)
        {
            state = GetWithCardMovedFromHand(state);
            state = state with { Actions = state.Actions - ActionCost , Insights = state.Insights - InsightCost};
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            List<PersistantEffector> appliedEffectors = state.AppliedEffectors.ToList();
            
            foreach (var item in state.UnappliedEffectors.Where(item => item.IsEnemyEffect))
            {
                unappliedEffectors.Remove(item);
                appliedEffectors.Add(item);
            }
            return state with { UnappliedEffectors = unappliedEffectors, AppliedEffectors = appliedEffectors };
        }
    }
}