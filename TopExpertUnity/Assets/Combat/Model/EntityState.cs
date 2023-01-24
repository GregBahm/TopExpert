namespace Combat.Model
{
    public class EntityState
    {
        public IEnemyAction EnemyAction { get; }
        public int MaxHP { get; }
        public int CurrentHP { get; }
        public int Shield { get; }

        // List status effect states here

        public EntityState(IEnemyAction attack,
            int maxHP,
            int currentHP,
            int shield)
        {
            EnemyAction = attack;
            MaxHP = maxHP;
            CurrentHP = currentHP;
            Shield = shield;
        }
    }
}