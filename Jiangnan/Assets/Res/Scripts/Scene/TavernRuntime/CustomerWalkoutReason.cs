namespace JN.Client.Scene
{
    /// <summary>
    /// 顾客中途离场原因（成就统计与后续离场表现共用）。
    /// </summary>
    public enum CustomerWalkoutReason
    {
        None = 0,
        QueueTooLong = 1,
        ServeTooSlow = 2,
        VipNegative = 3,
        OrderTooLong = 4,
        CheckoutTooLong = 5,
    }
}
