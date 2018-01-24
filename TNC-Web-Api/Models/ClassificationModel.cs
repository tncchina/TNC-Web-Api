using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TNC_Web_Api.Models
{
    public class ClassificationModel
    {
        public ClassificationModel(string photoName, string url)
        {
            this.PhotoName = photoName;
            this.PhotoUrl = url;
        }
        public string PhotoUrl { get; set; }

        public string PhotoName { get; set; }
    }
}