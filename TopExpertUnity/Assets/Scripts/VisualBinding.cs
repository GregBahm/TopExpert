using Investigation.Behaviors;
using Investigation.Model;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class VisualBinding : MonoBehaviour
    {
        [SerializeField]
        private CardVisualBindings cardBindings;
        [SerializeField]
        private DraftVisualBindings draftBindings;
        [SerializeField]
        private EffectorVisualBindings effectorBindings;

        public static VisualBinding Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            cardBindings.Initialize();
            draftBindings.Initialize();
            effectorBindings.Initialize();
        }

        public GameObject GetPrefabFor(PlayerCard card)
        {
            return cardBindings.GetPrefabFor(card);
        }
        public GameObject GetPrefabFor(DraftOption draftOption)
        {
            return draftBindings.GetPrefabFor(draftOption);
        }
        public GameObject GetPrefabFor(PersistantEffector effector)
        {
            return effectorBindings.GetPrefabFor(effector);
        }
    }
}