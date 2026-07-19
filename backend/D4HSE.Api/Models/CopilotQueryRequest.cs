using System.ComponentModel.DataAnnotations;

namespace D4HSE.Api.Models;

/// <summary>
/// Request body for the AI Copilot query endpoint.
/// </summary>
public class CopilotQueryRequest
{
    /// <summary>
    /// The natural language question about HSE barriers, incidents, near misses, or maintenance.
    /// </summary>
    [Required]
    [MinLength(3)]
    [MaxLength(2000)]
    public string Query { get; set; } = string.Empty;
}
