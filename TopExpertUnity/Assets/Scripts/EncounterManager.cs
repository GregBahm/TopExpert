using Encounter.Model;
using System.Collections.Generic;
using UnityEngine;

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
            List<PlayerCard> drawDeck = new List<PlayerCard>()
            {
            };
            List<PersistantEffector> baseEffectors = new List<PersistantEffector>()
            {
                // TODO: Some enemies go here
                new DrawHand(),
                new RestoreEnergy()
            };
            List<DraftOption> DraftDeck = new List<DraftOption>()
            { 
                // TODO: Draft Options go here
            };

            EncounterState initialState = new EncounterState()
            {
                Status = EncounterStatus.Ongoing,
                Actions = 3,
                ActionsPerTurn = 3,
                AdvantageToWin = 10,
                AdvantageToLose = -10,
                Phase = EncounterPhase.Investigation,
                DangerPhaseInsightsCost = 10,
                UnappliedEffectors = baseEffectors,
                AppliedEffectors = new List<PersistantEffector>(),
                Hand = new List<PlayerCard>(),
                DrawDeck = drawDeck,
                DiscardDeck = new List<PlayerCard>(),
                DissolvedCardsDeck = new List<PlayerCard>(),
                Draws = 7,
                MaxHandSize = 10,
                DraftDeck = DraftDeck,
                DraftOptions = new List<DraftOption>(),
                AvailableDrafts = 4
            };
            return new Model.Encounter(initialState);
        }

    }
}