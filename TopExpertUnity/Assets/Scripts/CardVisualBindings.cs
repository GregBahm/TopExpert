using Encounter.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter.Behaviors
{
    public class CardVisualBindings : MonoBehaviour
    {
        [SerializeField]
        private CardVisuals CarefulResearchCard;
        [SerializeField]
        private CardVisuals OverthinkerCard;
        [SerializeField]
        private CardVisuals HyperfocusCard;
        [SerializeField]
        private CardVisuals InvestigateCard;
        [SerializeField]
        private CardVisuals NewPlanCard;

        private Dictionary<Type, CardVisuals> bindings;

        public static CardVisualBindings Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            bindings = new Dictionary<Type, CardVisuals>
            {
                { typeof(CarefulResearchCard), CarefulResearchCard },
                { typeof(OverthinkerCard), OverthinkerCard },
                { typeof(HyperfocusCard), HyperfocusCard },
                { typeof(InvestigateCard), InvestigateCard },
                { typeof(NewPlanCard), NewPlanCard },
            };
        }

        public CardVisuals GetVisualsFor(PlayerCard card)
        {
            Type cardType = card.GetType();
            return bindings[cardType];
        }

        public IEnumerable<Type> GetAllCardTypes()
        {
            return bindings.Keys;
        }
    }

    [Serializable]
    public class CardVisuals
    {
        public string Name;
        public Sprite Image;
    }

}