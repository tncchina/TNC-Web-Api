using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;

namespace TNC_Web_Api.Controllers
{
    [RoutePrefix("api/storage")]
    public class FileController : ApiController
    {
        [HttpPost]
        [Route("photoUpload")]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            try
            {
                var file = provider.Contents[0];
                var nameArr = file.Headers.ContentDisposition.FileName.Trim('\"').Split('\\');
                var fileName = nameArr[nameArr.Length - 1];
                var buffer = await file.ReadAsByteArrayAsync();
                string filePath = "D:\\Projects\\TNC-Web-Api\\TNC-Web-Api\\Content\\photos\\" + fileName;
                File.WriteAllBytes(filePath, buffer);
                return Ok();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

        }
    }
}
