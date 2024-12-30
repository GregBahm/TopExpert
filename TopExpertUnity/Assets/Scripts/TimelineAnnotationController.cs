using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class TimelineAnnotationController : MonoBehaviour
    {
        private EncounterStep step;
        private int stepIndex;

        private RectTransform rectTransform;

        [SerializeField]
        private TextMeshProUGUI label;

        private void Start()
        {
            rectTransform = transform as RectTransform;
        }

        internal void SetStep(string label, int stepIndex)
        {
            this.label.text = label;
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