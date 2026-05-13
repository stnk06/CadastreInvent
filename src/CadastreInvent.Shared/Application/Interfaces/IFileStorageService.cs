using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CadastreInvent.Shared.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);
        Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken);
        Task<string> GeneratePreSignedUrlAsync(string fileName, string contentType, CancellationToken cancellationToken);
    }
}