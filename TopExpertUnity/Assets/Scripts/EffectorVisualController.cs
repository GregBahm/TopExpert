using System;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class EffectorVisualController : ElementVisualController<EffectorUiState>
    {
        private void Update()
        {
            Vector3 location = GetLocation();
            transform.position = location;
        }

        private Vector3 GetLocation()
        {

            Vector3 startLocation = GetStartLocation(state);
            Vector3 endLocation = GetEndLocation(state);
            return Vector3.Lerp(startLocation, endLocation, Mothership.SubTurnDisplay);
        }

        private Vector3 GetStartLocation(EffectorUiState state)
        {
            if (state.StartLocation == EffectorExistenceLocation.Inexistant)
                return Mothership.EffectorLeftPoint.position;

            int appliedEffectors = state.StartState.AppliedEffectors.Count;
            int unAppliedEffectors = state.StartState.UnappliedEffectors.Count;

            int index = state.StartOrder;
            if (state.StartLocation == EffectorExistenceLocation.Applied)
                index += unAppliedEffectors;

            float param = (float)index / (appliedEffectors + unAppliedEffectors);
            return Vector3.Lerp(Mothership.EffectorLeftPoint.position, Mothership.EffectorRightPoint.position, param);
        }

        private Vector3 GetEndLocation(EffectorUiState state)
        {
            if (state.EndLocation == EffectorExistenceLocation.Inexistant)
                return Mothership.EffectorRightPoint.position;

            int appliedEffectors = state.EndState.AppliedEffectors.Count;
            int unAppliedEffectors = state.EndState.UnappliedEffectors.Count;

            int index = state.EndOrder;
            if (state.EndLocation == EffectorExistenceLocation.Applied)
                index += unAppliedEffectors;

            float param = (float)index / (appliedEffectors + unAppliedEffectors);
            return Vector3.Lerp(Mothership.EffectorLeftPoint.position, Mothership.EffectorRightPoint.position, param);
        }
    }
}