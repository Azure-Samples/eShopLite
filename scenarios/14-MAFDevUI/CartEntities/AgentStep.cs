namespace CartEntities;

// DEMO: Represents a step in the agentic checkout pipeline
public class AgentStep
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Success, Warning, Error
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
