namespace JN.Client.Scene
{
    /// <summary>
    /// 小二任务基类。
    /// </summary>
    public abstract class WaiterTask : CharacterTaskBase
    {
        protected WaiterTask(int tableId)
        {
            TableId = tableId;
        }

        public int TableId { get; }
    }

    internal sealed class WaiterOrderTask : WaiterTask
    {
        public WaiterOrderTask(int tableId) : base(tableId)
        {
        }

        public override string TaskKey => "Order";
    }

    internal sealed class WaiterServeTask : WaiterTask
    {
        public WaiterServeTask(int tableId) : base(tableId)
        {
        }

        public override string TaskKey => "Serve";
    }

    internal sealed class WaiterCleanTask : WaiterTask
    {
        public WaiterCleanTask(int tableId) : base(tableId)
        {
        }

        public override string TaskKey => "Clean";
    }

    internal sealed class WaiterCheckoutTask : WaiterTask
    {
        public WaiterCheckoutTask(int tableId) : base(tableId)
        {
        }

        public override string TaskKey => "Checkout";
    }
}
