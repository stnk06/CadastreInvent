using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace CadastreInvent.Infrastructure.Integration
{
    public class DadataApiClient : IExternalCadastreService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DadataApiClient> _logger;

        public DadataApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<DadataApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var apiKey = configuration["Dadata:ApiKey"] ?? throw new ArgumentNullException("Dadata:ApiKey");

            _httpClient.BaseAddress = new Uri("https://suggestions.dadata.ru/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<List<ExternalPropertyDto>> GetPropertiesInAreaAsync(string wktPolygon, CancellationToken cancellationToken)
        {
            var reader = new WKTReader();
            var geometry = reader.Read(wktPolygon);
            var env = geometry.EnvelopeInternal;

            var points = new List<Coordinate>();
            double step = 0.002;

            for (double x = env.MinX; x <= env.MaxX; x += step)
            {
                for (double y = env.MinY; y <= env.MaxY; y += step)
                {
                    var pt = new Point(x, y);
                    if (geometry.Contains(pt) || geometry.Distance(pt) < 0.001)
                    {
                        points.Add(new Coordinate(x, y));
                    }
                }
            }

            if (points.Count == 0)
            {
                points.Add(geometry.Centroid.Coordinate);
            }

            var targetPoints = points.Take(40).ToList();
            var results = new Dictionary<string, ExternalPropertyDto>();

            foreach (var pt in targetPoints)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var requestBody = new
                {
                    lat = pt.Y,
                    lon = pt.X,
                    radius_meters = 200,
                    count = 20
                };

                var response = await _httpClient.PostAsJsonAsync("suggestions/api/4_1/rs/geolocate/address", requestBody, cancellationToken);

                response.EnsureSuccessStatusCode();

                var dadataResponse = await response.Content.ReadFromJsonAsync<DadataResponse>(cancellationToken: cancellationToken);
                if (dadataResponse?.Suggestions != null)
                {
                    foreach (var suggestion in dadataResponse.Suggestions)
                    {
                        string cadnum = suggestion.Data?.Cadnum;
                        string address = suggestion.Value;
                        string latStr = suggestion.Data?.GeoLat;
                        string lonStr = suggestion.Data?.GeoLon;

                        if (string.IsNullOrWhiteSpace(address)) continue;

                        string key = !string.IsNullOrWhiteSpace(cadnum) ? cadnum : address;

                        if (!results.ContainsKey(key))
                        {
                            double lat = pt.Y;
                            double lon = pt.X;

                            if (double.TryParse(latStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pLat) &&
                                double.TryParse(lonStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pLon))
                            {
                                lat = pLat;
                                lon = pLon;
                            }

                            results[key] = new ExternalPropertyDto
                            {
                                CadastralNumber = !string.IsNullOrWhiteSpace(cadnum) ? cadnum : $"GEN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                                Address = address,
                                Latitude = lat,
                                Longitude = lon
                            };
                        }
                    }
                }

                await Task.Delay(150, cancellationToken);
            }

            return results.Values.ToList();
        }

        private class DadataResponse
        {
            [JsonPropertyName("suggestions")]
            public DadataSuggestion[] Suggestions { get; set; } = Array.Empty<DadataSuggestion>();
        }

        private class DadataSuggestion
        {
            [JsonPropertyName("value")]
            public string? Value { get; set; }

            [JsonPropertyName("data")]
            public DadataData? Data { get; set; }
        }

        private class DadataData
        {
            [JsonPropertyName("cadnum")]
            public string? Cadnum { get; set; }
            [JsonPropertyName("geo_lat")]
            public string? GeoLat { get; set; }
            [JsonPropertyName("geo_lon")]
            public string? GeoLon { get; set; }
        }
    }
}