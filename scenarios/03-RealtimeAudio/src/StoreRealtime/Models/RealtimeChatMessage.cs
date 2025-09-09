using DataEntities;
using StoreRealtime.ContextManagers;

namespace StoreRealtime.Models;

public class RealtimeChatMessage
{    public string Message { get; set; } = string.Empty;
    public bool IsUser { get; set; } = false;
    public List<Product> Products { get; set; } = null;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
