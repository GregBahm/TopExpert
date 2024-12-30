namespace Investigation.Model
{
    public record PunchItDraftOption (ElementIdentifier Identifier) 
        : StandardDraftOption(Identifier)
    {
        public override int DraftCost => 2;

        public override PlayerCard Card => new PunchItCard(new ElementIdentifier());
    }
}