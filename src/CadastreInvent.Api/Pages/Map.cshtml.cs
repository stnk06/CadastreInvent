using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using NetTopologySuite.Geometries;

namespace CadastreInvent.Api.Pages
{
    [Authorize]
    public class MapModel : PageModel
    {
        private readonly CadastreDbContext _dbContext;

        public MapModel(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string SpatialDataJson { get; set; }

        public async Task OnGetAsync()
        {
            var units = await _dbContext.SpatialUnits
                .AsNoTracking()
                .Where(x => !EF.Property<bool>(x, "IsDeleted"))
                .Select(x => new
                {
                    id = x.Id,
                    reference = x.ReferenceNumber,
                    type = x.Type.ToString(),
                    area = x.AreaSqMeters,
                    wkt = x.Boundary.AsText()
                })
                .ToListAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            SpatialDataJson = JsonSerializer.Serialize(units, options);
        }
    }
}