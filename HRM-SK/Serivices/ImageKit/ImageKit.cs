using Imagekit.Sdk;

namespace HRM_SK.Serivices.ImageKit
{
    public class ImageKit
    {
        private readonly IConfigurationSection _imageKitConfigSection;
        private ImagekitClient imagekit;
        public ImageKit(IConfiguration configuration)
        {
            _imageKitConfigSection = configuration.GetSection("ImageKitSettings");
            imagekit = new ImagekitClient(
            publicKey: _imageKitConfigSection["publicKey"],
            privateKey: _imageKitConfigSection["privateKey"],
            urlEndPoint: _imageKitConfigSection["UrlEndpoint"]);
        }

        public async Task<byte[]> ConvertIFormFileToBytesAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task<Result> HandleNewFormFileUploadAsync(IFormFile file)
        {
            if (file is null) return null;
            try
            {
                var fileName = file.FileName;
                var fileExtension = Path.GetExtension(fileName);
                var fileInBytes = await this.ConvertIFormFileToBytesAsync(file);
                var response = await imagekit.UploadAsync(
                    new FileCreateRequest
                    {
                        file = fileInBytes,
                        fileName = $"{Guid.NewGuid().ToString()}{fileExtension}",
                        folder = "/HRMSK"
                    }
                );
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void BulkDelete(List<string> thumbnailUrl)
        {
            imagekit.BulkDeleteFiles(thumbnailUrl);
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var fileId = ExtractFileIdFromUrl(fileUrl);
                await imagekit.DeleteFileAsync("/HRMSK/" + fileId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }

        private string ExtractFileIdFromUrl(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            return uri.Segments.Last();
        }

        public async Task<Result> ReplaceFileAsync(string oldFileUrl, IFormFile newFile)
        {
            var deleteSuccess = await DeleteFileAsync(oldFileUrl);

            if (!deleteSuccess)
            {
                Console.WriteLine("Failed to delete the old file.");
            }

            var newFileUrl = await HandleNewFormFileUploadAsync(newFile);

            if (newFileUrl == null)
            {
                Console.WriteLine("Failed to upload the new file.");
            }

            return newFileUrl;
        }

    }
}

