﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Investigation.Model
{
    public class Encounter
    {
        private EncounterProgression progression;
        public EncounterState CurrentState
        { 
            get 
            { 
                return progression.LastState; 
            }
        }

        public int Turns { get { return progression.Turns.Count; } }
        public int Steps { get { return progression.Steps.Count; } }

        public event EventHandler<EncounterStep> StepAdded;

        public Encounter(EncounterState initialState)
        {
            progression = new EncounterProgression(initialState);
        }

        public void PlayCard(PlayerCard card)
        {
            EncounterState nextState = card.Play(CurrentState);
            progression = progression.GetWithAddedStep(card, nextState);
            OnStepAdded();
        }

        public EncounterTurn GetTurn(int turn)
        {
            return progression.Turns[turn];
        }
        public EncounterStep GetStep(int step)
        {
            return progression.Steps[step];
        }

        public void EndRound()
        {
            EncounterState state = CurrentState;
            while(state.UnappliedEffectors.Any())
            {
                PersistantEffector effector = state.UnappliedEffectors.First();
                state = effector.GetModifiedState(state);
                progression = progression.GetWithAddedStep(effector, state);
                OnStepAdded();
            }
            List<PersistantEffector> appliedEffectors = state.AppliedEffectors.ToList();
            EncounterState nextTurnState = state with { UnappliedEffectors = appliedEffectors, AppliedEffectors = new List<PersistantEffector>() };
            progression = progression.GetWithAddedTurn(null, nextTurnState);
            OnStepAdded();
        }

        private void OnStepAdded()
        {
            StepAdded?.Invoke(this, progression.Steps.Last());
        }
    }

    public record EncounterProgression(EncounterState initialState)
    {
        public IReadOnlyList<EncounterStep> Steps { get; init; } = new List<EncounterStep>()
        {
            new EncounterStep(null, initialState)
        };
        public IReadOnlyList<EncounterTurn> Turns { get; init; } = new List<EncounterTurn>()
        {
            new EncounterTurn(new EncounterStep(null, initialState))
        };
        public EncounterTurn LastTurn
        {
            get
            {
                return Turns[Turns.Count - 1];
            }
        }

        public EncounterState LastState
        {
            get
            {
                return Steps[Steps.Count - 1].State;
            }
        }

        public EncounterProgression GetWithAddedStep(IStateModifier modifier, EncounterState state)
        {
            EncounterStep newStep = new EncounterStep(modifier, state);
            List<EncounterStep> steps = Steps.ToList();
            steps.Add(newStep);

            List<EncounterTurn> turns = Turns.ToList();
            EncounterTurn newTurn = turns[turns.Count - 1].GetWithAddedStep(newStep);
            turns[turns.Count - 1] = newTurn;

            return this with { Steps = steps, Turns = turns };
        }

        public EncounterProgression GetWithAddedTurn(IStateModifier modifier, EncounterState state)
        {
            EncounterStep newStep = new EncounterStep(modifier, state);
            List<EncounterStep> steps = Steps.ToList();
            steps.Add(newStep);

            List<EncounterTurn> turns = Turns.ToList();
            EncounterTurn newTurn = new EncounterTurn(newStep);
            turns.Add(newTurn);
            return this with { Steps = steps, Turns = turns };
        }
    }

    public record EncounterTurn(EncounterStep initialStep)
    {
        public IReadOnlyList<EncounterStep> Steps { get; init; } = new List<EncounterStep>() { initialStep };

        public EncounterState StartingState
        {
            get
            {
                return Steps[0].State;
            }
        }
        public EncounterState EndingState
        {
            get
            {
                return Steps[Steps.Count - 1].State;
            }
        }

        public EncounterTurn GetWithAddedStep(EncounterStep step)
        {
            List<EncounterStep> steps = Steps.ToList();
            steps.Add(step);

            return this with { Steps = steps };
        }
    }

    public class EncounterStep
    {
        public IStateModifier Modifier { get; }
        public EncounterState State { get; }

        public EncounterStep(IStateModifier modifier, EncounterState state)
        {
            Modifier = modifier;
            State = state;
        }
    }
}