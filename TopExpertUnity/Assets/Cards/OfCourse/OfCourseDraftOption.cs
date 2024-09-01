namespace Investigation.Model
{
    public record OfCourseDraftOption(ElementIdentifier Identifier) 
        : StandardDraftOption(Identifier)
    {
        public override int DraftCost => 5;

        public override PlayerCard Card => new OfCourseCard(new ElementIdentifier());
    }
}