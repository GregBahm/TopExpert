using Investigation.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class EncounterManager : MonoBehaviour
    {
        public static EncounterManager Instance { get; private set; }

        public Model.Encounter Encounter { get; private set; }

        private EncounterVisualsManager visuals;

        [SerializeField]
        private TimelineManager timelineManager;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            visuals = GetComponent<EncounterVisualsManager>();
            Encounter = GetTestEncounter();
            Encounter.EndRound();

            timelineManager.InitializeTimeline(Encounter);
        }

        private Model.Encounter GetTestEncounter()
        {
            List<PlayerCard> drawDeck = new List<PlayerCard>()
            {
                new CarefulResearchCard(new ElementIdentifier()),
                new OverthinkerCard(new ElementIdentifier()),
                new HyperfocusCard(new ElementIdentifier()),
                new HyperfocusCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),

                new CommuneWithSpiritsCard(new ElementIdentifier()),
                new DaringGambitCard(new ElementIdentifier()),
                new GetAfterItCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvokeTheSpiritsCard(new ElementIdentifier()),
                new NewPlanCard(new ElementIdentifier()),
                new DistractionCard(new ElementIdentifier()),
                new OfCourseCard(new ElementIdentifier()),
                new PunchItCard(new ElementIdentifier()),
                new SpringTheTrapCard(new ElementIdentifier()),
                new UnravelTheMysteryCard(new ElementIdentifier()),

                new CarefulResearchCard(new ElementIdentifier()),
                new CommuneWithSpiritsCard(new ElementIdentifier()),
                new DaringGambitCard(new ElementIdentifier()),
                new GetAfterItCard(new ElementIdentifier()),
                new HyperfocusCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvokeTheSpiritsCard(new ElementIdentifier()),
                new NewPlanCard(new ElementIdentifier()),
                new DistractionCard(new ElementIdentifier()),
                new OfCourseCard(new ElementIdentifier()),
                new OverthinkerCard(new ElementIdentifier()),
                new PunchItCard(new ElementIdentifier()),
                new SpringTheTrapCard(new ElementIdentifier()),
                new UnravelTheMysteryCard(new ElementIdentifier()),
            };
            List<PersistantEffector> baseEffectors = new List<PersistantEffector>()
            {
                new HauntedCoffeeMachine(new ElementIdentifier()),
                new DrawHand(new ElementIdentifier()),
                new RestoreEnergy(new ElementIdentifier())
            };

            EncounterState initialState = new EncounterState()
            {
                Actions = 3,
                ActionsPerTurn = 3,
                AdvantageToWin = 20,
                AdvantageToLose = -20,
                UnappliedEffectors = baseEffectors,
                AppliedEffectors = new List<PersistantEffector>(),
                Hand = new List<PlayerCard>(),
                DrawDeck = drawDeck,
                DiscardDeck = new List<PlayerCard>(),
                DissolveDeck = new List<PlayerCard>(),
                Draws = 7,
                MaxHandSize = 10,
            };
            return new Model.Encounter(initialState);
        }

    }
}