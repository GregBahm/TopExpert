using Investigation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Investigation.Behaviors
{
    public class TimelineManager : MonoBehaviour
    {
        private float Progression
        {
            get
            {
                return EncounterVisualsManager.Instance.Progression;
            }
            set
            {
                EncounterVisualsManager.Instance.Progression = value;
            }
        }

        [SerializeField]
        private Slider timeSlider;

        [SerializeField]
        private bool autoAdvance = true;

        [SerializeField]
        private RectTransform annotationsRoot;

        [SerializeField]
        private GameObject annotationPrefab;

        [SerializeField]
        private float timelineSpeed;

        private int encounterStepsLastFrame;



        private void OnStepAdded(object sender, EncounterStep e)
        {
            int stepsCount = EncounterManager.Instance.Encounter.Steps;
            AddStep(e, stepsCount - 1);
            autoAdvance = true;
        }

        private void AddStep(EncounterStep step, int stepIndex)
        {
            GameObject annotation = Instantiate(annotationPrefab, annotationsRoot);
            TimelineAnnotationController controller = annotation.GetComponent<TimelineAnnotationController>();
            string label = VisualBinding.Instance.GetTimelineAnnotationFor(step);
            controller.SetStep(label, stepIndex);
        }

        private bool movedFromCode;

        private void Update()
        {
            movedFromCode = true;
            Model.Encounter encounter = EncounterManager.Instance.Encounter;
            if (encounter.Steps != encounterStepsLastFrame)
            {
                UpdateSliderValues(encounterStepsLastFrame, encounter.Steps);
            }
            if (autoAdvance)
            {
                float advance = (Time.deltaTime * timelineSpeed) / encounter.Steps;
                Progression = Mathf.Clamp01(Progression + advance);
                timeSlider.value = Progression;
            }
            encounterStepsLastFrame = encounter.Steps;
            movedFromCode = false;
        }

        private void UpdateSliderValues(int oldSteps, int newSteps)
        {
            float currentStep = oldSteps * Progression;
            Progression = currentStep / newSteps;
            timeSlider.value = Progression;
        }

        public void OnSliderMoved()
        {
            Progression = timeSlider.value;
            if (!movedFromCode)
            {
                autoAdvance = false;
            }
        }

        internal void InitializeTimeline(Encounter encounter)
        {
            for (int i = 0; i < encounter.Steps; i++)
            {
                EncounterStep step = encounter.GetStep(i);
                AddStep(step, i);
            }
            encounter.StepAdded += OnStepAdded;
        }
    }
}