using Schwab;
using Schwab.Enums;
using Schwab.Messages;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
  public class Program
  {
    static async Task Main(string[] args)
    {
      var cleaner = CancellationToken.None;
      var broker = new SchwabBroker
      {
      };

      broker.AccessToken = (await broker.Authenticate()).AccessToken;

      // Requests

      var maxDate = DateTime.Now;
      var minDate = DateTime.Now.AddDays(-10);
      var nextDate = DateTime.Now.AddDays(1);
      var account = await broker.GetAccountCode(cleaner);
      var accountCode = account?.FirstOrDefault()?.HashValue;
      var bars = await broker.GetBars(new() { Symbol = "SPY", EndDate = maxDate, StartDate = minDate }, cleaner);
      var options = await broker.GetOptions(new() { Symbol = "SPY", FromDate = nextDate, ToDate = nextDate }, cleaner);
      var summary = await broker.GetAccountSummary(new() { AccountCode = accountCode }, cleaner);
      var orders = await broker.GetOrders(new() { AccountCode = accountCode, FromEnteredTime = minDate, ToEnteredTime = maxDate }, cleaner);
      var positions = await broker.GetPositions(new() { AccountCode = accountCode }, cleaner);
      var actions = await broker.GetTransactions(new() { AccountCode = accountCode, Types = "TRADE", StartDate = minDate, EndDate = maxDate }, cleaner);

      // Subscriptions

      await broker.Stream(cleaner);
      await broker.SubscribeToDom("SPY", DomEnum.NYSE_BOOK, o => Console.WriteLine(JsonSerializer.Serialize(o)));
      await broker.Subscribe("SPY", SubscriptionEnum.LEVELONE_EQUITIES, o => Console.WriteLine(JsonSerializer.Serialize(o)));

      // OCO order

      var asset = new OrderLegMessage("F", 1, AssetTypeEnum.EQUITY, InstructionEnum.BUY);
      var ocoSL = new OrderMessage(new()
      {
        Price = 1,
        OrderType = OrderTypeEnum.STOP,
        Legs = [asset with { Instruction = nameof(InstructionEnum.SELL) }]
      });

      var ocoTP = new OrderMessage(new()
      {
        Price = 1,
        OrderType = OrderTypeEnum.LIMIT,
        Legs = [asset with { Instruction = nameof(InstructionEnum.SELL) }]
      });

      var ocoOrder = new OrderMessage(new()
      {
        Strategy = OrderStrategyEnum.TRIGGER,
        Orders = [ocoSL, ocoTP],
        Legs = [asset]
      });

      //var ocoResponse = await broker.SendOrder(ocoOrder, accountCode, cleaner);
      //var ocoStatus = await broker.ClearOrder(ocoResponse.OrderId, accountCode, cleaner);

      //// Option order

      //var optionOrder = new OrderMessage(new()
      //{
      //  Price = 0.05,
      //  Strategy = OrderStrategyEnum.SINGLE,
      //  OrderType = OrderTypeEnum.NET_DEBIT,
      //  Legs = [
      //    new("F251107C13", 1, AssetTypeEnum.OPTION, InstructionEnum.BUY_TO_OPEN),
      //    new("F251107C15", 1, AssetTypeEnum.OPTION, InstructionEnum.SELL_TO_OPEN)
      //  ]
      //});

      //var optionResponse = await broker.SendOrder(optionOrder, accountCode, cleaner);
      //var optionStatus = await broker.ClearOrder(optionResponse.OrderId, accountCode, cleaner);

      Console.ReadKey();

      await broker.Disconnect();
    }
  }
}
