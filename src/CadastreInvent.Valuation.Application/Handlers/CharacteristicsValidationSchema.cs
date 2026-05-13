namespace CadastreInvent.Valuation.Application.Handlers
{
    public class CharacteristicsValidationSchema
    {
        public float? Area { get; set; }
        public float? YearBuilt { get; set; }
        public float? Floor { get; set; }
        public float? DistanceToCenterKm { get; set; }
        public float? RoomsCount { get; set; }

        public bool IsValid() =>
            Area.HasValue &&

            YearBuilt.HasValue &&
            Floor.HasValue &&
            DistanceToCenterKm.HasValue &&
            RoomsCount.HasValue;

        public float GetArea() => Area ?? 0f;
        public float GetYearBuilt() => YearBuilt ?? 0f;
        public float GetFloor() => Floor ?? 0f;
        public float GetDistance() => DistanceToCenterKm ?? 0f;
        public float GetRooms() => RoomsCount ?? 0f;
    }
}