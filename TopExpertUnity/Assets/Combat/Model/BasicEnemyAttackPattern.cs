using System.Linq;

namespace Combat.Model
{
    public class BasicEnemyAttackPattern : IEnemyAction
    {
        private bool attackTurn;

        private readonly int attack = 5;
        private readonly int shield = 5;

        public BasicEnemyAttackPattern(bool attackTurn = false)
        {
            this.attackTurn = attackTurn;
        }

        public BattleState TakeAction(BattleState state)
        {
            BattleStateBuilder builder = new BattleStateBuilder(state);
            if (attackTurn)
            {
                builder.Investigators.ApplyAttackDamage(attack);
            }
            else
            {
                builder.Enemy.Shield += shield;
            }
            builder.Enemy.EnemyAction = new BasicEnemyAttackPattern(!attackTurn);
            return builder.ToState();
        }
    }
}