using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;

namespace surveyjs_aspnet_mvc
{
    public class BlobManager
    {
        private CloudBlobContainer blobContainer;

        public BlobManager(string ContainerName)
        {
            // Check if Container Name is null or empty  
            if (string.IsNullOrEmpty(ContainerName))
            {
                throw new ArgumentNullException("ContainerName", "Container Name can't be empty");
            }
            try
            {
                // Get azure table storage connection string.  
                string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=impowershell;AccountKey=Gx2iYTDMl1FEqOwADgdYFLBF+4pnTasjJJUu7BUzCMZXNof6tQasTmp8iTP7gwK4dckRxQLX7JS03Ok2kyUX1Q==;EndpointSuffix=core.windows.net";

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                // If the connection string is valid, proceed with operations against Blob
                // storage here.

                // Create the CloudBlobClient that represents the 
                // Blob storage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = cloudBlobClient.GetContainerReference(ContainerName);

                // Create the container and set the permission  
                if (blobContainer.CreateIfNotExists())
                {
                    //blobContainer.SetPermissions(
                    //    new BlobContainerPermissions
                    //    {
                    //        SharedAccessPolicies = 
                    //        PublicAccess = BlobContainerPublicAccessType.Blob
                    //    }
                    //);
                }
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }

        public string UploadTextToBlob(string blobName, string jsonString)
        {
            string AbsoluteUri;
            // Check HttpPostedFileBase is null or not  
            if (jsonString == null) //|| FileToUpload.ContentLength == 0
                return null;
            try
            {
                CloudBlockBlob blockBlob;
                // Create a block blob  
                blockBlob = blobContainer.GetBlockBlobReference(blobName);
                // Set the object's content type  
                //blockBlob.Properties.ContentType = FileToUpload.ContentType;
                // upload to blob  
                //blockBlob.UploadFromStream(FileToUpload.InputStream);
                blockBlob.UploadText(jsonString);

                // get file uri  
                AbsoluteUri = blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
            return AbsoluteUri;
        }


        public List<string> BlobList(string blobName = "")
        {
            List<string> _blobList = new List<string>();
            var result = blobContainer.GetDirectoryReference(blobName);
            foreach (IListBlobItem item in result.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob _blobpage = (CloudBlockBlob)item;
                    _blobList.Add(_blobpage.Uri.AbsoluteUri.ToString());
                }
            }
            return _blobList;
        }


        public Dictionary<string, string> GetBlobAsync(string blobPrefix = "")
        {
            try
            {

                Dictionary<string, string> surveys = new Dictionary<string, string>();
                // List the blobs in the container.
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    //var a = BlobList(blobName);
                    //var fileList = cloudFileShare.GetRootDirectoryReference().ListFilesAndDirectories();

                    //var resultss = await blobContainer.ListBlobsSegmentedAsync(blobName, blobContinuationToken);
                    //var results = await result.ListBlobsSegmentedAsync(blobContinuationToken);
                    // Get the value of the continuation token returned by the listing call.
                    //blobContinuationToken = result.ContinuationToken;

                    var result = blobContainer.GetDirectoryReference(blobPrefix);
                    List<string> blobNames = result.ListBlobs().OfType<CloudBlockBlob>().Select(b => b.Name).ToList();
                    foreach (string blobName in blobNames)
                    {
                        CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference(blobName);
                        var stringResult = cloudBlockBlob.DownloadText();
                        var key = blobName.Split("/").LastOrDefault();
                        surveys[key] = stringResult;
                    }
                } while (blobContinuationToken != null); // Loop while the continuation token is not null.

                return surveys;

            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }

        }
        public bool DeleteBlob(string blobPrefix, string blobName)
        {
            try
            {
                //Uri uriObj = new Uri(AbsoluteUri);
                //string BlobName = Path.GetFileName(uriObj.LocalPath);

                // get block blob Directory reference  
                var blobDirectory = blobContainer.GetDirectoryReference(blobPrefix);
                // get block blob refarence  
                CloudBlockBlob blockBlob = blobDirectory.GetBlockBlobReference(blobName);

                // delete blob from container      
                blockBlob.Delete();
                return true;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }
    }
}
