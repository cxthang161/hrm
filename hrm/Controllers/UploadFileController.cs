using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using hrm.Common;
using hrm.Respository.Configs;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    public class UploadFileController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;
        private readonly IConfigRespository _configRepository;


        public UploadFileController(IConfigRespository configRepository, Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
            _configRepository = configRepository;
        }

        [HttpPost("upload-image/{id}")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await _configRepository.UploadLogo(id, uploadResult.SecureUrl.AbsoluteUri);
                if (result == null)
                {
                    return NotFound("Configuration not found.");
                }
                return Ok(new BaseResponse<string>("", "Upload file successfully", true));
            }

            return StatusCode(500, "Failed to upload image.");
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
