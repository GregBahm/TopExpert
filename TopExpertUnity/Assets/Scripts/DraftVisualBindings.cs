using Investigation.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    [CreateAssetMenu(fileName = "New DraftVisualBindings", menuName = "Bindings/DraftVisualBindings")]
    public class DraftVisualBindings : ScriptableObject
    {
        [SerializeField]
        private GameObject ChannelTheOtherSideDraftOption;
        [SerializeField]
        private GameObject DaringGambitDraftOption;

        private Dictionary<Type, GameObject> bindings;

        public void Initialize()
        {
            bindings = new Dictionary<Type, GameObject>
            {
                { typeof(ChannelTheOtherSideDraftOption), ChannelTheOtherSideDraftOption },
                { typeof(DaringGambitDraftOption), DaringGambitDraftOption }
            };
        }

        public GameObject GetPrefabFor(DraftOption draftOption)
        {
            Type cardType = draftOption.GetType();
            return bindings[cardType];
        }
    }
}