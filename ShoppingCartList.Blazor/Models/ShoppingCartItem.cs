namespace ShoppingCartList.Blazor.Models;

internal class ShoppingCartItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Created { get; set; } = DateTime.Now;
    public string ItemName { get; set; } = null!;
    public bool Collected { get; set; }
    public string Category { get; set; } = null!;
}