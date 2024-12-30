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
        private GameObject ChannelTheOtherSideDraftOption;
        [SerializeField]
        private GameObject DaringGambitDraftOption;

        public override void Initialize()
        {
            AddBinding(typeof(ChannelTheOtherSideDraftOption), ChannelTheOtherSideDraftOption, "Channel the other side");
            AddBinding(typeof(DaringGambitDraftOption), DaringGambitDraftOption, "Daring gambit");
        }
    }
}