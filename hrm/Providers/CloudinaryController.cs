using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace hrm.Providers
{
    public class CloudinaryController
    {
        private readonly Cloudinary _cloudinary;


        public CloudinaryController(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> UploadImageOrFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isImage = extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";

            UploadResult uploadResult;

            if (isImage)
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };
                uploadResult = await _cloudinary.UploadAsync(imageParams);
            }
            else
            {
                var rawParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };
                uploadResult = await _cloudinary.UploadAsync(rawParams);
            }

            if (uploadResult.StatusCode == HttpStatusCode.OK)
            {
                return $"{uploadResult.SecureUrl.AbsoluteUri},{uploadResult.PublicId}";
            }

            return null;
        }


        public async Task<bool> DeleteFileFromCloudinary(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);

            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok" || result.Result == "not found";
        }
    }
}
