using FS.Core.Infrastructure;

namespace FS.Core.Data.View
{
    /// <summary>
    /// 视图操作
    /// </summary>
    /// <typeparam name="TEntity">实体</typeparam>
    public sealed class ViewSet<TEntity> : DbReadSet<ViewSet<TEntity>, TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        private readonly ViewContext _context;
        protected override BaseQueueManger QueueManger { get { return _context.QueueManger; } }

        /// <summary>
        /// 禁止外部实例化
        /// </summary>
        private ViewSet() { }
        public ViewSet(ViewContext context)
        {
            _context = context;
            SetState = _context.ContextMap.GetState(this.GetType()).Value;
            Name = SetState.SetAtt.Name;
        }
    }
}
