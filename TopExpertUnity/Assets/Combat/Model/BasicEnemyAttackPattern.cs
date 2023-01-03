using System.Linq;

namespace Combat.Model
{
    public class BasicEnemyAttackPattern : IEnemyAction
    {
        private readonly EntityId selfId;
        private bool attackTurn;

        private readonly int attack = 5;
        private readonly int shield = 5;

        public BasicEnemyAttackPattern(EntityId selfId, bool attackTurn = false)
        {
            this.selfId = selfId;
            this.attackTurn = attackTurn;
        }

        public BattleState TakeAction(BattleState state)
        {
            EntityState selfState = state.Enemies.First(item => item.Id == selfId);
            if (selfState.CurrentHP <= 0)
            {
                return state; // This enemy is dead
            }

            BattleStateBuilder builder = new BattleStateBuilder(state);
            EntityStateBuilder selfBuilder = builder.GetEnemy(selfId);
            if (attackTurn)
            {
                builder.Investigators.ApplyAttackDamage(attack);
            }
            else
            {
                selfBuilder.Shield += shield;
            }
            selfBuilder.EnemyAction = new BasicEnemyAttackPattern(selfId, !attackTurn);
            return builder.ToState();
        }
    }
}