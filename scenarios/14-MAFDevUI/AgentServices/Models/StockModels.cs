using CartEntities;

namespace AgentServices.Models;

/// <summary>
/// DEMO: Request model for the Stock Agent.
/// Contains cart items to validate stock availability.
/// </summary>
public class StockCheckRequest
{
    /// <summary>
    /// Cart items to check stock for.
    /// </summary>
    public IReadOnlyList<CartItem> Items { get; set; } = new List<CartItem>();

    /// <summary>
    /// Optional location ID to check stock at a specific location.
    /// If null, checks general availability across all locations.
    /// </summary>
    public int? LocationId { get; set; }
}

/// <summary>
/// DEMO: Response model from the Stock Agent.
/// Contains stock validation results and a summary.
/// </summary>
public class StockCheckResult
{
    /// <summary>
    /// Whether any items have stock issues.
    /// </summary>
    public bool HasStockIssues { get; set; }

    /// <summary>
    /// List of items with stock problems (if any).
    /// </summary>
    public List<StockIssue> Issues { get; set; } = new();

    /// <summary>
    /// Human-readable summary message (e.g., "All items are available" or "Item X reduced to 1 due to limited stock").
    /// </summary>
    public string SummaryMessage { get; set; } = string.Empty;

    /// <summary>
    /// Whether the stock check was successful.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// The location ID where stock was checked (if applicable).
    /// </summary>
    public int? CheckedLocationId { get; set; }

    /// <summary>
    /// The location name where stock was checked (if applicable).
    /// </summary>
    public string? CheckedLocationName { get; set; }
}

/// <summary>
/// Represents a stock issue for a specific item.
/// </summary>
public class StockIssue
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? LocationId { get; set; }
}
