namespace Encounter.Model
{

    public class NoWay : PlayerCard
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
            state.UnappliedEffectors
            return state;
        }
    }
}