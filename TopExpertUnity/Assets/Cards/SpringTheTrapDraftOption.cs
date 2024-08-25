namespace Investigation.Model
{
    public record SpringTheTrapDraftOption : StandardDraftOption
    {
        public override int DraftCost => 5;

        public override PlayerCard Card => new SpringTheTrapCard(new CardIdentifier());
    }
}