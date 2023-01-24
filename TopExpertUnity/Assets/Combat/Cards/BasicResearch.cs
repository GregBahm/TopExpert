using Combat.Model;

namespace Combat.Cards
{
    public class BasicResearch : CardTemplate
    {
        public override string Name => "Basic Research";

        public int ActionPointCost => 1;
        public int ResearchPower => 5;

        public override CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            ret.IsPlayable = state.RemainingActionPoints >= ActionPointCost;
            return ret;
        }

        public override BattleState Apply(BattleState state)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            builder.RemainingResearchPoints -= ResearchPower;
            builder.RemainingActionpoints -= ActionPointCost;
            return builder.ToState();
        }
    }
}