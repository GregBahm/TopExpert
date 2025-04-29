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
                new CarefulResearchCard(),
                new OverthinkerCard(),
                new HyperfocusCard(),
                new HyperfocusCard(),
                new InvestigateCard(),
                new InvestigateCard(),
                new InvestigateCard(),

                new CommuneWithSpiritsCard(),
                new DaringGambitCard(),
                new GetAfterItCard(),
                new InvestigateCard(),
                new InvestigateCard(),
                new InvokeTheSpiritsCard(),
                new NewPlanCard(),
                new DistractionCard(),
                new OfCourseCard(),
                new PunchItCard(),
                new SpringTheTrapCard(),
                new UnravelTheMysteryCard(),

                new CarefulResearchCard(),
                new CommuneWithSpiritsCard(),
                new DaringGambitCard(),
                new GetAfterItCard(),
                new HyperfocusCard(),
                new InvestigateCard(),
                new InvokeTheSpiritsCard(),
                new NewPlanCard(),
                new DistractionCard(),
                new OfCourseCard(),
                new OverthinkerCard(),
                new PunchItCard(),
                new SpringTheTrapCard(),
                new UnravelTheMysteryCard(),
            };
            List<PersistantEffector> baseEffectors = new List<PersistantEffector>()
            {
                new HauntedCoffeeMachine(),
                new DrawHand(),
                new RestoreEnergy()
            };

            EncounterState initialState = new EncounterState()
            {
                Actions = 3,
                ActionsPerTurn = 3,
                AdvantageToWin = 20,
                DangerToLose = -20,
                UnappliedEffectors = baseEffectors,
                AppliedEffectors = new List<PersistantEffector>(),
                Hand = new List<PlayerCard>(),
                DrawDeck = drawDeck,
                DiscardDeck = new List<PlayerCard>(),
                DissolveDeck = new List<PlayerCard>(),
                BaseDraws = 7,
                MaxHandSize = 10,
            };
            return new Model.Encounter(initialState);
        }

    }
}