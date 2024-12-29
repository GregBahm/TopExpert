using Investigation.Model;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Investigation.Behaviors
{
    public class EffectorVisualController : ElementVisualController<EffectorUiState>
    {
        [SerializeField]
        private Image background;

        private void Update()
        {
            Vector3 location = GetLocation();
            transform.position = location;
            background.color = GetColor();
        }

        private Color GetColor()
        {
            Color startColor = GetColor(state.StartLocation);
            Color endColor = GetColor(state.EndLocation);
            return Color.Lerp(startColor, endColor, Mothership.SubTurnDisplay);
        }

        private Color GetColor(EffectorExistenceLocation location)
        {
            switch (location)
            {
                case EffectorExistenceLocation.Unapplied:
                    return Mothership.UnappliedEffectorColor;
                case EffectorExistenceLocation.Applied:
                    return Mothership.AppliedEffectorColor;
                case EffectorExistenceLocation.Removed:
                case EffectorExistenceLocation.Inexistant:
                default:
                    return Color.clear;
            }
        }

        private Vector3 GetLocation()
        {
            Vector3 startLocation = GetLocation(state.StartState, state.StartOrder, state.StartLocation);
            Vector3 endLocation = GetLocation(state.EndState, state.EndOrder, state.EndLocation);

            if (state.StartLocation == EffectorExistenceLocation.Inexistant)
            {
                startLocation = endLocation + new Vector3(0, Mothership.EffectorEntranceOffset, 0);
            }
            if (state.EndLocation == EffectorExistenceLocation.Inexistant)
            {
                endLocation = startLocation + new Vector3(0, Mothership.EffectorEntranceOffset, 0);
            }

            return Vector3.Lerp(startLocation, endLocation, Mothership.SubTurnDisplay);
        }

        private Vector3 GetLocation(EncounterState encounterState, int stateOrder, EffectorExistenceLocation location)
        {
            if(location == EffectorExistenceLocation.Inexistant)
            {
                return Vector3.zero;
            }

            int appliedEffectors = encounterState.AppliedEffectors.Count;
            int unAppliedEffectors = encounterState.UnappliedEffectors.Count;
            int totalEffectors = appliedEffectors + unAppliedEffectors;
            if(location == EffectorExistenceLocation.Unapplied)
            {
                stateOrder += appliedEffectors;
            }
            float param = totalEffectors == 1 ? .5f : (float)stateOrder / (totalEffectors - 1);
            return Vector3.Lerp(Mothership.EffectorLeftPoint.position, Mothership.EffectorRightPoint.position, param);
        }
    }
}