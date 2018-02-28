namespace Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Olive;
    using Olive.Entities;
    using Olive.Mvc;

    public class SharedActionsController : BaseController
    {
        [Route("error")]
        public async Task<ActionResult> Error() => await View("error");

        [Route("error/404")]
        public new async Task<ActionResult> NotFound() => await View("error-404");

        [HttpPost, Authorize, Route("upload")]
        public ActionResult UploadTempFile(IFormFile[] files)
        {
            // Note: This will prevent uploading of all unsafe files defined at Blob.UnsafeExtensions
            // If you need to allow them, then comment it out.
            if (Blob.HasUnsafeFileExtension(files[0].FileName))
                return Json(new { Error = "Invalid file extension." });

            var path = System.IO.Path.Combine(FileUploadService.GetFolder(Guid.NewGuid().ToString()).FullName,
                files[0].FileName.ToSafeFileName());

            if (path.Length >= 260)
                return Json(new { Error = "File name length is too long." });

            return Json(new FileUploadService().TempSaveUploadedFile(files[0]));
        }

        [HttpGet, Route("file")]
        public async Task<ActionResult> DownloadFile()
        {
            var path = Request.QueryString.ToString().TrimStart('?');
            var accessor = await FileAccessor.Create(path, User);
            if (!accessor.IsAllowed()) return new UnauthorizedResult();

            if (accessor.Blob.IsMedia())
                return await RangeFileContentResult.From(accessor.Blob);
            else return await File(accessor.Blob);


        }

        [Route("temp-file/{key}")]
        public Task<ActionResult> DownloadTempFile(string key) => TempFileService.Download(key);

        [Route("/login")]
        public async Task<ActionResult> Login() => Redirect(Microservice.Of("auth").Url());
    }
}