using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.Azure;
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;
using System.Web.Http.Cors;

namespace TNC_Web_Api.Controllers
{
    [RoutePrefix("api/storage")]
    public class FileController : ApiController
    {


        [HttpPost]
        [Route("photoUpload")]
        [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            try
            {
                var file = provider.Contents[0];
                var nameArr = file.Headers.ContentDisposition.Name.Trim('\"').Split('\\');
                var fileName = nameArr[nameArr.Length - 1];
                var buffer = await file.ReadAsByteArrayAsync();
                string filePath = "C:\\Users\\zhulian\\Projects\\TNC-Web-Api\\resources\\" + fileName;
                File.WriteAllBytes(filePath, buffer);
                Uri uri = this.UploadToAzure(fileName, buffer);
                return Request.CreateResponse(HttpStatusCode.OK, "Upload succeed, photot azure link: " + uri.ToString());
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

        }

        const string STORAGE_ACCOUNT = "AZURE_STORAGE_CONNECTION_STRING";
        private Uri UploadToAzure(string filename, byte[] buffer)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable(STORAGE_ACCOUNT));
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

            // Create or overwrite the "myblob" blob with contents from a local file.
            using(MemoryStream ms = new MemoryStream(buffer))
                blockBlob.UploadFromStream(ms);
            return blockBlob.Uri;
        }

        const string PREDICTION_KEY = "14311bb72e51406a85c43f351a91890b";
        private void InvokeCognitive(string photoUrl)
        {
            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = PREDICTION_KEY };
            ImageUrl url = new ImageUrl(photoUrl);
            var result =   endpoint.PredictImageUrl(new Guid("152a2f03-5840-46b4-acfe-987a8342b47e"), url);
        }
    }
}
