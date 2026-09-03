namespace JN.Client.Scene
{
    /// <summary>
    /// Tavern 角色状态键常量。
    /// </summary>
    internal static class WaiterStateKeys
    {
        public const string Idle = "Idle";
        public const string MoveToTableForOrder = "MoveToTableForOrder";
        public const string Ordering = "Ordering";
        public const string MoveToNotifyChef = "MoveToNotifyChef";
        public const string CookStealing = "CookStealing";
        public const string MoveToPickupDish = "MoveToPickupDish";
        public const string MoveToServeTable = "MoveToServeTable";
        public const string Serving = "Serving";
        public const string MoveToTableForCheckout = "MoveToTableForCheckout";
        public const string Checkouting = "Checkouting";
        public const string Stealing = "Stealing";
        public const string MoveToCleanTable = "MoveToCleanTable";
        public const string Cleaning = "Cleaning";
        public const string Napping = "Napping";
        public const string ReturningHome = "ReturningHome";
        public const string MoveToAttractPoint = "MoveToAttractPoint";
        public const string Attracting = "Attracting";
    }

    internal static class ChefStateKeys
    {
        public const string Idle = "Idle";
        public const string Blocked = "Blocked";
        public const string Cooking = "Cooking";
        public const string Napping = "Napping";
        public const string ReturningHome = "ReturningHome";
    }

    internal static class CustomerStateKeys
    {
        public const string Spawning = "Spawning";
        public const string Queueing = "Queueing";
        public const string MovingToTable = "MovingToTable";
        public const string Seated = "Seated";
        public const string WaitingOrder = "WaitingOrder";
        public const string Dining = "Dining";
        public const string ReadyCheckout = "ReadyCheckout";
        public const string Leaving = "Leaving";
    }
}
