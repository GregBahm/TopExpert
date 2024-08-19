using System.Collections.Generic;
using System.Linq;

namespace Encounter.Model
{
    public record EncounterState
    {
        public EncounterStatus Status { get; }

        public int Insights { get; }
        public int Actions { get; }
        public int ActionsPerTurn { get; }
        public int Advantage { get; }
        public int AdvantageToWin { get; }
        public int AdvantageToLose { get; }

        public EncounterPhase Phase { get; }

        public int DangerPhaseInsightsCost { get; }

        public IReadOnlyList<PersistantEffector> UnappliedEffectors { get; }
        public IReadOnlyList<PersistantEffector> AppliedEffectors { get; }

        public IReadOnlyList<PlayerCard> Hand { get; }
        public IReadOnlyList<PlayerCard> DrawDeck { get; }
        public IReadOnlyList<PlayerCard> DiscardDeck { get; }
        public IReadOnlyList<PlayerCard> DissolvedCardsDeck { get; }

        public int Draws { get; }
        public int MaxHandSize { get; }

        public IReadOnlyList<DraftOption> DraftDeck { get; }
        public IReadOnlyList<DraftOption> DraftOptions { get; }
        public int AvailableDrafts { get; }
    }

    public class EncounterStateBuilder
    {
        public EncounterStatus Status { get; set; }

        public int Insights { get; set; }
        public int Actions { get; set; }
        public int ActionsPerTurn { get; set; }
        public int Advantage { get; set; }
        public int AdvantageToWin { get; set; }
        public int AdvantageToLose { get; set; }

        public EncounterPhase Phase { get; set; }

        public int DangerPhaseInsightsCost { get; set; }

        public List<PersistantEffector> UnappliedEffectors { get; set; }
        public List<PersistantEffector> AppliedEffectors { get; set; }

        public List<PlayerCard> Hand { get; set; }
        public List<PlayerCard> DrawDeck { get; set; }
        public List<PlayerCard> DiscardDeck { get; set; }
        public List<PlayerCard> DissolvedCardsDeck { get; set; }

        public int Draws { get; set; }
        public int MaxHandSize { get; set; }

        public List<DraftOption> DraftDeck { get; set; }
        public List<DraftOption> DraftOptions { get; set; }
        public int AvailableDrafts { get; set; }

        public EncounterStateBuilder(EncounterState state)
        {
            Status = state.Status;
            Insights = state.Insights;
            Actions = state.Actions;
            ActionsPerTurn = state.ActionsPerTurn;
            Advantage = state.Advantage;
            AdvantageToWin = state.AdvantageToWin;
            AdvantageToLose = state.AdvantageToLose;
            Phase = state.Phase;
            DangerPhaseInsightsCost = state.DangerPhaseInsightsCost;
            UnappliedEffectors = state.UnappliedEffectors.ToList();
            AppliedEffectors = state.AppliedEffectors.ToList();
            DrawDeck = state.DrawDeck.ToList();
            DiscardDeck = state.DiscardDeck.ToList();
            DissolvedCardsDeck = state.DissolvedCardsDeck.ToList();
            Draws = state.Draws;
            MaxHandSize = state.MaxHandSize;
            DraftDeck = state.DraftDeck.ToList();
            DraftOptions = state.DraftOptions.ToList();
            AvailableDrafts = state.AvailableDrafts;
        }
        public EncounterState ToState()
        {
            return new EncounterState(
                Status,
                Insights,
                Actions,
                ActionsPerTurn,
                Advantage,
                AdvantageToWin,
                AdvantageToLose,
                Phase,
                DangerPhaseInsightsCost,
                UnappliedEffectors.ToList(),
                AppliedEffectors.ToList(),
                Hand.ToList(),
                DrawDeck.ToList(),
                DiscardDeck.ToList(),
                DissolvedCardsDeck.ToList(),
                Draws,
                MaxHandSize,
                DraftDeck.ToList(),
                DraftOptions.ToList(),
                AvailableDrafts
                );
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
            EncounterStateBuilder builder = new EncounterStateBuilder(state);
            builder.UnappliedEffectors.Remove(this);
            ModifyState(builder);
            return builder.ToState();
        }

        protected abstract void ModifyState(EncounterStateBuilder builder);
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
        protected override void ModifyState(EncounterStateBuilder builder)
        {
            builder.Actions = builder.ActionsPerTurn;
            builder.AppliedEffectors.Add(this);
        }
    }

