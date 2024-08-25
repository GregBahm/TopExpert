using Investigation.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Investigation.Behaviors
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
            Encounter.EndRound();
        }

        private Model.Encounter GetTestEncounter()
        {
            List<PlayerCard> drawDeck = new List<PlayerCard>()
            {
                new CarefulResearchCard(new CardIdentifier()),
                new CarefulResearchCard(new CardIdentifier()),
                new CarefulResearchCard(new CardIdentifier()),
                new OverthinkerCard(new CardIdentifier()),
                new OverthinkerCard(new CardIdentifier()),
                new OverthinkerCard(new CardIdentifier()),
                new HyperfocusCard(new CardIdentifier()),
                new InvestigateCard(new CardIdentifier()),
                new NewPlanCard(new CardIdentifier())
            };
            List<PersistantEffector> baseEffectors = new List<PersistantEffector>()
            {
                new HauntedCoffeeMachine(new EffectorIdentifier()),
                new DrawHand(new EffectorIdentifier()),
                new RestoreEnergy(new EffectorIdentifier())
            };
            List<DraftOption> DraftDeck = new List<DraftOption>()
            { 
                new ChannelTheOtherSideDraftOption(new DraftOptionIdentifier()),
                new DaringGambitDraftOption(new DraftOptionIdentifier()),
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