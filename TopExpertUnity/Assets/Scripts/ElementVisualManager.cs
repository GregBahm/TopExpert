using Investigation.Model;
using System.Collections.Generic;
using UnityEngine;


namespace Investigation.Behaviors
{
    public abstract class ElementVisualManager<TModel, TVisualController, TUIState>
        where TUIState : ElementUIState<TModel>
        where TVisualController : ElementVisualController<TUIState>
    {
        private Dictionary<ElementIdentifier, TVisualController> effectors = new Dictionary<ElementIdentifier, TVisualController>();

        protected abstract TVisualController InstantiateController(TModel model);

        public void VisualizeEncounter(EncounterState previousState, EncounterState nextState)
        {
            Dictionary<ElementIdentifier, TUIState> effectorStates = GetEffectorStates(previousState, nextState);
            foreach (var item in effectorStates)
            {
                if (!effectors.ContainsKey(item.Key))
                {
                    TModel effector = item.Value.StartElementState ?? item.Value.EndElementState;
                    TVisualController viewModel = InstantiateController(effector);
                    effectors.Add(item.Key, viewModel);
                }
            }
            List<ElementIdentifier> toDelete = new List<ElementIdentifier>();
            foreach (var item in effectors)
            {
                if (effectorStates.ContainsKey(item.Key))
                {
                    TVisualController viewModel = effectors[item.Key];
                    TUIState state = effectorStates[item.Key];
                    viewModel.SetDrawState(state);
                }
                else
                {
                    toDelete.Add(item.Key);
                }
            }
            DeleteObseleteUi(toDelete);
        }

        private void DeleteObseleteUi(List<ElementIdentifier> toDelete)
        {
            foreach (var item in toDelete)
            {
                TVisualController behavior = effectors[item];
                GameObject.Destroy(behavior.gameObject);
                effectors.Remove(item);
            }
        }

        protected abstract Dictionary<ElementIdentifier, TUIState> GetEffectorStates(EncounterState previousState, EncounterState nextState);

    }
}