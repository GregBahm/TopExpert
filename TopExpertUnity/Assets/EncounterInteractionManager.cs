using Investigation.Behaviors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EncounterManager))]
public class EncounterInteractionManager : MonoBehaviour
{
    private EncounterManager encounter;

    private void Start()
    {
        encounter = GetComponent<EncounterManager>();
    }

    public void EndTurn()
    {
        encounter.Encounter.EndRound();
    }
}
