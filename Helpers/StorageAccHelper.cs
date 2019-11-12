using CatalogAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogAPI.Helpers
{
    public class StorageAccHelper
    {
        public string storageConnectionString;
        public string tableConnectionString;
        private CloudStorageAccount storageAccount;
        private CloudStorageAccount tableStorageAccount;
        private CloudBlobClient cloudBlobClient;
        private CloudTableClient cloudTableClient;

        public string StorageConnectionString
        {
            get { return storageConnectionString; }
            set
            {
                this.storageConnectionString = value;
                // Storage account object creation static method.
                storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);
            }
        }

        public string TableConnectionString
        {
            get { return tableConnectionString; }
            set
            {
                this.tableConnectionString = value;
                // Storage table account object creation static method.
                tableStorageAccount = CloudStorageAccount.Parse(this.tableConnectionString);
            }
        }

        public async Task<string> UploadFileToBlobAsync(string filePath, string containerName)
        {
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var container = cloudBlobClient.GetContainerReference(containerName);
            BlobContainerPermissions containerPermissions = new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Container
            };
            await container.SetPermissionsAsync(containerPermissions);

            var fileName = Path.GetFileName(filePath);
            var blob = container.GetBlockBlobReference(fileName);
            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(filePath);

            return blob.Uri.AbsoluteUri;
        }

        public async Task<CatalogEntity> SaveToAzureTableAsync(CatalogItem item)
        {
            CatalogEntity catalogEntity = new CatalogEntity(item.Name, item.Id)
            {
                Price = item.Price,
                Quantity =item.Quantity,
                ReorderLevel = item.ReorderLevel,
                ManufacturingDate = item.ManufacturingDate,
                ImageUrl = item.ImageUrl
            };
            //cloudTableClient = storageAccount.CreateCloudTableClient();
            cloudTableClient = tableStorageAccount.CreateCloudTableClient();
            var catalogTable = cloudTableClient.GetTableReference("catalog");
            await catalogTable.CreateIfNotExistsAsync();
            TableOperation operation = TableOperation.InsertOrMerge(catalogEntity);
            var tableResult = await catalogTable.ExecuteAsync(operation);

            return tableResult.Result as CatalogEntity;
        }
    }
}
