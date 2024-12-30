using Investigation.Model;
using UnityEngine;

namespace Investigation.Behaviors
{
    [CreateAssetMenu(fileName = "New EffectorVisualBindings", menuName = "Bindings/EffectorVisualBindings")]
    public class EffectorVisualBindings : VisualBindingsBase<PersistantEffector>
    {
        [SerializeField]
        private GameObject DrawHand;
        [SerializeField]
        private GameObject RestoreEnergy;
        [SerializeField]
        private GameObject HauntedCoffeeMachine;
        [SerializeField]
        private GameObject HyperfocusEffector;

        public override void Initialize()
        {
            AddBinding(typeof(HauntedCoffeeMachine), HauntedCoffeeMachine, "Haunted coffee machine");
            AddBinding(typeof(HyperfocusEffector), HyperfocusEffector, "Hyperfocus");
            AddBinding(typeof(DrawHand), DrawHand, "Draw hand");
            AddBinding(typeof(RestoreEnergy), RestoreEnergy, "Restore energy");
        }
    }
}