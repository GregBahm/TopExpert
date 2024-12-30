using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Investigation.Behaviors
{
    [CreateAssetMenu(fileName = "New CardVisualBindings", menuName = "Bindings/CardVisualBindings")]
    public class CardVisualBindings : VisualBindingsBase<PlayerCard>
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

        public override void Initialize()
        {
            AddBinding(typeof(CarefulResearchCard), CarefulResearchCard, "Careful research");
            AddBinding(typeof(CommuneWithSpiritsCard), CommuneWithSpiritsCard, "Commune with the sprits");
            AddBinding(typeof(DaringGambitCard), DaringGambitCard, "Daring gambit");
            AddBinding(typeof(GetAfterItCard), GetAfterItCard, "Get after it");
            AddBinding(typeof(HyperfocusCard), HyperfocusCard, "Hyperfocus");
            AddBinding(typeof(InvestigateCard), InvestigateCard, "Investigate");
            AddBinding(typeof(InvokeTheSpiritsCard), InvokeTheSpiritsCard, "Invoke the spirits");
            AddBinding(typeof(NewPlanCard), NewPlanCard, "New plan");
            AddBinding(typeof(DistractionCard), DistractionCard, "Distraction");
            AddBinding(typeof(OfCourseCard), OfCourseCard, "Of course");
            AddBinding(typeof(OverthinkerCard), OverthinkerCard, "Overthinker");
            AddBinding(typeof(PunchItCard), PunchItCard, "Punch it");
            AddBinding(typeof(SpringTheTrapCard), SpringTheTrapCard, "Spring the trap");
            AddBinding(typeof(UnravelTheMysteryCard), UnravelTheMysteryCard, "Unravel the mystery");
        }
    }
}
