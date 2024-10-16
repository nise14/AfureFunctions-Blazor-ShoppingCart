using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShoppingCartList.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace ShoppingCartList
{
    public class ShoppingCartApi
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public ShoppingCartApi(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer("ShoppingCartItems", "Items");
        }

        [FunctionName("GetContainers")]
        public async Task<IActionResult> GetContainers(
            [HttpTrigger(AuthorizationLevel.Anonymous,"get",Route ="containers")]
            HttpRequest req, ILogger log
        )
        {
            log.LogInformation("Get Containers");
            var database = _cosmosClient.GetDatabase("ShoppingCartItems");

            FeedIterator<ContainerProperties> iterator = database.GetContainerQueryIterator<ContainerProperties>();

            FeedResponse<ContainerProperties> containers = await iterator.ReadNextAsync().ConfigureAwait(false);

            return new OkObjectResult(containers.Select(x => x.Id).ToList());
        }

        [FunctionName("GetShoppingCartItems")]
        public async Task<IActionResult> GetShoppingCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shoppingcartitem")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting All shopping Car Items");

            List<ShoppingCartItem> shoppingCartItems = new();
            var items = _container.GetItemQueryIterator<ShoppingCartItem>();
            while (items.HasMoreResults)
            {
                var response = await items.ReadNextAsync();
                shoppingCartItems.AddRange(response.ToList());
            }

            return new OkObjectResult(shoppingCartItems);
        }

        [FunctionName("GetShoppingCartItemById")]
        public async Task<IActionResult> GetShoppingCartItemById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shoppingcartitem/{id}/{category}")]
            HttpRequest req,
            ILogger log, string id, string category)
        {
            log.LogInformation($"Getting Shopping Cart with ID: {id}");

            try
            {
                var item = await _container.ReadItemAsync<ShoppingCartItem>(id, new PartitionKey(category));
                return new OkObjectResult(item.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("CreateShoppingCartItem")]
        public async Task<IActionResult> CreateShoppingCartItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "shoppingcartitem")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating Shopping Cart Item");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CreateShoppingCartItem>(requestData);

            var item = new ShoppingCartItem
            {
                ItemName = data.ItemName,
                Category = data.Category
            };

            await _container.CreateItemAsync(item, new PartitionKey(item.Category));

            return new OkObjectResult(item);
        }

        [FunctionName("PutShoppingCartItem")]
        public async Task<IActionResult> PutShoppingCartItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "shoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id, string category)
        {
            log.LogInformation($"Updating Shopping Cart Item with ID: {id}");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UpdateShoppingCartItem>(requestData);

            var item = await _container.ReadItemAsync<ShoppingCartItem>(id, new PartitionKey(category));

            if (item.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }

            item.Resource.Collected = data.Collected;
            await _container.UpsertItemAsync(item.Resource);
            return new OkObjectResult(item.Resource);
        }

        [FunctionName("DeleteShoppingCartItem")]
        public async Task<IActionResult> DeleteShoppingCartItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "shoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id, string category)
        {
            log.LogInformation($"Deleting Shopping Cart Item with ID: {id}");

            await _container.DeleteItemAsync<ShoppingCartItem>(id, new PartitionKey(category));

            return new OkResult();
        }
    }
}
