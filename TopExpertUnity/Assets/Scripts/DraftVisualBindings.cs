using Investigation.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    [CreateAssetMenu(fileName = "New DraftVisualBindings", menuName = "Bindings/DraftVisualBindings")]
    public class DraftVisualBindings : VisualBindingsBase<DraftOption>
    {
        [SerializeField]
        private GameObject ChannelTheOtherSide;
        [SerializeField]
        private GameObject DaringGambit;
        [SerializeField]
        private GameObject OfCourse;
        [SerializeField]
        private GameObject PunchIt;
        [SerializeField]
        private GameObject SpringTheTrap;
        [SerializeField]
        private GameObject UnravelTheMystery;

        public override void Initialize()
        {
            AddBinding(typeof(ChannelTheOtherSideDraftOption), ChannelTheOtherSide, "Channel the other side");
            AddBinding(typeof(DaringGambitDraftOption), DaringGambit, "Daring gambit");
            AddBinding(typeof(OfCourseDraftOption), OfCourse, "Of course");
            AddBinding(typeof(PunchItDraftOption), PunchIt, "Punch it");
            AddBinding(typeof(SpringTheTrapDraftOption), SpringTheTrap, "Spring the trap");
            AddBinding(typeof(UnravelTheMysteryDraftOption), UnravelTheMystery, "Unravel the mystery");
        }
    }
}