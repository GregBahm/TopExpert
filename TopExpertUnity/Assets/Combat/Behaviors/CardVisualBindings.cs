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
        [SerializeField]
        private Sprite CloserLook;
        [SerializeField]
        private Sprite Compartmentalize;
        [SerializeField]
        private Sprite Concentrate;
        [SerializeField]
        private Sprite Consideration;
        [SerializeField]
        private Sprite FigureItOut;
        [SerializeField]
        private Sprite HumanTestSubject;
        [SerializeField]
        private Sprite KeepInMind;
        [SerializeField]
        private Sprite Lookout;
        [SerializeField]
        private Sprite Overthink;
        [SerializeField]
        private Sprite Precaution;
        [SerializeField]
        private Sprite SimplePlan;
        [SerializeField]
        private Sprite ThinkFast;
        [SerializeField]
        private Sprite Violence;

        private Dictionary<Type, Sprite> bindings;

        public static CardVisualBindings Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            bindings = new Dictionary<Type, Sprite>
            {
                { typeof(CarefulResearch), CarefulResearch },
                { typeof(CloserLook), CloserLook },
                { typeof(Compartmentalize), Compartmentalize },
                { typeof(Concentrate), Concentrate },
                { typeof(Consideration), Consideration },
                { typeof(FigureItOut), FigureItOut },
                { typeof(HumanTestSubject), HumanTestSubject },
                { typeof(KeepInMind), KeepInMind },
                { typeof(Lookout), Lookout },
                { typeof(Overthink), Overthink },
                { typeof(Precaution), Precaution },
                { typeof(SimplePlan), SimplePlan },
                { typeof(ThinkFast), ThinkFast },
                { typeof(Violence), Violence },
            };
        }

        public Sprite GetVisualFor(ICard card)
        {
            Type cardType = card.GetType();
            return bindings[cardType];
        }

        public IEnumerable<Type> GetAllCardTypes()
        {
            return bindings.Keys;
        }
    }
}