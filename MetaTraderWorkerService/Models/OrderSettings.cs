namespace MetaTraderWorkerService.Models;

// TODO: Implement trading platform logic so we can open trades for different trading platforms.
public class OrderSettings
{
    public Guid Id { get; set; }
    public required string Key { get; set; } // E.g., "TradingGroupId"
    public required string Value { get; set; } // E.g., "TradingGroup" for specific group criteria
    public string? Description { get; set; } // Explanation of what the setting controls
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}