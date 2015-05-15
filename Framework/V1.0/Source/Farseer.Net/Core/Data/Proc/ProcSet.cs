using System.Collections.Generic;
using FS.Mapping.Context;

namespace FS.Core.Data.Proc
{
    /// <summary>
    /// 存储过程操作
    /// </summary>
    /// <typeparam name="TEntity">实体</typeparam>
    public sealed class ProcSet<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        private readonly ProcContext _context;

        private ProcQueueManger QueueManger { get { return _context.QueueManger; } }
        private Queue Queue { get { return QueueManger.CreateQueue(_name, _map); } }

        /// <summary>
        /// 存储过程名
        /// </summary>
        private readonly string _name;
        /// <summary>
        /// 实体类映射
        /// </summary>
        private readonly FieldMap _map;

        /// <summary>
        /// 禁止外部实例化
        /// </summary>
        private ProcSet() { }
        public ProcSet(ProcContext context)
        {
            _context = context;
            _map = typeof(TEntity);
            var contextState = _context.ContextMap.GetState(this.GetType());
            _name = contextState.Value.SetAtt.Name;
        }

        /// <summary>
        /// 返回查询的值
        /// </summary>
        public T GetValue<T>(TEntity entity = null, T t = default(T))
        {
            // 加入委托
            QueueManger.Append(_name, _map, (queryQueue) => t = queryQueue.ExecuteValue(entity, t), true);
            return t;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        public void Execute(TEntity entity = null)
        {
            // 加入委托
            QueueManger.Append(_name, _map, (queryQueue) => queryQueue.Execute(entity), !_context.IsMergeCommand);
        }

        /// <summary>
        /// 返回单条记录
        /// </summary>
        public TEntity ToEntity(TEntity entity = null)
        {
            // 加入委托
            QueueManger.Append(_name, _map, (queryQueue) => entity = queryQueue.ExecuteInfo(entity), true);
            return entity;
        }

        /// <summary>
        /// 返回多条记录
        /// </summary>
        public List<TEntity> ToList(TEntity entity = null)
        {
            List<TEntity> lst = null;
            // 加入委托
            QueueManger.Append(_name, _map, (queryQueue) => lst = queryQueue.ExecuteList(entity), true);
            return lst;
        }
    }
}