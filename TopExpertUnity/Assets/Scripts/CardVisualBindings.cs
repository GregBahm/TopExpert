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
        private GameObject ChannelTheOtherSideDraftOption;
        [SerializeField]
        private GameObject CommuneWithSpiritsCard;
        [SerializeField]
        private GameObject DaringGambitCard;
        [SerializeField]
        private GameObject DaringGambitDraftOption;
        [SerializeField]
        private GameObject GetAfterItCard;
        [SerializeField]
        private GameObject HyperfocusCard;
        [SerializeField]
        private GameObject InvestigateCard;
        [SerializeField]
        private GameObject InvokeTheSpiritsCard;
        [SerializeField]
        private GameObject NewPlanCard;
        [SerializeField]
        private GameObject NoWayCard;
        [SerializeField]
        private GameObject OfCourseCard;
        [SerializeField]
        private GameObject OverthinkerCard;
        [SerializeField]
        private GameObject PunchItCard;
        [SerializeField]
        private GameObject SpringTheTrapCard;
        [SerializeField]
        private GameObject UnravelTheMysteryCard;

        private Dictionary<Type, GameObject> bindings;

        public static CardVisualBindings Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            bindings = new Dictionary<Type, GameObject>
            {
                { typeof(CarefulResearchCard), CarefulResearchCard },
                { typeof(CommuneWithSpiritsCard), CommuneWithSpiritsCard },
                { typeof(DaringGambitCard), DaringGambitCard },
                { typeof(GetAfterItCard), GetAfterItCard },
                { typeof(HyperfocusCard), HyperfocusCard },
                { typeof(InvestigateCard), InvestigateCard },
                { typeof(InvokeTheSpiritsCard), InvokeTheSpiritsCard },
                { typeof(NewPlanCard), NewPlanCard },
                { typeof(NoWayCard), NoWayCard },
                { typeof(OfCourseCard), OfCourseCard },
                { typeof(OverthinkerCard), OverthinkerCard },
                { typeof(PunchItCard), PunchItCard },
                { typeof(SpringTheTrapCard), SpringTheTrapCard },
                { typeof(UnravelTheMysteryCard), UnravelTheMysteryCard },
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