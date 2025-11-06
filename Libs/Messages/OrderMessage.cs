namespace Schwab.Messages
{
  using Schwab.Enums;
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  /// <summary>
  /// Standard order properties
  /// </summary>
  public class OrderProps
  {
    public virtual double? Price { get; set; }
    public virtual double? ActivationPrice { get; set; }
    public virtual OrderTypeEnum OrderType { get; set; } = OrderTypeEnum.MARKET;
    public virtual OrderSessionEnum Session { get; set; } = OrderSessionEnum.NORMAL;
    public virtual OrderDurationEnum Duration { get; set; } = OrderDurationEnum.DAY;
    public virtual OrderStrategyEnum Strategy { get; set; } = OrderStrategyEnum.SINGLE;
    public virtual List<OrderMessage> Orders { get; set; } = new();
    public virtual List<OrderLegMessage> Legs { get; set; } = new();
  }

  public partial class OrderMessage
  {
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("session")]
    public string Session { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orderType")]
    public string OrderType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("cancelTime")]
    public DateTime? CancelTime { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("complexOrderStrategyType")]
    public string ComplexOrderStrategyType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("quantity")]
    public double? Quantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("filledQuantity")]
    public double? FilledQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("remainingQuantity")]
    public double? RemainingQuantity { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("requestedDestination")]
    public string RequestedDestination { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("destinationLinkName")]
    public string DestinationLinkName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("releaseTime")]
    public DateTime? ReleaseTime { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stopPrice")]
    public double? StopPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stopPriceLinkBasis")]
    public string StopPriceLinkBasis { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stopPriceLinkType")]
    public string StopPriceLinkType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stopPriceOffset")]
    public double? StopPriceOffset { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("stopType")]
    public string StopType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("priceLinkBasis")]
    public string PriceLinkBasis { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("priceLinkType")]
    public string PriceLinkType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("price")]
    public double? Price { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("taxLotMethod")]
    public string TaxLotMethod { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orderLegCollection")]
    public List<OrderLegMessage> OrderLegCollection { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("activationPrice")]
    public double? ActivationPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("specialInstruction")]
    public string SpecialInstruction { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orderStrategyType")]
    public string OrderStrategyType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("cancelable")]
    public bool? Cancelable { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("editable")]
    public bool? Editable { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("enteredTime")]
    public DateTime? EnteredTime { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("closeTime")]
    public DateTime? CloseTime { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tag")]
    public string Tag { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("orderActivityCollection")]
    public List<OrderActivityMessage> OrderActivityCollection { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("replacingOrderCollection")]
    public List<OrderMessage> ReplacingOrderCollection { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("childOrderStrategies")]
    public List<OrderMessage> ChildOrderStrategies { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("statusDescription")]
    public string StatusDescription { get; set; }

    public OrderMessage() { }

    public OrderMessage(OrderProps o)
    {
      Session = Enum.GetName(o.Session);
      Duration = Enum.GetName(o.Duration);
      OrderType = Enum.GetName(o.OrderType);
      OrderStrategyType = Enum.GetName(o.Strategy);

      switch (o.OrderType)
      {
        case OrderTypeEnum.STOP: StopPrice = o.Price; break;
        case OrderTypeEnum.LIMIT: Price = o.Price; break;
        case OrderTypeEnum.STOP_LIMIT:
          StopPrice = o.ActivationPrice;
          Price = o.Price;
          break;
      }

      OrderLegCollection = o.Legs;
      ChildOrderStrategies = o.Orders;
    }
  }
}
