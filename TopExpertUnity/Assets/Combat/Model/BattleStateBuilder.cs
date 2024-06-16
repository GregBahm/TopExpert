using System.Collections.Generic;
using System.Linq;

namespace Combat.Model
{
    public class BattleStateBuilder
    {
        public BattleStatus Status { get; set; }

        public EntityStateBuilder Investigators { get; set; }
        public EntityStateBuilder Enemy { get; set; }

        public List<ICard> Hand { get; set; }
        public List<ICard> DrawDeck { get; set; }
        public List<ICard> DiscardDeck { get; set; }
        public List<ICard> BurnedCardsDeck { get; set; }

        public int MaxResearchPoints { get; set; }
        public int RemainingResearchPoints { get; set; }

        public int Draws { get; set; }
        public int MaxHandSize { get; set; }

        public BattleStateBuilder()
        {
        }

        public BattleStateBuilder(BattleState state)
        {
            Status = state.Status;
            Investigators = new EntityStateBuilder(state.Investigators);
            Enemy = new EntityStateBuilder(state.Enemy);

            Hand = state.Hand.ToList();
            DrawDeck = state.DrawDeck.ToList();
            DiscardDeck = state.DiscardDeck.ToList();
            BurnedCardsDeck = state.BurnedCardsDeck.ToList();

            MaxResearchPoints = state.MaxResearchPoints;
            RemainingResearchPoints = state.RemainingResearchPoints;
            Draws = state.Draws;
            MaxHandSize = state.MaxHandSize;
        }

        public BattleState ToState()
        {
            return new BattleState(Status,
                Investigators.ToState(),
                Enemy.ToState(),
                Hand,
                DrawDeck,
                DiscardDeck,
                BurnedCardsDeck,
                MaxResearchPoints,
                RemainingResearchPoints,
                Draws,
                MaxHandSize);
        }

        internal void TryDrawCardToHand()
        {
            if (DrawDeck.Any())
            {
                ICard card = DrawDeck[0];
                DrawDeck.RemoveAt(0);
                Hand.Add(card);
            }
            else if (DiscardDeck.Any())
            {
                ShuffleDiscardsIntoDrawDeck();
                TryDrawCardToHand();
            }
        }

        private void ShuffleDiscardsIntoDrawDeck()
        {
            List<ICard> newDrawDeck = DrawDeck != null ? DrawDeck.ToList() : new List<ICard>();
            if (DiscardDeck != null)
            {
                newDrawDeck.AddRange(DiscardDeck);
                DiscardDeck.Clear();
            }
            List<ICard> shuffledCards = newDrawDeck.OrderBy(item => UnityEngine.Random.value).ToList(); // Todo: make this random value deterministic
            DrawDeck = shuffledCards;
        }
    }
}