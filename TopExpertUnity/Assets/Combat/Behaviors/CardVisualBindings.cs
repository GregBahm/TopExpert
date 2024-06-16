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
        private Sprite CarefulResearch;

        private Dictionary<Type, Sprite> bindings;

        public static CardVisualBindings Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            bindings = new Dictionary<Type, Sprite>
            {
                { typeof(CarefulResearch), CarefulResearch },
            };
        }

        public Sprite GetVisualFor(ICard card)
        {
            Type cardType = card.GetType();
            return bindings[cardType];
        }
    }
}