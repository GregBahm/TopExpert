namespace TopExpert.Combat
{
    public class EntityState
    {
        public EntityId Id { get; }
        public IEnemyAction EnemyAction { get; }
        public int MaxHP { get; }
        public int CurrentHP { get; }
        public int Shield { get; }

        // List status effect states here

        public EntityState(EntityId id,
            IEnemyAction attack,
            int maxHP,
            int currentHP,
            int shield)
        {
            Id = id;
            EnemyAction = attack;
            MaxHP = maxHP;
            CurrentHP = currentHP;
            Shield = shield;
        }
    }
}