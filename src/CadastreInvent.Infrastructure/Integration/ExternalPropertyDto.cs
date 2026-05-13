namespace CadastreInvent.Infrastructure.Integration
{
    public class ExternalPropertyDto
    {
        public string CadastralNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}