    public class DrawHand : PersistantEffector
    {
        protected override void ModifyState(EncounterStateBuilder builder)
        {
            List<PlayerCard> hand = builder.Hand.ToList();
            builder.Hand = hand.Where(item => item.Persists).ToList();
            foreach (PlayerCard card in hand)
            {
                if(card.DissolvesIfNotPlayed)
                {
                    builder.DissolvedCardsDeck.Add(card);
                }
                else
                {
                    builder.DiscardDeck.Add(card);
                }
            }
            for (int i = 0; i < builder.Draws; i++)
            {
                DrawCard(builder);
            }
        }

        private void DrawCard(EncounterStateBuilder builder)
        {
            if(builder.DrawDeck.Any())
            {
                PlayerCard card = builder.DrawDeck.Last();
                builder.DrawDeck.RemoveAt(builder.DrawDeck.Count - 1);
                builder.Hand.Add(card);
            }
            else
            {
                if(builder.DiscardDeck.Any())
                {
                    ShuffleDiscardIntoDraw(builder);
                    DrawCard(builder);
                }
            }
        }

        private void ShuffleDiscardIntoDraw(EncounterStateBuilder builder)
        {
            List<PlayerCard> discardDeck = builder.DiscardDeck.ToList();
            builder.DiscardDeck.Clear();
            for (int i = 0; i < discardDeck.Count; i++)
            {
                int nextCard = UnityEngine.Random.Range(0, discardDeck.Count - 1);
                PlayerCard card = discardDeck[nextCard];
                discardDeck.RemoveAt(nextCard);
                builder.DrawDeck.Add(card);
            }
        }
    }

    public class ActivateDangerPhase : IStateModifier
    {
        public EncounterState GetModifiedState(EncounterState state)
        {
            EncounterStateBuilder builder = new EncounterStateBuilder(state);
            builder.Insights -= builder.DangerPhaseInsightsCost;
            builder.Phase = EncounterPhase.Danger;
            return builder.ToState();
        }
    }
    public abstract class DraftOption : IStateModifier
    {
        public abstract bool CanDraft(EncounterState state);
        public abstract EncounterState DraftCard(EncounterState state);
    }

    ////////////////////////////////////////////

    public abstract class StandardPlayerCard : PlayerCard
    {
        public abstract int ActionCost { get; }

        public override bool CanPlay(EncounterState state)
        {
            return state.Actions >= ActionCost;
        }

        public override EncounterState Play(EncounterState state)
        {
            EncounterStateBuilder builder = new EncounterStateBuilder(state);
            MoveCardFromHand(builder);
            ModifyState(builder);
            return builder.ToState();
        }

        private void MoveCardFromHand(EncounterStateBuilder builder)
        {
            builder.Hand.Remove(this);
            if (DissolvesOnPlay)
            {
                builder.DissolvedCardsDeck.Add(this);
            }
            else
            {
                builder.DiscardDeck.Add(this);
            }
        }

        protected abstract void ModifyState(EncounterStateBuilder builder);
    }

    public class CarefulResearchCard : StandardPlayerCard
    {
        public override int ActionCost => 1;

        protected override void ModifyState(EncounterStateBuilder builder)
        {
            builder.Insights += 2;
        }
    }

    public class OverthinkerCard : StandardPlayerCard
    {
        public override int ActionCost => 0;

        protected override void ModifyState(EncounterStateBuilder builder)
        {
            int count = 0;
            foreach (var card in builder.Hand
                .Concat(builder.DrawDeck)
                .Concat(builder.DiscardDeck))
            {
                if (card.GetType() == typeof(OverthinkerCard))
                {
                    count++;
                }
            }
            builder.Insights += count;
        }
    }

    public class HyperfocusCard : StandardPlayerCard
    {
        public override int ActionCost => 2;
        public override bool DissolvesOnPlay => true;

        protected override void ModifyState(EncounterStateBuilder builder)
        {
            builder.Draws -= 1;
            HyperfocusEffector hyperfocus = new HyperfocusEffector();
            builder.UnappliedEffectors.Add(hyperfocus);
        }
    }

    public class HyperfocusEffector : PersistantEffector
    {
        protected override void ModifyState(EncounterStateBuilder builder)
        {
            builder.Insights += 1;
            builder.AppliedEffectors.Add(this);
        }
    }

    public class InvestigateCard : StandardPlayerCard
    {
        public override int ActionCost => 1;

        protected override void ModifyState(EncounterStateBuilder builder)
        {
            builder.Insights += 1;
        }
    }
    public class NewPlanCard : StandardPlayerCard
    {
        public override int ActionCost => 2;

        protected override void ModifyState(EncounterStateBuilder builder)
        {
            int handSize = builder.Hand.Count;
            // Discard the hand and redraw as many cards
            // Move the discarding logic to the encounter state builder 
        }
    }
}