using Investigation.Behaviors;
using Investigation.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Investigation.Behaviors
{
    public class EffectorVisualManager
    {
        private Dictionary<ElementIdentifier, EffectorVisualController> effectors = new Dictionary<ElementIdentifier, EffectorVisualController>();

        public void VisualizeEncounter(EncounterState previousState, EncounterState nextState)
        {
        }
    }

    public class EffectorVisualController : MonoBehaviour
    {

    }
}