using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using FS.Core.Infrastructure;
using FS.Extends;
using FS.Mapping.Context;

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

        private ViewQueueManger QueueManger { get { return (ViewQueueManger)_context.QueueManger; } }
        protected override IQueue Queue { get { return _context.QueueManger.GetQueue(Name, Map); } }

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
