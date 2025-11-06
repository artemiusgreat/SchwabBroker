using Flurl;
using Flurl.Http;
using Schwab.Enums;
using Schwab.Extensions;
using Schwab.Mappers;
using Schwab.Messages;
using Schwab.Queries;
using Schwab.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Schwab
{
  public class SchwabBroker : IDisposable
  {
    /// <summary>
    /// Request ID
    /// </summary>
    protected int counter;

    /// <summary>
    /// Converter
    /// </summary>
    protected MapService map = new();

    /// <summary>
    /// Disposable connections
    /// </summary>
    protected IList<IDisposable> connections;

    /// <summary>
    /// Order book action
    /// </summary>
    protected Action<DomMessage> onDom = o => { };

    /// <summary>
    /// Price action
    /// </summary>
    protected Action<PriceMessage> onPrice = o => { };

    /// <summary>
    /// Socket connection
    /// </summary>
    public virtual ClientWebSocket Streamer { get; protected set; }

    /// <summary>
    /// User preferences
    /// </summary>
    public virtual UserDataMessage UserData { get; protected set; }

    /// <summary>
    /// Data source
    /// </summary>
    public virtual string DataUri { get; set; }

    /// <summary>
    /// Streaming source
    /// </summary>
    public virtual string StreamUri { get; set; }

    /// <summary>
    /// Access token
    /// </summary>
    public virtual string AccessToken { get; set; }

    /// <summary>
    /// Refresh token
    /// </summary>
    public virtual string RefreshToken { get; set; }

    /// <summary>
    /// Client ID
    /// </summary>
    public virtual string ClientId { get; set; }

    /// <summary>
    /// Client secret
    /// </summary>
    public virtual string ClientSecret { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public SchwabBroker()
    {
      DataUri = "https://api.schwabapi.com";
      StreamUri = "wss://streamer-api.schwab.com/ws";

      counter = 0;
      connections = [];
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public virtual void Dispose() => Disconnect();

    /// <summary>
    /// Connect
    /// </summary>
    public virtual async Task<bool> Connect()
    {
      var interval = new System.Timers.Timer(TimeSpan.FromMinutes(1));

      await UpdateToken($"{DataUri}/v1/oauth/token");

      interval.Enabled = true;
      interval.Elapsed += async (sender, e) => await UpdateToken($"{DataUri}/v1/oauth/token");

      connections.Add(interval);

      return true;
    }

    /// <summary>
    /// Subscribe to data streams
    /// </summary>
    /// <param name="instrument"></param>
    /// <param name="assetType"></param>
    /// <param name="action"></param>
    public virtual async Task<bool> Subscribe(string instrument, SubscriptionEnum assetType, Action<PriceMessage> action)
    {
      var streamData = UserData.Streamer.FirstOrDefault();

      onPrice += action;

      await SendStream(new StreamInputMessage
      {
        Requestid = ++counter,
        Service = $"{assetType}",
        Command = "ADD",
        CustomerId = streamData.CustomerId,
        CorrelationId = $"{Guid.NewGuid()}",
        Parameters = new SrteamParamsMessage
        {
          Keys = instrument,
          Fields = string.Join(",", Enumerable.Range(0, 10))
        }
      });

      return true;
    }

    /// <summary>
    /// Subscribe to data streams
    /// </summary>
    /// <param name="instrument"></param>
    /// <param name="domType"></param>
    /// <param name="action"></param>
    public virtual async Task<bool> SubscribeToDom(string instrument, DomEnum domType, Action<DomMessage> action)
    {
      var streamData = UserData.Streamer.FirstOrDefault();

      onDom += action;

      await SendStream(new StreamInputMessage
      {
        Command = "ADD",
        Requestid = ++counter,
        Service = $"{domType}",
        CustomerId = streamData.CustomerId,
        CorrelationId = $"{Guid.NewGuid()}",
        Parameters = new SrteamParamsMessage
        {
          Keys = instrument,
          Fields = string.Join(",", Enumerable.Range(0, 3))
        }
      });

      return true;
    }

    /// <summary>
    /// Save state and dispose
    /// </summary>
    public virtual Task<bool> Disconnect()
    {
      connections?.ForEach(o => o?.Dispose());
      connections?.Clear();

      return Task.FromResult(true);
    }

    /// <summary>
    /// Sync open balance, order, and positions 
    /// </summary>
    /// <param name="cleaner"></param>
    public virtual async Task<AccountNumberMessage[]> GetAccountCode(CancellationToken cleaner)
    {
      var query = new SenderQuery
      {
        Cleaner = cleaner,
        Source = $"{DataUri}/trader/v1/accounts/accountNumbers"
      };

      return await Send<AccountNumberMessage[]>(query);
    }

    /// <summary>
    /// User preferences
    /// </summary>
    /// <param name="cleaner"></param>
    public virtual async Task<UserDataMessage> GetUserData(CancellationToken cleaner)
    {
      var query = new SenderQuery
      {
        Cleaner = cleaner,
        Source = $"{DataUri}/trader/v1/userPreference"
      };

      return await Send<UserDataMessage>(query);
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="accountCode"></param>
    /// <param name="cleaner"></param>
    public virtual async Task<OrderMessage> ClearOrder(string orderId, string accountCode, CancellationToken cleaner)
    {
      var query = new SenderQuery
      {
        Cleaner = cleaner,
        Action = HttpMethod.Delete,
        Source = $"{DataUri}/trader/v1/accounts/{accountCode}/orders/{orderId}"
      };

      return await Send<OrderMessage>(query);
    }

    /// <summary>
    /// Get options
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="cleaner"></param>
    public virtual async Task<OptionChainMessage> GetOptions(ChainQuery criteria, CancellationToken cleaner)
    {
      var props = new Hashtable();

      Op(criteria?.Symbol, o => props["symbol"] = o);
      Op(criteria?.ExpMonth, o => props["expMonth"] = o);
      Op(criteria?.Interval, o => props["interval"] = o);
      Op(criteria?.Strike, o => props["strike"] = o);
      Op(criteria?.ToDate, o => props["toDate"] = $"{o:yyyy-MM-dd}");
      Op(criteria?.FromDate, o => props["fromDate"] = $"{o:yyyy-MM-dd}");
      Op(criteria?.StrikeCount, o => props["strikeCount"] = o);

      var query = new SenderQuery
      {
        Cleaner = cleaner,
        Source = $"{DataUri}/marketdata/v1/chains".SetQueryParams(props)
      };

      return await Send<OptionChainMessage>(query);
    }

    /// <summary>
    /// Get historical ticks
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="cleaner"></param>
    public virtual async Task<BarsMessage> GetBars(HistoryQuery criteria, CancellationToken cleaner)
    {
      var props = new Hashtable();

      Op(criteria?.Symbol, o => props["symbol"] = o);
      Op(criteria?.Period, o => props["period"] = o);
      Op(criteria?.PeriodType, o => props["periodType"] = o);
      Op(criteria?.Frequency, o => props["frequency"] = o);
      Op(criteria?.FrequencyType, o => props["frequencyType"] = o);
      Op(criteria?.EndDate, o => props["endDate"] = new DateTimeOffset(criteria.EndDate).ToUnixTimeMilliseconds());
      Op(criteria?.StartDate, o => props["startDate"] = new DateTimeOffset(criteria.StartDate).ToUnixTimeMilliseconds());

      var query = new SenderQuery
      {
        Cleaner = cleaner,
        Source = $"{DataUri}/marketdata/v1/pricehistory".SetQueryParams(props)
      };

      return await Send<BarsMessage>(query);
    }

    /// <summary>
    /// Create order
    /// </summary>
    /// <param name="order"></param>
    /// <param name="accountCode"></param>
    /// <param name="cleaner"></param>
    public virtual async Task<OrderMessage> SendOrder(OrderMessage order, string accountCode, CancellationToken cleaner)
    {
      var query = new SenderQuery
      {
        Headers = new(),
        Content = order,
        Cleaner = cleaner,
        Action = HttpMethod.Post,
        Source = $"{DataUri}/trader/v1/accounts/{accountCode}/orders"
      };

      var response = await Send<OrderMessage>(query);

      if (query.Headers.Get("Location").FirstOrDefault() is string res and not null)
      {
        order.OrderId = $"{res[(res.LastIndexOf('/') + 1)..]}";
      }

      return response;
    }

    /// <summary>
    /// Sync open balance, order, and positions 
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="cleaner"></param>
    public virtual async Task<AccountsMessage> GetAccountSummary(AccountQuery criteria, CancellationToken cleaner)
    {
      var props = new Hashtable();

      Op(criteria?.Fields, o => props["fields"] = string.Join(",", criteria.Fields));

      var query = new SenderQuery
      {
        Cleaner = cleaner,
        Source = $"{DataUri}/trader/v1/accounts/{criteria.AccountCode}".SetQueryParams(props)
      };

      return await Send<AccountsMessage>(query);
    }

    /// <summary>
    /// Get orders
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="cleaner"></param>
    public virtual async Task<IList<OrderMessage>> GetOrders(OrderQuery criteria, CancellationToken cleaner)
    {
      var props = new Hashtable();

      Op(criteria?.MaxResults, o => props["maxResults"] = o);
      Op(criteria?.ToEnteredTime, o => props["toEnteredTime"] = $"{o:yyyy-MM-ddTHH:mm:ss.fffZ}");
      Op(criteria?.FromEnteredTime, o => props["fromEnteredTime"] = $"{o:yyyy-MM-ddTHH:mm:ss.fffZ}");
      Op(criteria?.Status, o => props["status"] = o);

      var query = new SenderQuery
      {
        Cleaner = cleaner,
        Source = $"{DataUri}/trader/v1/accounts/{criteria.AccountCode}/orders".SetQueryParams(props)
      };

      return await Send<OrderMessage[]>(query);
    }

    /// <summary>
    /// Get positions 
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="cleaner"></param>
    public virtual async Task<IList<PositionMessage>> GetPositions(AccountQuery criteria, CancellationToken cleaner)
    {
      criteria.Fields ??= ["positions"];

      return (await GetAccountSummary(criteria, cleaner))?.SecuritiesAccount?.Positions;
    }

    /// <summary>
    /// Send data to the API
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    public virtual async Task<T> Send<T>(SenderQuery query)
    {
      var message = $"{new UriBuilder(query.Source)}"
        .WithHeader("Accept", "application/json")
        .WithHeader("Authorization", $"Bearer {AccessToken}");

      var data = null as StringContent;

      if (query.Content is not null)
      {
        data = new StringContent(JsonSerializer.Serialize(query.Content, map.Options), Encoding.UTF8, "application/json");
      }

      var response = await message
        .SendAsync(query.Action ?? HttpMethod.Get, data, HttpCompletionOption.ResponseContentRead, CancellationToken.None)
        .ConfigureAwait(false);

      foreach (var o in query.Headers ?? [])
      {
        query.Headers[o.Key] = response.ResponseMessage.Headers.TryGetValues(o.Key, out var v) ? v : null;
      }

      var responseContent = await response
        .ResponseMessage
        .Content
        .ReadAsStringAsync()
        .ConfigureAwait(false);

      return JsonSerializer.Deserialize<T>(responseContent, map.Options);
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    public virtual async Task<ScopeMessage> UpdateToken(string source)
    {
      var message = $"{new UriBuilder(source)}"
        .WithHeader("Accept", "application/json")
        .WithHeader("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"))}");

      var content = new Hashtable
      {
        ["grant_type"] = "refresh_token",
        ["refresh_token"] = RefreshToken
      };

      var response = await message
        .PostUrlEncodedAsync(content)
        .ConfigureAwait(false);

      if (response.ResponseMessage.IsSuccessStatusCode is false)
      {
        throw new HttpRequestException(response.ResponseMessage.ReasonPhrase, null, response.ResponseMessage.StatusCode);
      }

      var responseContent = await response
        .ResponseMessage
        .Content
        .ReadAsStringAsync()
        .ConfigureAwait(false);

      var scope = JsonSerializer.Deserialize<ScopeMessage>(responseContent, map.Options);

      if (scope is not null)
      {
        AccessToken = scope.AccessToken;
        RefreshToken = scope.RefreshToken;
      }

      return scope;
    }

    /// <summary>
    /// Send data to web socket stream
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cleaner"></param>
    public virtual Task SendStream(object data, CancellationTokenSource cleaner = null)
    {
      var content = JsonSerializer.Serialize(data, map.Options);
      var message = Encoding.UTF8.GetBytes(content);

      return Streamer.SendAsync(
        message,
        WebSocketMessageType.Text,
        true,
        cleaner?.Token ?? CancellationToken.None);
    }

    /// <summary>
    /// Read response from web socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cleaner"></param>
    public virtual async Task<T> ReceiveStream<T>(CancellationToken cleaner)
    {
      var data = new byte[short.MaxValue];
      var response = await Streamer.ReceiveAsync(data, cleaner);
      var message = Encoding.UTF8.GetString(data, 0, response.Count);

      return JsonSerializer.Deserialize<T>(message, map.Options);
    }

    /// <summary>
    /// Web socket stream
    /// </summary>
    /// <param name="cleaner"></param>
    public virtual async Task<ClientWebSocket> ConnectStream(CancellationToken cleaner)
    {
      Streamer = new ClientWebSocket();
      UserData = await GetUserData(cleaner);

      var source = new UriBuilder(StreamUri);
      var streamData = UserData.Streamer.FirstOrDefault();

      await Streamer.ConnectAsync(source.Uri, cleaner);
      await SendStream(new StreamInputMessage
      {
        Service = "ADMIN",
        Command = "LOGIN",
        Requestid = ++counter,
        CustomerId = streamData.CustomerId,
        CorrelationId = $"{Guid.NewGuid()}",
        Parameters = new StreamLoginMessage
        {
          Channel = streamData.Channel,
          FunctionId = streamData.FunctionId,
          Authorization = AccessToken
        }
      });

      var data = new byte[short.MaxValue];
      var adminResponse = await ReceiveStream<StreamLoginResponseMessage>(cleaner);
      var adminCode = adminResponse.Response.FirstOrDefault().Content.Code;
      var process = new Thread(async () =>
      {
        while (Streamer.State is WebSocketState.Open)
        {
          var streamResponse = await Streamer.ReceiveAsync(new ArraySegment<byte>(data), cleaner);
          var content = Encoding.UTF8.GetString(data, 0, streamResponse.Count);
          var message = JsonNode.Parse(content);

          if (message["data"] is not null)
          {
            var streamItems = message["data"]
              .AsArray()
              .Select(o => o.Deserialize<StreamDataMessage>());

            var points = streamItems
              .Where(o => Enum.IsDefined(typeof(SubscriptionEnum), o.Service))
              .ToList();

            var doms = streamItems
              .Where(o => Enum.IsDefined(typeof(DomEnum), o.Service))
              .ToList();

            if (points.Count is not 0)
            {
              OnPriceMessage(points, content);
            }

            if (doms.Count is not 0)
            {
              OnDomMessage(doms);
            }
          }
        }
      });

      process.Start();
      connections.Add(Streamer);

      return Streamer;
    }

    /// <summary>
    /// Process quote from the stream
    /// </summary>
    /// <param name="items"></param>
    protected virtual void OnPriceMessage(IEnumerable<StreamDataMessage> items, string content)
    {
      foreach (var item in items)
      {
        var map = GetStreamMap(item.Service);

        foreach (var data in item.Content)
        {
          var price = new PriceMessage();

          price.Time = DateTime.Now.Ticks;
          price.Name = $"{data.Get("key")}";
          price.Bid = Parse($"{data.Get(map.Get("Bid Price"))}", price.Bid);
          price.Ask = Parse($"{data.Get(map.Get("Ask Price"))}", price.Ask);
          price.BidSize = Parse($"{data.Get(map.Get("Bid Size"))}", price.BidSize);
          price.AskSize = Parse($"{data.Get(map.Get("Ask Size"))}", price.AskSize);
          price.Last = Parse($"{data.Get(map.Get("Last Price"))}", price.Last);
          price.Last = price.Last is 0 or null ? price.Bid ?? price.Ask : price.Last;
          price.Volume = Parse($"{data.Get(map.Get("Last Size"))}", price.Volume);

          if (price.Bid is null || price.Ask is null)
          {
            continue;
          }

          onPrice(price);
        }
      }
    }

    /// <summary>
    /// Process quote from the stream
    /// </summary>
    /// <param name="items"></param>
    protected virtual void OnDomMessage(IEnumerable<StreamDataMessage> items)
    {
      foreach (var item in items)
      {
        var map = StreamDomMap.Map;

        foreach (var data in item.Content)
        {
          var dom = new DomMessage();
          var bids = data.Get(map.Get("Bid Side Levels"));
          var asks = data.Get(map.Get("Ask Side Levels"));

          dom.Bids = [.. bids.AsArray().Select(node =>
          {
            var o = node.AsObject();
            var point = new PriceMessage
            {
              Last = o.TryGetPropertyValue("0", out var price) ? double.Parse($"{price}") : 0,
              Volume = o.TryGetPropertyValue("1", out var volume) ? double.Parse($"{volume}") : 0
            };

            return point;

          }).OrderBy(o => o.Last)];

          dom.Asks = [.. asks.AsArray().Select(node =>
          {
            var o = node.AsObject();
            var point = new PriceMessage
            {
              Last = o.TryGetPropertyValue("0", out var price) ? double.Parse($"{price}") : 0,
              Volume = o.TryGetPropertyValue("1", out var volume) ? double.Parse($"{volume}") : 0
            };

            return point;

          }).OrderBy(o => o.Last)];

          onDom(dom);
        }
      }
    }

    /// <summary>
    /// Params
    /// </summary>
    /// <param name="value"></param>
    /// <param name="action"></param>
    protected virtual void Op(object value, Action<object> action)
    {
      if (value is not null)
      {
        action(value);
      }
    }

    /// <summary>
    /// Parse
    /// </summary>
    /// <param name="value"></param>
    /// <param name="action"></param>
    protected virtual double? Parse(string o, double? origin)
    {
      return double.TryParse(o, out var num) ? num : origin;
    }

    /// <summary>
    /// Map fields in the stream
    /// </summary>
    /// <param name="assetType"></param>
    protected virtual IDictionary<string, string> GetStreamMap(string assetType)
    {
      switch (assetType?.ToUpper())
      {
        case nameof(SubscriptionEnum.LEVELONE_FOREX): return StreamCurrencyMap.Map;
        case nameof(SubscriptionEnum.LEVELONE_OPTIONS): return StreamOptionMap.Map;
        case nameof(SubscriptionEnum.LEVELONE_EQUITIES): return StreamEquityMap.Map;
        case nameof(SubscriptionEnum.LEVELONE_FUTURES): return StreamFutureMap.Map;
        case nameof(SubscriptionEnum.LEVELONE_FUTURES_OPTIONS): return StreamFutureOptionMap.Map;
      }

      return null;
    }
  }
}
