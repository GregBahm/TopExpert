using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class TimelineAnnotationController : MonoBehaviour
    {
        private EncounterStep step;
        private int stepIndex;

        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = transform as RectTransform;
        }

        internal void SetStep(EncounterStep step, int stepIndex)
        {
            this.step = step;
            this.stepIndex = stepIndex;
        }

        private void Update()
        {
            int totalSteps = EncounterManager.Instance.Encounter.Steps;
            float stepProgression = totalSteps == 1 ?
                .5f 
                :(float)stepIndex / (totalSteps - 1);

            rectTransform.anchorMin = new Vector2(stepProgression, 0);
            rectTransform.anchorMax = new Vector2(stepProgression, 0);
        }
    }
}