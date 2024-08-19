using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Encounter.Model
{
    public record EncounterState
    {
        public EncounterStatus Status { get; init; }

        public int Insights { get; init; }
        public int Actions { get; init; }
        public int ActionsPerTurn { get; init; }
        public int Advantage { get; init; }
        public int AdvantageToWin { get; init; }
        public int AdvantageToLose { get; init; }

        public EncounterPhase Phase { get; init; }

        public int DangerPhaseInsightsCost { get; init; }

        public IReadOnlyList<PersistantEffector> UnappliedEffectors { get; init; }
        public IReadOnlyList<PersistantEffector> AppliedEffectors { get; init; }

        public IReadOnlyList<PlayerCard> Hand { get; init; }
        public IReadOnlyList<PlayerCard> DrawDeck { get; init; }
        public IReadOnlyList<PlayerCard> DiscardDeck { get; init; }
        public IReadOnlyList<PlayerCard> DissolvedCardsDeck { get; init; }

        public int Draws { get; init; }
        public int MaxHandSize { get; init; }

        public IReadOnlyList<DraftOption> DraftDeck { get; init; }
        public IReadOnlyList<DraftOption> DraftOptions { get; init; }
        public int AvailableDrafts { get; init; }

        public EncounterState GetWithDraw()
        {
            EncounterState state = this;
            if (DrawDeck.Any())
            {
                List<PlayerCard> newDrawDeck = DrawDeck.ToList();
                PlayerCard card = newDrawDeck.Last();
                newDrawDeck.RemoveAt(DrawDeck.Count - 1);

                List<PlayerCard> newHand = Hand.ToList();
                newHand.Add(card);

                state = state with { Hand = newHand, DrawDeck = newDrawDeck };
            }
            else
            {
                if (DiscardDeck.Any())
                {
                    state = ShuffleDiscardsIntoDraw();
                    state = GetWithDraw();
                }
            }
            return state;
        }

        private EncounterState ShuffleDiscardsIntoDraw()
        {
            List<PlayerCard> oldDiscardDeck = DiscardDeck.ToList();
            List<PlayerCard> newDrawDeck = new List<PlayerCard>();

            for (int i = 0; i < oldDiscardDeck.Count; i++)
            {
                int nextCard = UnityEngine.Random.Range(0, oldDiscardDeck.Count - 1);
                PlayerCard card = oldDiscardDeck[nextCard];
                oldDiscardDeck.RemoveAt(nextCard);
                newDrawDeck.Add(card);
            }

            return this with { DiscardDeck = new List<PlayerCard>(), DrawDeck = newDrawDeck };
        }

        public EncounterState GetWithCardDiscarded(PlayerCard card)
        {
            List<PlayerCard> hand = Hand.ToList();
            List<PlayerCard> dissolveDeck = DissolvedCardsDeck.ToList();
            List<PlayerCard> discardDeck = DiscardDeck.ToList();

            hand.Remove(card);
            if (card.DissolvesIfNotPlayed)
                dissolveDeck.Add(card);
            else
                discardDeck.Add(card);

            return this with { Hand = hand, DissolvedCardsDeck = dissolveDeck, DiscardDeck = discardDeck };
        }
    }
    public interface IStateModifier { }

    public abstract class PlayerCard : IStateModifier
    {
        public virtual bool DissolvesOnPlay { get; }
        public virtual bool DissolvesIfNotPlayed { get; }
        public virtual bool Persists { get; }

        public abstract EncounterState Play(EncounterState state);
        public abstract bool CanPlay(EncounterState state);
    }

    public abstract class PersistantEffector : IStateModifier
    {
        public EncounterState GetModifiedState(EncounterState state)
        {
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            unappliedEffectors.Remove(this);
            state = state with { UnappliedEffectors = unappliedEffectors };

            List<PersistantEffector> appliedEffectors = state.AppliedEffectors.ToList();
            appliedEffectors.Add(this);
            state = state with { AppliedEffectors= appliedEffectors };
            return ModifyState(state);
        }

        protected abstract EncounterState ModifyState(EncounterState builder);
    }

    public enum EncounterStatus
    {
        Ongoing,
        PlayersWon,
        PlayersLost
    }

    public enum EncounterPhase
    {
        Investigation,
        Danger
    }

    public class RestoreEnergy : PersistantEffector
    {
        protected override EncounterState ModifyState(EncounterState state)
        {
            return state with { Actions = state.ActionsPerTurn };
        }
    }

    public class DrawHand : PersistantEffector
    {
        protected override EncounterState ModifyState(EncounterState state)
        {

            List<PlayerCard> oldHand = state.Hand.ToList();
            List<PlayerCard> dissolvedCards = state.DissolvedCardsDeck.ToList();
            List<PlayerCard> discardedCards = state.DiscardDeck.ToList();

            List<PlayerCard> newHand = new List<PlayerCard>();

            foreach (PlayerCard card in oldHand)
            {
                if (card.Persists)
                {
                    newHand.Add(card);
                }
                else if (card.DissolvesIfNotPlayed)
                {
                    dissolvedCards.Add(card);
                }
                else
                {
                    discardedCards.Add(card);
                }
            }
            state = state with { Hand = newHand, DissolvedCardsDeck = dissolvedCards, DiscardDeck = discardedCards };
            for (int i = 0; i < state.Draws; i++)
            {
                state = state.GetWithDraw();
            }
            return state;
        }
    }

    public class ActivateDangerPhase : IStateModifier
    {
        public EncounterState GetModifiedState(EncounterState state)
        {
            int newInsights = state.Insights - state.DangerPhaseInsightsCost;
            state = state with { Insights = newInsights, Phase = EncounterPhase.Danger };
            return state;
        }
    }
    public abstract class DraftOption : IStateModifier
    {
        public abstract bool CanDraft(EncounterState state);
        public abstract EncounterState DraftCard(EncounterState state);
    }

    public abstract class StandardPlayerCard : PlayerCard
    {
        public abstract int ActionCost { get; }

        public override bool CanPlay(EncounterState state)
        {
            return state.Actions >= ActionCost;
        }

        public override EncounterState Play(EncounterState state)
        {
            state = MoveCardFromHand(state);
            state = state with { Actions = state.Actions - ActionCost };
            state = ModifyState(state);
            return state;
        }

        private EncounterState MoveCardFromHand(EncounterState state)
        {
            List<PlayerCard> hand = state.Hand.ToList();
            List<PlayerCard> dissolveDeck = state.DissolvedCardsDeck.ToList();
            List<PlayerCard> discardDeck = state.DiscardDeck.ToList();
            hand.Remove(this);
            if (DissolvesOnPlay)
                dissolveDeck.Add(this);
            else
                discardDeck.Add(this);

            return state with { Hand = hand, DissolvedCardsDeck = dissolveDeck, DiscardDeck = discardDeck };
        }

        protected abstract EncounterState ModifyState(EncounterState builder);
    }

    public class CarefulResearchCard : StandardPlayerCard
    {
        public override int ActionCost => 1;

        protected override EncounterState ModifyState(EncounterState state)
        {
            return state with { Insights = state.Insights + 2 };
        }
    }

    public class OverthinkerCard : StandardPlayerCard
    {
        public override int ActionCost => 0;

        protected override EncounterState ModifyState(EncounterState state)
        {
            int count = 0;
            foreach (var card in state.Hand
                .Concat(state.DrawDeck)
                .Concat(state.DiscardDeck))
            {
                if (card.GetType() == typeof(OverthinkerCard))
                {
                    count++;
                }
            }
            return state with { Insights = state.Insights + count };
        }
    }

    public class HyperfocusCard : StandardPlayerCard
    {
        public override int ActionCost => 2;
        public override bool DissolvesOnPlay => true;

        protected override EncounterState ModifyState(EncounterState state)
        {
            List<PersistantEffector> unappliedEffectors = state.UnappliedEffectors.ToList();
            HyperfocusEffector hyperfocus = new HyperfocusEffector();
            unappliedEffectors.Add(hyperfocus);
            return state with { UnappliedEffectors = unappliedEffectors, Draws = state.Draws - 1 };
        }
    }

    public class HyperfocusEffector : PersistantEffector
    {
        protected override EncounterState ModifyState(EncounterState state)
        {
            return state with { Insights = state.Insights + 1 };
        }
    }

    public class InvestigateCard : StandardPlayerCard
    {
        public override int ActionCost => 1;

        protected override EncounterState ModifyState(EncounterState state)
        {
            return state with { Insights = state.Insights + 1 };
        }
    }
    public class NewPlanCard : StandardPlayerCard
    {
        public override int ActionCost => 2;

        protected override EncounterState ModifyState(EncounterState state)
        {
            int handSize = state.Hand.Count;
            List<PlayerCard> hand = state.Hand.ToList();
            foreach (var card in hand)
            {
                state = state.GetWithCardDiscarded(card);
            }
            for (int i = 0; i < handSize; i++)
            {
                state = state.GetWithDraw();
            }
            return state;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}