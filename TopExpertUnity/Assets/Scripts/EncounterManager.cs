using Encounter.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ObjectChangeEventStream;
using static UnityEditor.Progress;

namespace Encounter.Behaviors
{
    public class EncounterManager : MonoBehaviour
    {
        public static EncounterManager Instance { get; private set; }

        public Model.Encounter Encounter { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Encounter = GetTestEncounter();
        }

        private Model.Encounter GetTestEncounter()
        {

            throw new NotImplementedException("Create a sample encounter");
        }

    }
}