using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace hrm.Providers
{
    public class UploadFileProvider
    {
        private readonly Cloudinary _cloudinary;


        public UploadFileProvider(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> UploadImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (uploadResult.SecureUrl.AbsoluteUri + "," + uploadResult.PublicId);
            }

            return null;
        }



        //[HttpPost("delete-image/{id}")]
        //public async Task<IActionResult> DeleteImage(int id)
        //{
        //    var config = await _configRepository.GetConfigById(id);
        //    if (config == null || string.IsNullOrEmpty(config.LogoUrl))
        //    {
        //        return NotFound("Configuration or logo not found.");
        //    }
        //    var publicId = config.LogoUrl.Split('/').Last().Split('.').First();
        //    var deleteParams = new DeletionParams(publicId);
        //    var result = await _cloudinary.DestroyAsync(deleteParams);
        //    if (result.Result == "ok")
        //    {
        //        return Ok(new BaseResponse<string>("", "Delete image successfully", true));
        //    }
        //    return StatusCode(500, "Failed to delete image.");
        //}
    }
}
