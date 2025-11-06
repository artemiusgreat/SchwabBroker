namespace Schwab.Messages
{
  using Schwab.Enums;
  using System;
  using System.Text.Json.Serialization;

  public record OrderLegMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orderLegType")]
    public string OrderLegType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("legId")]
    public string LegId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("instrument")]
    public InstrumentMessage Instrument { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("instruction")]
    public string Instruction { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("positionEffect")]
    public string PositionEffect { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("quantity")]
    public double? Quantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("quantityType")]
    public string QuantityType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("divCapGains")]
    public string DivCapGains { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("toSymbol")]
    public string ToSymbol { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public OrderLegMessage() { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="amount"></param>
    /// <param name="assetType"></param>
    /// <param name="instruction"></param>
    public OrderLegMessage(string name, double amount, AssetTypeEnum assetType, InstructionEnum instruction)
    {
      var instrument = new InstrumentMessage
      {
        Symbol = name,
        AssetType = Enum.GetName(assetType)
      };

      Quantity = amount;
      Instrument = instrument;
      Instruction = Enum.GetName(instruction);
    }
  }
}
