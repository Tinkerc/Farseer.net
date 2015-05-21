using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FS.Core.Data;
using FS.Core.Infrastructure;
using FS.Mapping.Context.Attribute;
using FS.Utils;

namespace FS.Core.Client.Common.SqlBuilder
{
    public class SqlOper : SqlQuery, IBuilderSqlOper
    {
        /// <summary>
        /// 查询支持的SQL方法
        /// </summary>
        /// <param name="queueManger">队列管理模块</param>
        /// <param name="queue">包含数据库SQL操作的队列</param>
        public SqlOper(BaseQueueManger queueManger, Queue queue) : base(queueManger, queue) { }

        public virtual Queue Delete()
        {
            Queue.Sql = new StringBuilder();
            var strWhereSql = Visit.Where(Queue.ExpWhere);

            if (!string.IsNullOrWhiteSpace(strWhereSql)) { strWhereSql = "WHERE " + strWhereSql; }

            Queue.Sql.AppendFormat("DELETE FROM {0} {1}", QueueManger.DbProvider.KeywordAegis(Queue.Name), strWhereSql);
            return Queue;
        }

        public virtual Queue Insert<TEntity>(TEntity entity) where TEntity : class,new()
        {
            Queue.Sql = new StringBuilder();
            var strinsertAssemble = Visit.Insert(entity);

            Queue.Sql.AppendFormat("INSERT INTO {0} {1}", Queue.Name, strinsertAssemble);
            return Queue;
        }

        public virtual Queue InsertIdentity<TEntity>(TEntity entity) where TEntity : class,new()
        {
            Queue.Sql = new StringBuilder();
            var strinsertAssemble = Visit.Insert(entity);
            Queue.Sql.AppendFormat("INSERT INTO {0} {1}", Queue.Name, strinsertAssemble);
            return Queue;
        }

        public virtual Queue Update<TEntity>(TEntity entity) where TEntity : class,new()
        {
            Queue.Sql = new StringBuilder();
            var strWhereSql = Visit.Where(Queue.ExpWhere);
            var strAssemble = Visit.Assign(entity);
            var readCondition = Visit.ReadCondition(entity);

            Check.NotEmpty(strAssemble, "更新操作时，当前实体没有要更新的字段。" + typeof (TEntity));

            // 主键如果有值、或者设置成只读条件，则自动转成条件
            if (!string.IsNullOrWhiteSpace(readCondition)) { strWhereSql += string.IsNullOrWhiteSpace(strWhereSql) ? readCondition : " AND " + readCondition; }
            if (!string.IsNullOrWhiteSpace(strWhereSql)) { strWhereSql = "WHERE " + strWhereSql; }

            Queue.Sql.AppendFormat("UPDATE {0} SET {1} {2}", QueueManger.DbProvider.KeywordAegis(Queue.Name), strAssemble, strWhereSql);
            return Queue;
        }

        public virtual Queue AddUp()
        {
            Check.IsTure(Queue.ExpAssign == null || Queue.ExpAssign.Count == 0, "赋值的参数不能为空！");

            Queue.Sql = new StringBuilder();
            var strWhereSql = Visit.Where(Queue.ExpWhere);

            if (!string.IsNullOrWhiteSpace(strWhereSql)) { strWhereSql = "WHERE " + strWhereSql; }

            #region 字段赋值
            var sqlAssign = new StringBuilder();
            foreach (var keyValue in Queue.ExpAssign)
            {
                var strAssemble = Visit.Assign(keyValue.Key);
                var strs = strAssemble.Split(',');
                foreach (var s in strs) { sqlAssign.AppendFormat("{0} = {0} + {1},", s, keyValue.Value); }
            }
            if (sqlAssign.Length > 0) { sqlAssign = sqlAssign.Remove(sqlAssign.Length - 1, 1); }
            #endregion

            Queue.Sql.AppendFormat("UPDATE {0} SET {1} {2}", QueueManger.DbProvider.KeywordAegis(Queue.Name), sqlAssign, strWhereSql);
            return Queue;
        }
    }
}