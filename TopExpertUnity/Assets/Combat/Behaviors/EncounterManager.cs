using Combat.Cards;
using Combat.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.ObjectChangeEventStream;
using static UnityEditor.Progress;

namespace Combat.Behaviors
{
    public class EncounterManager : MonoBehaviour
    {
        public static EncounterManager Instance { get; private set; }

        public Encounter Encounter { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Encounter = GetTestEncounter();
        }

        private Encounter GetTestEncounter()
        {
            BattleStateBuilder builder = new BattleStateBuilder();
            builder.MaxActionPoints = 4;
            builder.MaxHandSize = 7;

            EntityStateBuilder investigators = new EntityStateBuilder(new EntityId());
            investigators.MaxHP = 100;
            investigators.CurrentHP = 100;

            EntityId enemyIdA = new EntityId();
            EntityId enemyIdB = new EntityId();

            BasicEnemyAttackPattern enemyAAttack = new BasicEnemyAttackPattern(enemyIdA);
            BasicEnemyAttackPattern enemyBAttack = new BasicEnemyAttackPattern(enemyIdB);

            EntityStateBuilder firstEnemy = new EntityStateBuilder(enemyIdA);
            firstEnemy.MaxHP = 50;
            firstEnemy.CurrentHP = 50;
            firstEnemy.EnemyAction = enemyAAttack;
            EntityStateBuilder secondEnemy = new EntityStateBuilder(enemyIdB);
            secondEnemy.MaxHP = 50;
            secondEnemy.CurrentHP = 50;
            secondEnemy.EnemyAction = enemyBAttack;

            builder.Investigators = investigators;
            builder.Enemies = new List<EntityStateBuilder> { firstEnemy, secondEnemy };

            builder.DrawDeck = GetTestDeck().OrderBy(item => UnityEngine.Random.value).ToList();
            BattleState initialState = builder.ToState().StartNewRound();

            return new Encounter(initialState);
        }

        private IEnumerable<ICard> GetTestDeck()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new BasicDefense();
            }
            for (int i = 0; i < 10; i++)
            {
                yield return new BasicAttack();
            }
            for (int i = 0; i < 10; i++)
            {
                yield return new BasicResearch();
            }
        }
    }
}