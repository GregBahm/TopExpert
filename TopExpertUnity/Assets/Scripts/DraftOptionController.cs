using Investigation.Behaviors;
using Investigation.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class DraftOptionController : ElementVisualController<DraftUiState>
    {
        private void Update()
        {
            Vector3 location = GetLocation();
            transform.position = location;
        }


        private Vector3 GetLocation()
        {
            Vector3 startLocation = GetLocation(state.StartState, state.StartOrder, state.StartLocation);
            Vector3 endLocation = GetLocation(state.EndState, state.EndOrder, state.EndLocation);

            if (state.StartLocation == DraftExistenceLocation.Inexistant)
            {
                startLocation = endLocation + new Vector3(0, Mothership.EffectorEntranceOffset, 0);
            }
            if (state.EndLocation == DraftExistenceLocation.Inexistant)
            {
                endLocation = startLocation + new Vector3(0, Mothership.EffectorEntranceOffset, 0);
            }

            return Vector3.Lerp(startLocation, endLocation, Mothership.SubTurnDisplay);
        }


        private Vector3 GetLocation(EncounterState encounterState, int stateOrder, DraftExistenceLocation location)
        {
            if (location == DraftExistenceLocation.Inexistant)
            {
                return Vector3.zero;
            }
            if (location == DraftExistenceLocation.DraftDeck)
            {
                return Mothership.DraftDrawPile.position;
            }

            int draftOptions = encounterState.DraftOptions.Count;
            float param = draftOptions == 1 ? .5f : (float)stateOrder / (draftOptions - 1);
            return Vector3.Lerp(Mothership.DraftLeftPoint.position, Mothership.DraftRightPoint.position, param);
        }
    }
}
