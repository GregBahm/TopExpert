using System;
using System.Collections.Generic;
using System.Linq;

namespace Investigation.Model
{
    public record EncounterState
    {
        public EncounterStatus Status
        {
            get
            {
                if (Advantage >= AdvantageToWin)
                    return EncounterStatus.PlayersWon;
                if (Advantage <= DangerToLose)
                    return EncounterStatus.PlayersLost;
                return EncounterStatus.Ongoing;
            }
        }

        public int Insights { get; init; }
        public int Actions { get; init; }
        public int ActionsPerTurn { get; init; }
        public int Advantage { get; init; }
        public int AdvantageToWin { get; init; }
        public int Danger { get; init; }
        public int DangerToLose { get; init; }
        public int Defenses { get; init; }

        public IReadOnlyList<PersistantEffector> UnappliedEffectors { get; init; }
        public IReadOnlyList<PersistantEffector> AppliedEffectors { get; init; }

        public IReadOnlyList<PlayerCard> Hand { get; init; }
        public IReadOnlyList<PlayerCard> DrawDeck { get; init; }
        public IReadOnlyList<PlayerCard> DiscardDeck { get; init; }
        public IReadOnlyList<PlayerCard> DissolveDeck { get; init; }

        public int BaseDraws { get; init; }
        public int TemporaryDraws { get; init; } // TemporaryDraws is reset to 0 by DrawHand
        public int MaxHandSize { get; init; }

        public int SpiritsPower { get; init; }

        public int DeckReshuffles { get; init; }

        public IEnumerable<PlayerCard> AllCards
        {
            get
            {
                foreach (var card in Hand)
                {
                    yield return card;
                }
                foreach (var card in DrawDeck)
                {
                    yield return card;
                }
                foreach (var card in DiscardDeck)
                {
                    yield return card;
                }
                foreach (var card in DissolveDeck)
                {
                    yield return card;
                }
            }
        }

        public IEnumerable<PersistantEffector> AllEffectors
        {
            get
            {
                foreach (PersistantEffector effector in UnappliedEffectors)
                {
                    yield return effector;
                }
                foreach (PersistantEffector effector in AppliedEffectors)
                {
                    yield return effector;
                }
            }
        }

        public EncounterState GetWithDraw()
        {
            return GetWithDraw(this);
        }

        private static EncounterState GetWithDraw(EncounterState state)
        {
            if (state.DrawDeck.Any())
            {
                List<PlayerCard> newDrawDeck = state.DrawDeck.ToList();
                PlayerCard card = newDrawDeck.Last();
                newDrawDeck.RemoveAt(state.DrawDeck.Count - 1);

                List<PlayerCard> newHand = state.Hand.ToList();
                newHand.Add(card);

                state = state with { Hand = newHand, DrawDeck = newDrawDeck };
            }
            else
            {
                if (state.DiscardDeck.Any())
                {
                    state = ShuffleDiscardsIntoDraw(state);
                    state = GetWithDraw(state);
                }
            }
            return state;
        }

        private static EncounterState ShuffleDiscardsIntoDraw(EncounterState state)
        {
            List<PlayerCard> oldDiscardDeck = state.DiscardDeck.ToList();
            List<PlayerCard> newDrawDeck = new List<PlayerCard>();
            int originalCount = oldDiscardDeck.Count;
            for (int i = 0; i < originalCount; i++)
            {
                int nextCard = UnityEngine.Random.Range(0, oldDiscardDeck.Count);
                PlayerCard card = oldDiscardDeck[nextCard];
                oldDiscardDeck.RemoveAt(nextCard);
                newDrawDeck.Add(card);
            }

            return state with { DiscardDeck = new List<PlayerCard>(), DrawDeck = newDrawDeck, DeckReshuffles = state.DeckReshuffles + 1 };
        }

        public EncounterState GetWithCardDiscarded(PlayerCard card)
        {
            List<PlayerCard> hand = Hand.ToList();
            List<PlayerCard> dissolveDeck = DissolveDeck.ToList();
            List<PlayerCard> discardDeck = DiscardDeck.ToList();

            hand.Remove(card);
            if (card.DissolvesIfNotPlayed)
                dissolveDeck.Add(card);
            else
                discardDeck.Add(card);

            return this with { Hand = hand, DissolveDeck = dissolveDeck, DiscardDeck = discardDeck };
        }
        public EncounterState GetWithEffectorAdded(PersistantEffector effector)
        {
            List<PersistantEffector> unappliedEffectors = UnappliedEffectors.ToList();
            unappliedEffectors.Add(effector);
            return this with { UnappliedEffectors = unappliedEffectors };
        }

        public EncounterState GetWithEffectorReplaced(PersistantEffector newEffector)
        {
            return GetWithEffectorReplaced(newEffector.Identifier, newEffector);
        }

