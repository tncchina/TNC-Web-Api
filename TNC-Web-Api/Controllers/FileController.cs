using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;
using System.Web.Http.Cors;
using TNC_Web_Api.Models;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace TNC_Web_Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/storage")]
    public class FileController : ApiController
    {
        private const string AIEngineUrl = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/124a8097-fce4-4b00-9b4e-8a89c2d32d63/url?iterationId=152a2f03-5840-46b4-acfe-987a8342b47e";

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("photoUpload")]
        [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
        public async Task<ClassificationModel> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var fileName = "unknown";
            try
            {
                var file = provider.Contents[0];
                var nameArr = file.Headers.ContentDisposition.Name.Trim('\"').Split('\\');
                fileName = nameArr[nameArr.Length - 1];
                var buffer = await file.ReadAsByteArrayAsync();
                string filePath = "C:\\Users\\zhulian\\Projects\\TNC-Web-Api\\resources\\" + fileName;
                File.WriteAllBytes(filePath, buffer);
                Uri uri = this.UploadToAzure(fileName, buffer);
                string photoUrl = uri.ToString();

                using(var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Prediction-Key", "14311bb72e51406a85c43f351a91890b");
                    var values = new Dictionary<string, string>
                    {
                        { "Url", photoUrl }
                    };
                    var content = new FormUrlEncodedContent(values);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    var response = await client.PostAsync(AIEngineUrl, content);
                    string prediction = response.Content.ReadAsStringAsync().Result;
                    prediction = this.ParsePrediction(prediction);
                    return new ClassificationModel(fileName, uri.ToString(), prediction);
                }

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return new ClassificationModel(fileName, "Error: " + exc.Message, "Unknow");
            }
        }

        private const string PREDICTIONS = "$..Predictions";
        private const string TASK_ID = "$..TagId";
        private string ParsePrediction(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;
            string result = string.Empty;
            try
            {
                JToken token = JToken.Parse(content);
                if(token != null)
                {
                    List<JToken> tagTokens = token.SelectTokens(TASK_ID).ToList();
                    while(tagTokens != null && tagTokens.Count!=0)
                    {
                        tagTokens.First().Parent.Remove();
                        tagTokens = token.SelectTokens(TASK_ID).ToList();
                    }
                    JToken predictionMatch = token.SelectToken(PREDICTIONS);
                    return predictionMatch.ToString(Formatting.Indented);
                }
            }catch(Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
            }
            return content;
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
