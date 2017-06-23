using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using stark.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace stark.Web.Services
{
    public class AwsStorageService : BaseService, IStorageService
    {

        public string Upload(string originalFilename, Stream fileDataStream)
        {

            string bucketName = ConfigurationManager.AppSettings["BucketName"];
            Guid g = Guid.NewGuid();
            string keyName = "C34/" + g + originalFilename;

            using (AmazonS3Client client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSAccessKey"], ConfigurationManager.AppSettings["AWSSecretKey"], Amazon.RegionEndpoint.USWest2))
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                request.InputStream = fileDataStream;

                PutObjectResponse response = client.PutObject(request);
                               
                if ((int)response.HttpStatusCode > 400)
                {
                    throw new System.ArgumentException("AWS returned " + (int)response.HttpStatusCode + " status code");
                }
            }

            return HttpUtility.UrlPathEncode(bucketName + ".s3-us-west-2.amazonaws.com/" + keyName);

        }
    }
}