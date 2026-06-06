using DataEntities;
using Insights.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Insights.Agents;

/// <summary>
/// Runs SentimentAgent and LanguageAgent concurrently using Microsoft Agent Framework (MAF)
/// workflows, replacing the previous Semantic Kernel ConcurrentOrchestration.
/// Each agent gets its own single-agent BuildSequential workflow; both run via Task.WhenAll
/// to preserve the concurrent execution semantics of the original design.
/// </summary>
public class Generator(
    ILogger<Generator> logger,
    [FromKeyedServices(SentimentAgent.AgentName)] AIAgent sentimentAgent,
    [FromKeyedServices(LanguageAgent.AgentName)] AIAgent languageAgent)
{
    public async Task<string> GenerateInsightAsync(string search, Context db)
    {
        // Build individual single-agent MAF workflows for concurrent execution
        var sentimentWorkflow = AgentWorkflowBuilder.BuildSequential("SentimentWorkflow", [sentimentAgent]);
        var languageWorkflow = AgentWorkflowBuilder.BuildSequential("LanguageWorkflow", [languageAgent]);

        var sentimentMessages = new List<ChatMessage> { new(ChatRole.User, search) };
        var languageMessages = new List<ChatMessage> { new(ChatRole.User, search) };

        // Run both agents concurrently using MAF InProcessExecution (returns ValueTask, so .AsTask())
        var sentimentRunTask = InProcessExecution.RunAsync(sentimentWorkflow, sentimentMessages).AsTask();
        var languageRunTask = InProcessExecution.RunAsync(languageWorkflow, languageMessages).AsTask();

        await Task.WhenAll(sentimentRunTask, languageRunTask);

        var sentimentResult = sentimentRunTask.Result.OutgoingEvents
            .OfType<AgentRunResponseEvent>()
            .LastOrDefault()?.Response.Text ?? "";
        var languageResult = languageRunTask.Result.OutgoingEvents
            .OfType<AgentRunResponseEvent>()
            .LastOrDefault()?.Response.Text ?? "";

        var analysisResult = TransformToAnalysis([sentimentResult, languageResult]);

        // add insight to the database
        var insight = new UserQuestionInsight
        {
            CreatedAt = DateTime.UtcNow,
            Question = search,
            Sentiment = analysisResult.Sentiment,
            Language = analysisResult.Language
        };
        db.UserQuestionInsight.Add(insight);
        await db.SaveChangesAsync();
        var sanitizedQuestion = insight.Question.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
        logger.LogInformation("Added insight: {sanitizedQuestion}", sanitizedQuestion);
        return "OK";
    }

    public Analysis TransformToAnalysis(string[] messages)
    {
        Analysis analysisResult = new();

        foreach (var message in messages)
        {
            if (message.Contains("sentiment", StringComparison.OrdinalIgnoreCase))
            {
                var parts = message.Split(':');
                if (parts.Length >= 2)
                {
                    var sentiment = parts[1].Trim();
                    analysisResult.Sentiment = Enum.TryParse<Sentiment>(sentiment, true, out var parsedSentiment) ? parsedSentiment : Sentiment.NotDefined;
                }
            }
            else if (message.Contains("language", StringComparison.OrdinalIgnoreCase))
            {
                var parts = message.Split(':');
                if (parts.Length >= 2)
                {
                    analysisResult.Language = parts[1].Trim();
                }
            }
        }

        return analysisResult;
    }
}

public class Analysis
{
    public Sentiment Sentiment { get; set; } = Sentiment.NotDefined;
    public string Language { get; set; } = string.Empty;
}
