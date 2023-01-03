using Combat.Cards;
using Combat.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Behaviors
{
    public class CardVisualBindings : MonoBehaviour
    {
        [SerializeField]
        private Sprite BasicResearchArt;
        [SerializeField]
        private Sprite BasicAttackArt;
        [SerializeField]
        private Sprite BasicDefenseArt;

        private Dictionary<Type, Sprite> bindings;

        public static CardVisualBindings Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            bindings = new Dictionary<Type, Sprite>
            {
                { typeof(BasicResearch), BasicResearchArt },
                { typeof(BasicAttack), BasicAttackArt },
                { typeof(BasicDefense), BasicDefenseArt },
            };
        }

        public Sprite GetVisualFor(ICard card)
        {
            Type cardType = card.GetType();
            return bindings[cardType];
        }
    }
}