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

        private ViewQueueManger QueueManger { get { return _context.QueueManger; } }
        protected override Queue Queue { get { return QueueManger.CreateQueue(Name, Map); } }

        /// <summary>
        /// 禁止外部实例化
        /// </summary>
        private ViewSet() { }
        public ViewSet(ViewContext context)
        {
            _context = context;
            Map = typeof(TEntity);
            var contextState = _context.ContextMap.GetState(this.GetType());
            Name = contextState.Value.SetAtt.Name;
        }
    }
}
