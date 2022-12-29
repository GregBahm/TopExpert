using UnityEngine;

namespace TopExpert.Combat
{
    public class EntityStateBuilder
    {
        public EntityId Id { get; set; }
        public IEnemyAction EnemyAction { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Shield { get; set; }

        public EntityStateBuilder(EntityId id)
        {
            Id = id;
        }
        public EntityStateBuilder(EntityState state)
        {
            Id = state.Id;
            EnemyAction = state.EnemyAction;
            MaxHP = state.MaxHP;
            CurrentHP = state.CurrentHP;
            Shield = state.Shield;
        }

        public EntityState ToState()
        {
            return new EntityState(
                Id,
                EnemyAction,
                MaxHP,
                CurrentHP,
                Shield);
        }

        public void ApplyAttackDamage(int damage)
        {
            int shieldedDamage = Mathf.Max(0, damage - Shield);
            CurrentHP -= damage;
        }
    }
}