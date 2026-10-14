using Contracts;
using Microsoft.AspNetCore.Components.AI;

namespace Store.Components;

[ToolBlock("search_products")]
public partial class ProductSearchBlock : FunctionInvocationContentBlock
{
    [ToolParameter(Name = "query")]
    public string? Query { get; set; }

    [ToolResult]
    public ProductSearchResult? SearchResult { get; set; }
}

[ToolBlock("get_product")]
public partial class ProductDetailsBlock : FunctionInvocationContentBlock
{
    [ToolResult]
    public Product? Product { get; set; }
}
