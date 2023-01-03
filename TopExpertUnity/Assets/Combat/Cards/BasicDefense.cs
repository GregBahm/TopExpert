using Combat.Model;

namespace Combat.Cards
{
    public class BasicDefense : CardTemplate
    {
        public override string Name => "Basic Defense";

        public int ActionPointCost => 1;
        public int DefensePower => 5;

        public override CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            ret.IsPlayable = state.RemainingActionPoints >= ActionPointCost;
            return ret;
        }

        public override BattleState Apply(BattleState state, EntityId target)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            builder.Investigators.Shield += DefensePower;
            builder.RemainingActionpoints -= ActionPointCost;
            return builder.ToState();
        }
    }
}