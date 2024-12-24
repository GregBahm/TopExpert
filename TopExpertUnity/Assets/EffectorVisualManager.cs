using Investigation.Behaviors;
using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Investigation.Behaviors
{

    public class EffectorVisualManager : ElementVisualManager<PersistantEffector, EffectorVisualController, EffectorUiState>
    {
        protected override Dictionary<ElementIdentifier, EffectorUiState> GetEffectorStates(EncounterState previousState, EncounterState nextState)
        {
            Dictionary<ElementIdentifier, EffectorUiState> ret = new Dictionary<ElementIdentifier, EffectorUiState>();

            //TODO: this when you sober

            return ret;
        }

        protected override EffectorVisualController InstantiateController(PersistantEffector effector)
        {
            return EncounterVisualsManager.Instance.InstantiateEffectorUi(effector);
        }
    }
}