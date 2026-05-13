using System;

namespace CadastreInvent.Registry.Domain.Entities
{
    public class ImportHistory
    {
        public Guid Id { get; set; }
        public DateTime ImportDateUtc { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int ImportedRows { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        protected ImportHistory() { }

        public ImportHistory(string fileName, int totalRows, int importedRows, Guid userId, string userName)
        {
            Id = Guid.NewGuid();
            ImportDateUtc = DateTime.UtcNow;
            FileName = fileName;
            TotalRows = totalRows;
            ImportedRows = importedRows;
            UserId = userId;
            UserName = userName;
        }
    }
}