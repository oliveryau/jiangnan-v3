namespace JN.Client.Scene
{
    internal abstract class CustomerStateBase : CharacterStateBase<CustomerCharacter>
    {
    }

    internal sealed class CustomerSpawningState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.Spawning;
    }

    internal sealed class CustomerQueueingState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.Queueing;
    }

    internal sealed class CustomerMovingToTableState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.MovingToTable;
    }

    internal sealed class CustomerSeatedState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.Seated;
    }

    internal sealed class CustomerWaitingOrderState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.WaitingOrder;
    }

    internal sealed class CustomerDiningState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.Dining;
    }

    internal sealed class CustomerReadyCheckoutState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.ReadyCheckout;
    }

    internal sealed class CustomerLeavingState : CustomerStateBase
    {
        public override string StateKey => CustomerStateKeys.Leaving;
    }
}
