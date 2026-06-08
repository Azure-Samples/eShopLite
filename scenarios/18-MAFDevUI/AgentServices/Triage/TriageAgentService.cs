namespace AgentServices.Triage;

public class TriageAgentService
{
    public const string AgentName = "TriageAgent";

    public const string AgentInstructions =
        "You determine which agent to use based on the user's request. " +
        "For stock availability questions, handoff to stock_agent. " +
        "For discount calculations, handoff to discount_agent. " +
        "ALWAYS handoff to another agent.";
}
