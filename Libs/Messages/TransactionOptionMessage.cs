namespace Schwab.Messages
{
  using Schwab.Enums;
  using System.Text.Json.Serialization;

  public partial class TransactionOptionMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("rootSymbol")]
    public string RootSymbol { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("strikePercent")]
    public long? StrikePercent { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("deliverableNumber")]
    public long? DeliverableNumber { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("deliverableUnits")]
    public long? DeliverableUnits { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("deliverable")]
    public TransactionDeliverableMessage Deliverable { get; set; }
  }
}
