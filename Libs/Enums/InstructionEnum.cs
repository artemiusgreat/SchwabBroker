namespace Schwab.Enums
{
  public enum InstructionEnum : byte
  {
    BUY,
    DEBIT,
    BUY_TO_OPEN,
    BUY_TO_CLOSE,
    NET_DEBIT,

    SELL,
    CREDIT,
    SELL_SHORT,
    SELL_TO_OPEN,
    SELL_TO_CLOSE,
    NET_CREDIT
  }
}
