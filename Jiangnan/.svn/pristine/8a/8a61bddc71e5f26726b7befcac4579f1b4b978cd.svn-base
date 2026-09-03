namespace JN.Client.Scene
{
    /// <summary>
    /// 顾客分配任务基类。
    /// </summary>
    public abstract class CustomerAssignmentTask : CharacterTaskBase
    {
    }

    internal sealed class QueueCustomerAssignmentTask : CustomerAssignmentTask
    {
        public override string TaskKey => "Queue";
    }

    internal sealed class SeatCustomerAssignmentTask : CustomerAssignmentTask
    {
        public override string TaskKey => "Seat";
    }

    internal sealed class LeaveCustomerAssignmentTask : CustomerAssignmentTask
    {
        public override string TaskKey => "Leave";
    }
}
