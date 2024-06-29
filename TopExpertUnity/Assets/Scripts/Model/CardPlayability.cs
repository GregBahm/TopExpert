using System.Collections.Generic;

namespace Combat.Model
{
    public class CardPlayability
    {
        public bool IsPlayable { get; private set; }

        public static CardPlayability AlwaysPlayable { get; } = new CardPlayability() { IsPlayable = true };
    }
}