using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

/*
 ================================================================
// Code Attribution

Author: Mick Gouweloos
Link: https://github.com/mickymouse777/Cloud_Storage
Date Accessed: 20 August 2024

Author: Mick Gouweloos
Link: https://github.com/mickymouse777/SimpleSample.git
Date Accessed: 20 September 2024

Author: W3schools
Link: https://www.w3schools.com/colors/colors_picker.asp
Date Accessed: 21 August 2024

Author: W3schools
Link: https://www.w3schools.com/css/css_font.asp 
Date Accessed: 21 August 2024

 *********All Images used throughout project are adapted from https://bangtanpictures.net/index.php and https://shop.weverse.io/en/home*************

 ================================================================
!--All PAGES are edited but layout depicted from Tooplate Template-
(https://www.tooplate.com/) 

 */
namespace CLDV6212_TableStorageFunction
{
    public class CLDV6212_TableStorageFunction
    {
        // Declare a private TableClient object to interact with Azure Table Storage
        private readonly TableClient _tableClient;

        // Declare a private logger field for logging important information
        private readonly ILogger<CLDV6212_TableStorageFunction> _logger;

        // Constructor that initializes the logger and the TableClient
        public CLDV6212_TableStorageFunction(ILogger<CLDV6212_TableStorageFunction> logger)
        {
            _logger = logger; // Initialize the logger with the provided logger instance

            // Retrieve the connection string for Azure Storage from environment variables
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Initialize a TableServiceClient using the connection string
            var serviceClient = new TableServiceClient(connectionString);

            // Get a reference to the "Products" table in Azure Table Storage
            _tableClient = serviceClient.GetTableClient("Products");

            // Ensure the "Products" table is created if it doesn't already exist
            _tableClient.CreateIfNotExists();
        }

        // The function that processes HTTP POST requests to store a product in Azure Table Storage
        [Function("st10252746TableStorageFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            // Log that the function has started processing a request
            _logger.LogInformation("st10252746TableStorageFunction processed a request for a product");

            // Read the incoming request body and convert it to a string
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize (convert) the JSON string from the request body into a Product object
            Product product = JsonConvert.DeserializeObject<Product>(requestBody);

            // Check if the deserialization was successful and the product object is not null
            if (product == null)
            {
                // If the product data is invalid, return a BadRequest response with an error message
                return new BadRequestObjectResult("Invalid product data.");
            }

            // If the product data is valid, add the product to the Azure Table Storage
            await _tableClient.AddEntityAsync(product); // Use the _tableClient to add the entity

            // Create a response message to confirm the product was added successfully
            string responseMessage = $"Product {product.Name} added successfully.";

            // Return an OK result with the response message
            return new OkObjectResult(responseMessage);
        }
    }

    // Define the Product class that implements ITableEntity to be used in Azure Table Storage
    public class Product : ITableEntity
    {
        // These properties are required for Table Storage: PartitionKey and RowKey
        public string PartitionKey { get; set; } // Used to organize data by category or group
        public string RowKey { get; set; } // Uniquely identifies each product within the partition
        public DateTimeOffset? Timestamp { get; set; } // Stores when the record was last updated
        public ETag ETag { get; set; } // Used for concurrency checks in Table Storage

        // Product-specific properties
        public string Name { get; set; } // Name of the product
        public string ProductDescription { get; set; } // Detailed description of the product
        public double Price { get; set; } // Price of the product
        public string Category { get; set; } // Category to which the product belongs (e.g., electronics, clothing)
        public string ImageUrlPath { get; set; } // URL of the product image stored in Blob Storage
    }
}
