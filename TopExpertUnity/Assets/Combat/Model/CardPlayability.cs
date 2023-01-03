using System.Collections.Generic;

namespace Combat.Model
{
    public class CardPlayability
    {
        public bool IsPlayable { get; set; }
        public bool NeedsTarget { get; set; }
        public List<EntityId> PotentialTargets { get; set; }
    }
}