namespace Investigation.Model
{
    public record PunchItDraftOption : StandardDraftOption
    {
        public override int DraftCost => 2;

        public override PlayerCard Card => new PunchItCard(new CardIdentifier());
    }
}