using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.DTOs;
using CadastreInvent.Valuation.Application.Queries;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class GetValuationHistoryQueryHandler : IRequestHandler<GetValuationHistoryQuery, List<ValuationHistoryDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public GetValuationHistoryQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ValuationHistoryDto>> Handle(GetValuationHistoryQuery request, CancellationToken cancellationToken)
        {
            var sql = @"
                SELECT ""AssessedValue"", ""Method"", ""ValidFrom"", ""ValidTo""
                FROM (
                    SELECT ""AssessedValue"", ""Method"", ""ValidFrom"", ""ValidTo"" FROM valuation.valuations WHERE ""Id"" = @p0
                    UNION ALL
                    SELECT ""AssessedValue"", ""Method"", ""ValidFrom"", ""ValidTo"" FROM valuation.valuations_history WHERE ""Id"" = @p0
                ) AS history
                ORDER BY ""ValidFrom"" DESC";

            var historyRecords = new List<ValuationHistoryDto>();

            using var command = _dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            var param = command.CreateParameter();
            param.ParameterName = "@p0";
            param.Value = request.ValuationId;
            command.Parameters.Add(param);

            bool wasClosed = command.Connection.State == ConnectionState.Closed;
            if (wasClosed)
            {
                await command.Connection.OpenAsync(cancellationToken);
            }

            try
            {
                using var result = await command.ExecuteReaderAsync(cancellationToken);

                while (await result.ReadAsync(cancellationToken))
                {
                    var dto = new ValuationHistoryDto
                    {
                        AssessedValue = result.GetDecimal(0),
                        Method = result.GetString(1),
                        ValidFrom = result.GetDateTime(2),
                        ValidTo = result.GetDateTime(3)
                    };

                    dto.IsCurrent = dto.ValidTo.Year == 9999;
                    historyRecords.Add(dto);
                }
            }
            finally
            {
                if (wasClosed)
                {
                    await command.Connection.CloseAsync();
                }
            }

            return historyRecords;
        }
    }
}