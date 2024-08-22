namespace Encounter.Model
{
    public record OfCourseDraftOption : StandardDraftOption<OfCourseCard>
    {
        public override int DraftCost => 5;
    }
}