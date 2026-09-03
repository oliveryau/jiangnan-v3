namespace JN.Client.Scene
{
    /// <summary>
    /// 顾客等待 HUD 所表示的服务阶段。
    /// </summary>
    public enum CustomerWaitHudState
    {
        None = 0,
        Queue = 1,
        WaitingOrder = 2,
        WaitingServe = 3,
        WaitingCheckout = 4,
    }
}
