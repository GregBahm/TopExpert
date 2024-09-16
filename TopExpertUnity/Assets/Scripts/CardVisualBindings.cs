using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Investigation.Behaviors
{
    [CreateAssetMenu(fileName = "New CardVisualBindings", menuName = "Bindings/CardVisualBindings")]
    public class CardVisualBindings : ScriptableObject
    {
        [SerializeField]
        private GameObject CarefulResearchCard;
        [SerializeField]
        private GameObject CommuneWithSpiritsCard;
        [SerializeField]
        private GameObject DaringGambitCard;
        [SerializeField]
        private GameObject DistractionCard;
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

        public void Initialize()
        {
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
                { typeof(DistractionCard), DistractionCard },
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
