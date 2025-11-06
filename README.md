# Schwab API wrapper

Minimalistic async wrapper around Schwab Trader API.

# Status 

![GitHub Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/Indemos/Terminal/dotnet.yml?event=push)
![GitHub](https://img.shields.io/github/license/Indemos/Terminal)
![GitHub](https://img.shields.io/badge/system-Windows%20%7C%20Linux%20%7C%20Mac-blue)

# Nuget 

`dotnet add package SchwabBroker --version 0.0.1`

# Usage 

```C#

var cleaner = CancellationToken.None;
var broker = new SchwabBroker
{
  ClientId = "",
  ClientSecret = "",
  AccessToken = "",
  RefreshToken = ""
};

await broker.Connect();
await broker.ConnectStream(cleaner);

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

// Subscriptions

broker.OnDom += o => Console.WriteLine(JsonSerializer.Serialize(o));
broker.OnPrice += o => Console.WriteLine(JsonSerializer.Serialize(o));

await broker.SubscribeToDom("SPY", DomEnum.NYSE_BOOK);
await broker.Subscribe("SPY", SubscriptionEnum.LEVELONE_EQUITIES);

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

var ocoResponse = await broker.SendOrder(ocoOrder, accountCode, cleaner);
var ocoStatus = await broker.ClearOrder(ocoResponse.OrderId, accountCode, cleaner);

// Option order

var optionOrder = new OrderMessage(new()
{
  Price = 0.05,
  Strategy = OrderStrategyEnum.SINGLE,
  OrderType = OrderTypeEnum.NET_DEBIT,
  Legs = [
    new("F251107C13", 1, AssetTypeEnum.OPTION, InstructionEnum.BUY_TO_OPEN),
    new("F251107C15", 1, AssetTypeEnum.OPTION, InstructionEnum.SELL_TO_OPEN)
  ]
});

var optionResponse = await broker.SendOrder(optionOrder, accountCode, cleaner);
var optionStatus = await broker.ClearOrder(optionResponse.OrderId, accountCode, cleaner);

Console.ReadKey();

await broker.Disconnect();


```