        public EncounterState GetWithEffectorReplaced(ElementIdentifier identifier, PersistantEffector newEffector)
        {
            List<PersistantEffector> unapplied = UnappliedEffectors.ToList();
            List<PersistantEffector> applied = AppliedEffectors.ToList();
            for (int i = 0; i < unapplied.Count; i++)
            {
                ElementIdentifier old = unapplied[i].Identifier;
                if(old == identifier)
                {
                    unapplied[i] = newEffector;
                }
            }
            for (int i = 0; i < applied.Count; i++)
            {
                ElementIdentifier old = applied[i].Identifier;
                if (old == identifier)
                {
                    applied[i] = newEffector;
                }
            }
            return this with { UnappliedEffectors = unapplied, AppliedEffectors = applied };
        }

        internal EncounterState GetWithEffectorRemoved(ElementIdentifier identifier)
        {
            List<PersistantEffector> unapplied = UnappliedEffectors.Where(item => item.Identifier != identifier).ToList();
            List<PersistantEffector> applied = AppliedEffectors.Where(item => item.Identifier != identifier).ToList();
            return this with { UnappliedEffectors = unapplied, AppliedEffectors = applied };
        }

        public EncounterState GetWithDangerApplied(int danger)
        {
            int afterDefense = danger - Defenses;
            if (afterDefense > 0)
            {
                return this with { Defenses = afterDefense };
            }
            else
            {
                return this with { Defenses = 0, Danger = -afterDefense };
            }
        }

        internal EncounterState GetWithCardAdded(PlayerCard card, CardExistenceLocation location, bool addToEnd = false)
        {
            switch (location)
            {
                case CardExistenceLocation.Inexistant:
                        throw new Exception("Can't add a card to inexistance");
                case CardExistenceLocation.Discard:
                    return GetWithCardAddedToDiscard(card);
                case CardExistenceLocation.Dissolve:
                    return GetWithCardAddedToDissolve(card);
                case CardExistenceLocation.DrawDeck:
                    return GetWithCardAddedToDrawDeck(card);
                case CardExistenceLocation.Hand:
                default:
                    return GetWithCardAddedToHand(card, addToEnd);

            }
        }
        private EncounterState GetWithCardAddedToDrawDeck(PlayerCard card, bool addToEnd = false)
        {
            List<PlayerCard> drawDeck = DrawDeck.ToList();
            if (addToEnd)
            {
                drawDeck.Add(card);
            }
            else
            {
                drawDeck.Insert(0, card);
            }
            return this with { DrawDeck = drawDeck };
        }

        private EncounterState GetWithCardAddedToDissolve(PlayerCard card, bool addToEnd = false)
        {
            List<PlayerCard> dissolve = DissolveDeck.ToList();
            if (addToEnd)
            {
                dissolve.Add(card);
            }
            else
            {
                dissolve.Insert(0, card);
            }
            return this with { DissolveDeck = dissolve };
        }

        private EncounterState GetWithCardAddedToDiscard(PlayerCard card, bool addToEnd = false)
        {
            List<PlayerCard> discard = DiscardDeck.ToList();
            if (addToEnd)
            {
                discard.Add(card);
            }
            else
            {
                discard.Insert(0, card);
            }
            return this with { DiscardDeck = discard };
        }

        private EncounterState GetWithCardAddedToHand(PlayerCard card, bool addToEnd)
        {
            List<PlayerCard> hand = Hand.ToList();
            if(addToEnd)
            {
                hand.Add(card);
            }
            else
            {
                hand.Insert(0, card);
            }
            return this with { Hand = hand };
        }

        public EncounterState GetWithCardDissolved(ElementIdentifier identifier)
        {
            List<PlayerCard> newHand = new List<PlayerCard>();
            List<PlayerCard> newDiscard = new List<PlayerCard>();
            List<PlayerCard> newDrawDeck = new List<PlayerCard>();

            List<PlayerCard> dissolved = DissolveDeck.ToList();
            
            foreach (PlayerCard card in Hand)
            {
                if(card.Identifier == identifier)
                    dissolved.Add(card);
                else
                    newHand.Add(card);
            }
            foreach (PlayerCard card in DiscardDeck)
            {
                if (card.Identifier == identifier)
                    dissolved.Add(card);
                else
                    newDiscard.Add(card);
            }
            foreach (PlayerCard card in DrawDeck)
            {
                if (card.Identifier == identifier)
                    dissolved.Add(card);
                else
                    newDrawDeck.Add(card);
            }
            return this with { Hand = newHand, DiscardDeck= newDiscard,DrawDeck = newDrawDeck, DissolveDeck = dissolved };
        }
    }
}