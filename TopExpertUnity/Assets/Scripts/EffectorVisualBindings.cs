using Investigation.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    [CreateAssetMenu(fileName = "New EffectorVisualBindings", menuName = "Bindings/EffectorVisualBindings")]
    public class EffectorVisualBindings : ScriptableObject
    {
        [SerializeField]
        private GameObject HauntedCoffeeMachine;
        [SerializeField]
        private GameObject HyperfocusEffector;

        private Dictionary<Type, GameObject> bindings;

        public void Initialize()
        {
            bindings = new Dictionary<Type, GameObject>
            {
                { typeof(HauntedCoffeeMachine), HauntedCoffeeMachine },
                { typeof(HyperfocusEffector), HyperfocusEffector }
            };
        }

        public GameObject GetPrefabFor(PersistantEffector effector)
        {
            Type cardType = effector.GetType();
            return bindings[cardType];
        }

        public bool HasVisuals(PersistantEffector effector)
        {
            return bindings.ContainsKey(effector.GetType());
        }
    }
}