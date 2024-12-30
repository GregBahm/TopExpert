using Investigation.Model;
using UnityEngine;


namespace Investigation.Behaviors
{
    public abstract class ElementVisualController<T> : MonoBehaviour
    {
        protected T state;

        protected ElementIdentifier identifier;

        public void Initialize(ElementIdentifier identifier)
        {
            this.identifier = identifier;
        }

        public void SetDrawState(T state)
        {
            this.state = state;
        }

        protected static EncounterVisualsManager Mothership
        {
            get
            {
                return EncounterVisualsManager.Instance;
            }
        }
    }
}