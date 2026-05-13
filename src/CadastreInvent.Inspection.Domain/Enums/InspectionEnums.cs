namespace CadastreInvent.Inspection.Domain.Enums
{
    public enum TaskState
    {
        Created,
        Assigned,
        InProgress,
        Completed,
        Verified,
        RequiresRework
    }

    public enum ViolationStatus
    {
        None,
        Minor,
        Major,
        Critical
    }

    public enum ObservationCategory
    {
        BoundaryVerification,
        ConditionAssessment,
        DiscrepancyFound,
        IllegalConstruction
    }
}