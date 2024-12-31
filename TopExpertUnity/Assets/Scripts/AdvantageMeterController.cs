using Investigation.Behaviors;
using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class AdvantageMeterController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform friendlyBar;
        [SerializeField]
        private RectTransform enemyBar;

        public void SetVisual(EncounterState previousState, EncounterState nextState)
        {
            float previousAdvantage = GetNormalizedAdvantage(previousState);
            float nextAdvantage = GetNormalizedAdvantage(nextState);

            float displayAdvantage = Mathf.Lerp(previousAdvantage, nextAdvantage, EncounterVisualsManager.Instance.SubTurnDisplay);
            SetVisual(displayAdvantage);
        }

        private float GetNormalizedAdvantage(EncounterState state)
        {
            if (state.Advantage > 0)
            {
                return (float)state.Advantage / state.AdvantageToWin;
            }
            return (float)-state.Advantage / state.AdvantageToLose;
        }

        private void SetVisual(float advantage)
        {
            float friendlyAdvantage = Mathf.Clamp(advantage, 0, 1);
            friendlyAdvantage = Mathf.Lerp(.5f, 1, friendlyAdvantage);
            float enemyAdvantage = Mathf.Clamp(-advantage, 0, 1);
            enemyAdvantage = Mathf.Lerp(.5f, 0, enemyAdvantage);

            friendlyBar.anchorMax = new Vector2(friendlyAdvantage, 1);
            enemyBar.anchorMin = new Vector2(enemyAdvantage, 0);
        }
    }
}