namespace CadastreInvent.Registry.Domain.Enums
{
    public enum SpatialUnitType { Parcel, Building, Room, Volume3D }
    public enum BAUnitType { BasicPropertyUnit, LeasedUnit, RightOfUseUnit }
    public enum PartyType { NaturalPerson, NonNaturalPerson, Municipality, State }
    public enum RRRType { Ownership, Lease, Mortgage, Servitude, Usufruct }
    public enum SourceType { SaleContract, CourtDecision, InheritanceCertificate, AdministrativeAct }
    public enum BatchJobStatus { Pending, Processing, Completed, Failed }
    public enum BatchItemStatus { Pending, Processed, Failed }
}