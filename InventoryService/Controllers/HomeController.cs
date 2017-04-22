using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using InventoryCommon;

namespace InventoryService.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpCommunicationClientFactory _clientFactory
    = new HttpCommunicationClientFactory(new ServicePartitionResolver(() => new FabricClient()));
        private readonly Uri _serviceUri = new Uri($"{FabricRuntime.GetActivationContext().ApplicationName}/InventoryRepository");

        [HttpGet]
        public async Task<IActionResult> Index(InventoryItemType? selectedItemType)
        {
            // If not item type is selected, just display a blank list
            if (selectedItemType == null)
            {
                return View(new ItemListViewModel { InventoryItems = new InventoryItem[] { } });
            }

            // An item type has been selected - retrieve its contents.
            var partitionKey = new ServicePartitionKey((Int64)selectedItemType);
            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(_clientFactory, _serviceUri, partitionKey);
            var items = await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var response = await client.HttpClient.GetAsync(new Uri($"{client.BaseUri}/api/inventory"));
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error - {response.StatusCode}: {response.ReasonPhrase}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var resultItems = JsonConvert.DeserializeObject<IEnumerable<InventoryItem>>(responseContent);

                // Note - filter applied client-side for demo purposes only.
                return resultItems.Where(x => x.ItemType == selectedItemType);

            }, CancellationToken.None);

            var viewModel = new ItemListViewModel
            {
                InventoryItems = items,
                SelectedItemType = selectedItemType
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddNewInventoryItem(InventoryItemType itemType)
        {
            var newItem = new InventoryItem { ItemType = itemType };
            return View(newItem);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewInventoryItem(InventoryItem newItem)
        {
            var partitionKey = new ServicePartitionKey((Int64)newItem.ItemType);
            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(_clientFactory, _serviceUri, partitionKey);
            var results = await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var newItemContent = new StringContent(JsonConvert.SerializeObject(newItem), Encoding.UTF8, "application/json");
                var response = await client.HttpClient.PostAsync(new Uri($"{client.BaseUri}/api/inventory"), newItemContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error - {response.StatusCode}: {response.ReasonPhrase}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<InventoryItem>(responseContent);

            }, CancellationToken.None);

            return RedirectToAction("Index", new { SelectedItemType = newItem.ItemType });
        }

        [HttpGet]
        public async Task<IActionResult> AddInventory(Guid itemId, InventoryItemType selectedItemType)
        {
            var partitionKey = new ServicePartitionKey((Int64)selectedItemType);
            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(_clientFactory, _serviceUri, partitionKey);
            var item = await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var response = await client.HttpClient.GetAsync(new Uri($"{client.BaseUri}/api/inventory/{itemId}"));
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error - {response.StatusCode}: {response.ReasonPhrase}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<InventoryItem>(responseContent);

            }, CancellationToken.None);

            var viewModel = new InventoryQuantityViewModel
            {
                ItemId = item.ItemId,
                ItemType = item.ItemType,
                Display = $"{item.Name} ({item.ItemType})",
                IsAdd = true,
                Quantity = 1
            };
            return View("UpdateInventoryQuantity", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddInventory(InventoryQuantityViewModel viewModel)
        {
            var partitionKey = new ServicePartitionKey((Int64)viewModel.ItemType);
            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(_clientFactory, _serviceUri, partitionKey);
            await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var uri = new Uri($"{client.BaseUri}/api/inventory/{viewModel.ItemId}/addinventory/{viewModel.Quantity}");
                var response = await client.HttpClient.PostAsync(uri, null);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error - {response.StatusCode}: {response.ReasonPhrase}");
                }

                return;
            }, CancellationToken.None);

            return RedirectToAction("Index", new { SelectedItemType = viewModel.ItemType });
        }

        [HttpGet]
        public async Task<IActionResult> RemoveInventory(Guid itemId, InventoryItemType selectedItemType)
        {
            var partitionKey = new ServicePartitionKey((Int64)selectedItemType);
            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(_clientFactory, _serviceUri, partitionKey);
            var item = await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var response = await client.HttpClient.GetAsync(new Uri($"{client.BaseUri}/api/inventory/{itemId}"));
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error - {response.StatusCode}: {response.ReasonPhrase}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<InventoryItem>(responseContent);

            }, CancellationToken.None);

            var viewModel = new InventoryQuantityViewModel
            {
                ItemId = item.ItemId,
                ItemType = item.ItemType,
                Display = $"{item.Name} ({item.ItemType})",
                IsAdd = false,
                Quantity = 1
            };
            return View("UpdateInventoryQuantity", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveInventory(InventoryQuantityViewModel viewModel)
        {
            var partitionKey = new ServicePartitionKey((Int64)viewModel.ItemType);
            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(_clientFactory, _serviceUri, partitionKey);
            await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var uri = new Uri($"{client.BaseUri}/api/inventory/{viewModel.ItemId}/removeinventory/{viewModel.Quantity}");
                var response = await client.HttpClient.PostAsync(uri, null);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error - {response.StatusCode}: {response.ReasonPhrase}");
                }

                return;
            }, CancellationToken.None);

            return RedirectToAction("Index", new { SelectedItemType = viewModel.ItemType });
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }

    public class ItemListViewModel
    {
        public IEnumerable<InventoryItem> InventoryItems { get; set; }
        public InventoryItemType? SelectedItemType { get; set; }
    }

    public class InventoryQuantityViewModel
    {
        public Guid ItemId { get; set; }
        public InventoryItemType ItemType { get; set; }
        public String Display { get; set; }
        public Boolean IsAdd { get; set; }
        public Int32 Quantity { get; set; }
    }
}
