namespace Investigation.Model
{
    public record OfCourseDraftOption : StandardDraftOption
    {
        public override int DraftCost => 5;

        public override PlayerCard Card => new OfCourseCard(new CardIdentifier());
    }
}