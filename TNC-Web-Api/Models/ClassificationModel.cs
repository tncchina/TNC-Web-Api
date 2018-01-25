using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TNC_Web_Api.Models
{
    public class ClassificationModel
    {
        public ClassificationModel(string photoName, string url, string prediction)
        {
            this.PhotoName = photoName;
            this.PhotoUrl = url;
            this.Prediction = prediction;
        }
        public string PhotoUrl { get; set; }

        public string PhotoName { get; set; }

        public string Prediction { get; set; }
    }
}