namespace Combat.Model
{
    public class EntityStateBuilder
    {
        public IEnemyAction EnemyAction { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Shield { get; set; }

        public EntityStateBuilder()
        {
        }
        public EntityStateBuilder(EntityState state)
        {
            EnemyAction = state.EnemyAction;
            MaxHP = state.MaxHP;
            CurrentHP = state.CurrentHP;
            Shield = state.Shield;
        }

        public EntityState ToState()
        {
            return new EntityState(
                EnemyAction,
                MaxHP,
                CurrentHP,
                Shield);
        }

        public void ApplyAttackDamage(int damage)
        {
            int shieldedDamage = UnityEngine.Mathf.Max(0, damage - Shield);
            CurrentHP -= shieldedDamage;
        }
    }
}