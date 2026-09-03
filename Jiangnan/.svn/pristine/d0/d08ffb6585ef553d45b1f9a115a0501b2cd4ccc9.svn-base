namespace JN.Client.Scene
{
    /// <summary>
    /// 厨师任务基类。
    /// </summary>
    public abstract class ChefTask : CharacterTaskBase
    {
        protected ChefTask(int tableId = -1)
        {
            TableId = tableId;
        }

        public int TableId { get; }
    }

    internal sealed class CookDishTask : ChefTask
    {
        public CookDishTask(int tableId = -1) : base(tableId)
        {
        }

        public override string TaskKey => "CookDish";
    }

    internal sealed class PlateDishTask : ChefTask
    {
        public PlateDishTask(int tableId = -1) : base(tableId)
        {
        }

        public override string TaskKey => "PlateDish";
    }

    /// <summary>
    /// 厨师任务派发服务。
    /// </summary>
    internal sealed class TavernChefDispatchService
    {
        public bool TryDispatchChefTask(IChefRuntimeHost host, CookDishTask task)
        {
            if (host == null || task == null)
            {
                return false;
            }

            var chef = host.GetAvailableChefForTask(task);
            if (chef == null)
            {
                return false;
            }

            return host.TryStartChefTask(chef, task, new ChefCookingState());
        }
    }
}
