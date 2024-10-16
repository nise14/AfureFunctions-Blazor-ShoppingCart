using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace ShoppingCartList.Models;

internal class ShoppingCartItem : TableEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Created { get; set; } = DateTime.Now;
    public string ItemName { get; set; } = null!;
    public bool Collected { get; set; }
    [JsonProperty("category")]
    public string Category { get; set; }
}