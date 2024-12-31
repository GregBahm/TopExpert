using Investigation.Behaviors;
using Investigation.Model;
using System;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class VisualBinding : MonoBehaviour
    {
        [SerializeField]
        private CardVisualBindings cardBindings;

        [SerializeField]
        private EffectorVisualBindings effectorBindings;

        public static VisualBinding Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            cardBindings.Initialize();
            effectorBindings.Initialize();
        }

        public GameObject GetPrefabFor(PlayerCard card)
        {
            return cardBindings.GetPrefabFor(card);
        }

        public GameObject GetPrefabFor(PersistantEffector effector)
        {
            return effectorBindings.GetPrefabFor(effector);
        }

        public string GetTimelineAnnotationFor(EncounterStep step)
        {
            if (step.Modifier == null)
                return "New Turn";

            Type modifierType = step.Modifier.GetType();
            if (modifierType.IsSubclassOf(typeof(PlayerCard)))
            {
                return cardBindings.GetTimelineAnnotationFor(modifierType);
            }
            if (modifierType.IsSubclassOf(typeof(PersistantEffector)))
            {
                return effectorBindings.GetTimelineAnnotationFor(modifierType);
            }
            throw new Exception("No timeline annotation found for " + modifierType);
        }
    }
}