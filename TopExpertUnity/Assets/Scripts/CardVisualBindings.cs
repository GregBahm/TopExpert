using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class CardVisualBindings : MonoBehaviour
    {
        [SerializeField]
        private GameObject CarefulResearchCard;
        [SerializeField]
        private GameObject OverthinkerCard;
        [SerializeField]
        private GameObject HyperfocusCard;
        [SerializeField]
        private GameObject InvestigateCard;
        [SerializeField]
        private GameObject NewPlanCard;

        private Dictionary<Type, GameObject> bindings;

        public static CardVisualBindings Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            bindings = new Dictionary<Type, GameObject>
            {
                { typeof(CarefulResearchCard), CarefulResearchCard },
                { typeof(OverthinkerCard), OverthinkerCard },
                { typeof(HyperfocusCard), HyperfocusCard },
                { typeof(InvestigateCard), InvestigateCard },
                { typeof(NewPlanCard), NewPlanCard },
            };
        }

        public GameObject GetPrefabFor(PlayerCard card)
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