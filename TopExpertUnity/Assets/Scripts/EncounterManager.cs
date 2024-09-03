using Investigation.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
{
    public class EncounterManager : MonoBehaviour
    {
        public static EncounterManager Instance { get; private set; }

        public Model.Encounter Encounter { get; private set; }

        private EncounterVisualsManager visuals;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            visuals = GetComponent<EncounterVisualsManager>();
            Encounter = GetTestEncounter();
            Encounter.EndRound();
        }

        private Model.Encounter GetTestEncounter()
        {
            List<PlayerCard> drawDeck = new List<PlayerCard>()
            {
                new CarefulResearchCard(new ElementIdentifier()),
                new CommuneWithSpiritsCard(new ElementIdentifier()),
                new DaringGambitCard(new ElementIdentifier()),
                new GetAfterItCard(new ElementIdentifier()),
                new HyperfocusCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvokeTheSpiritsCard(new ElementIdentifier()),
                new NewPlanCard(new ElementIdentifier()),
                new NoWayCard(new ElementIdentifier()),
                new OfCourseCard(new ElementIdentifier()),
                new OverthinkerCard(new ElementIdentifier()),
                new PunchItCard(new ElementIdentifier()),
                new SpringTheTrapCard(new ElementIdentifier()),
                new UnravelTheMysteryCard(new ElementIdentifier()),

                new CarefulResearchCard(new ElementIdentifier()),
                new CommuneWithSpiritsCard(new ElementIdentifier()),
                new DaringGambitCard(new ElementIdentifier()),
                new GetAfterItCard(new ElementIdentifier()),
                new HyperfocusCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvestigateCard(new ElementIdentifier()),
                new InvokeTheSpiritsCard(new ElementIdentifier()),
                new NewPlanCard(new ElementIdentifier()),
                new NoWayCard(new ElementIdentifier()),
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
            List<DraftOption> DraftDeck = new List<DraftOption>()
            { 
                new ChannelTheOtherSideDraftOption(new ElementIdentifier()),
                new DaringGambitDraftOption(new ElementIdentifier()),
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
                DissolveDeck = new List<PlayerCard>(),
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