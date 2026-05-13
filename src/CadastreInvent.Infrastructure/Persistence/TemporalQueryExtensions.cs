using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CadastreInvent.Infrastructure.Persistence.Extensions
{
    public static class TemporalQueryExtensions
    {
        public static IQueryable<TEntity> TemporalAsOf<TEntity>(this DbSet<TEntity> dbSet, DateTime date)
            where TEntity : class
        {
            var entityType = dbSet.EntityType;
            var schema = entityType.GetSchema() ?? "public";
            var tableName = entityType.GetTableName();
            var historyTableName = $"{tableName}_history";

            var utcDate = date.ToUniversalTime();

            var sql = $@"
                SELECT * FROM (
                    SELECT * FROM ""{schema}"".""{tableName}""
                    UNION ALL
                    SELECT * FROM ""{schema}"".""{historyTableName}""
                ) AS temporal_data
                WHERE ""ValidFrom"" <= {{0}} AND ""ValidTo"" > {{0}}";

            return dbSet.FromSqlRaw(sql, utcDate).IgnoreQueryFilters();
        }
    }
}