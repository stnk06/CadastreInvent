using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Infrastructure.Services
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3FileStorageService(IOptions<S3Settings> settings)
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = settings.Value.UseHttp ? $"http://{settings.Value.Endpoint}" : $"https://{settings.Value.Endpoint}",
                ForcePathStyle = true,
                UseHttp = settings.Value.UseHttp,
                AuthenticationRegion = "us-east-1" 
            };

            _s3Client = new AmazonS3Client(settings.Value.AccessKey, settings.Value.SecretKey, s3Config);
            _bucketName = settings.Value.BucketName;
        }

        private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName }, cancellationToken);
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyExists" || ex.ErrorCode == "BucketAlreadyOwnedByYou")
            {
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
        {
            await EnsureBucketExistsAsync(cancellationToken);

            var fileKey = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileKey,
                BucketName = _bucketName,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest, cancellationToken);

            return $"/{_bucketName}/{fileKey}";
        }

        public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            var uri = new Uri(fileUrl, UriKind.RelativeOrAbsolute);
            var key = uri.IsAbsoluteUri ? uri.AbsolutePath.TrimStart('/') : fileUrl.TrimStart('/');

            if (key.StartsWith($"{_bucketName}/"))
            {
                key = key.Substring(_bucketName.Length + 1);
            }

            await _s3Client.DeleteObjectAsync(_bucketName, key, cancellationToken);
        }

        public async Task<string> GeneratePreSignedUrlAsync(string fileName, string contentType, CancellationToken cancellationToken)
        {
            await EnsureBucketExistsAsync(cancellationToken);

            var fileKey = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(2),
                ContentType = contentType
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}