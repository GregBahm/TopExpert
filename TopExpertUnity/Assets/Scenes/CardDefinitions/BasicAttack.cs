using System.Linq;

namespace TopExpert.Combat
{
    public class BasicAttack : ICard
    {
        public string Name => "Basic Attack";

        public int ActionPointCost => 1;
        public int AttackPower => 5;

        public bool Consumeable => false;

        public bool Hold => false;

        public CardPlayability GetPlayability(BattleState state)
        {
            CardPlayability ret = new CardPlayability();
            bool hasActionPoints = state.RemainingActionPoints >= ActionPointCost;
            if (hasActionPoints)
            {
                var aliveEnemies = state.Enemies.Where(item => item.CurrentHP > 0).Select(item => item.Id);
                if (aliveEnemies.Any())
                {
                    ret.IsPlayable = true;
                    ret.NeedsTarget = true;
                    ret.PotentialTargets = aliveEnemies.ToList();
                }
            }
            return ret;
        }

        public BattleState Apply(BattleState state, EntityId target)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            EntityStateBuilder enemy = builder.GetEnemy(target);
            enemy.ApplyAttackDamage(AttackPower);
            return builder.ToState();
        }
    }
}