namespace Schwab.Enums
{
  public enum OrderTypeEnum : byte
  {
    MARKET,
    LIMIT,
    STOP,
    STOP_LIMIT,
    TRAILING_STOP,
    CABINET,
    NON_MARKETABLE,
    MARKET_ON_CLOSE,
    EXERCISE,
    TRAILING_STOP_LIMIT,
    NET_DEBIT,
    NET_CREDIT,
    NET_ZERO,
    LIMIT_ON_CLOSE,
  }
}
