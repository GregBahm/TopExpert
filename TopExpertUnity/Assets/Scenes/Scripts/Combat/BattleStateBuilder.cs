using System.Collections.Generic;
using System.Linq;

namespace TopExpert.Combat
{
    public class BattleStateBuilder
    {
        public BattleStatus Status { get; set; }

        public EntityStateBuilder Investigators { get; set; }
        public List<EntityStateBuilder> Enemies { get; set; }

        public List<ICard> Hand { get; set; }
        public List<ICard> DrawDeck { get; set; }
        public List<ICard> DiscardDeck { get; set; }
        public List<ICard> BurnedCardsDeck { get; set; }

        public int MaxResearchPoints { get; set; }
        public int RemainingResearchPoints { get; set; }

        public int MaxActionPoints { get; set; }
        public int RemainingActionpoints { get; set; }

        public int Draws { get; set; }
        public int MaxHandSize { get; set; }

        public BattleStateBuilder()
        {
        }

        public BattleStateBuilder(BattleState state)
        {
            Status = state.Status;
            Investigators = new EntityStateBuilder(state.Investigators);
            Enemies = state.Enemies.Select(item => new EntityStateBuilder(item)).ToList();

            Hand = state.Hand.ToList();
            DrawDeck = state.DrawDeck.ToList();
            DiscardDeck = state.DiscardDeck.ToList();
            BurnedCardsDeck = state.BurnedCardsDeck.ToList();

            MaxResearchPoints = state.MaxResearchPoints;
            RemainingResearchPoints = state.RemainingResearchPoints;
            MaxActionPoints = state.MaxActionPoints;
            RemainingActionpoints = state.RemainingActionPoints;
            Draws = state.Draws;
            MaxHandSize = state.MaxHandSize;
        }

        public EntityStateBuilder GetEnemy(EntityId id)
        {
            return Enemies.First(item => item.Id == id);
        }

        public BattleState ToState()
        {
            List<EntityState> enemies = Enemies.Select(item => item.ToState()).ToList();
            return new BattleState(Status,
                Investigators.ToState(),
                enemies,
                Hand,
                DrawDeck,
                DiscardDeck,
                BurnedCardsDeck,
                MaxResearchPoints,
                RemainingResearchPoints,
                MaxActionPoints,
                RemainingActionpoints,
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