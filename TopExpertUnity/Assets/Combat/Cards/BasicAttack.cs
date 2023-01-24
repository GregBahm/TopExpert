using Combat.Model;
using System.Linq;

namespace Combat.Cards
{
    public class BasicAttack : CardTemplate
    {
        public override string Name => "Basic Attack";

        public int ActionPointCost => 1;
        public int AttackPower => 5;

        public override CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            ret.IsPlayable = state.RemainingActionPoints >= ActionPointCost;
            return ret;
        }

        public override BattleState Apply(BattleState state)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            builder.Enemy.ApplyAttackDamage(AttackPower);
            builder.RemainingActionpoints -= ActionPointCost;
            return builder.ToState();
        }
    }
}