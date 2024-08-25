namespace Investigation.Model
{
    public record OfCourseDraftOption(DraftOptionIdentifier Identifier) 
        : StandardDraftOption(Identifier)
    {
        public override int DraftCost => 5;

        public override PlayerCard Card => new OfCourseCard(new CardIdentifier());
    }
}