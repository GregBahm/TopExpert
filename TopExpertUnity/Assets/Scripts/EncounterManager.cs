using Combat.Cards;
using Combat.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
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
            builder.MaxHandSize = 7;

            EntityStateBuilder investigators = new EntityStateBuilder();
            investigators.MaxHP = 100;
            investigators.CurrentHP = 100;

            EntityStateBuilder enemy = new EntityStateBuilder();
            enemy.MaxHP = 50;
            enemy.CurrentHP = 50;
            enemy.EnemyAction = new BasicEnemyAttackPattern();

            builder.Investigators = investigators;
            builder.Enemy = enemy;

            builder.DrawDeck = GetTestDeck().OrderBy(item => UnityEngine.Random.value).ToList();
            BattleState initialState = builder.ToState().StartNewRound();

            return new Encounter(initialState);
        }

        private IEnumerable<ICard> GetTestDeck()
        {
            IEnumerable<Type> cardTypes = CardVisualBindings.Instance.GetAllCardTypes();
            foreach (Type cardType in cardTypes)
            {
                yield return (ICard)Activator.CreateInstance(cardType);
            }
        }
    }
}