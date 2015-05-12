using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FS.Core.Infrastructure;
using FS.Utils;

namespace FS.Core.Data.Table
{
    /// <summary>
    /// 表操作
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class TableSet<TEntity> : DbWriteSet<TableSet<TEntity>, TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        private readonly TableContext _context;
        private TableQueueManger QueueManger { get { return (TableQueueManger)_context.QueueManger; } }
        protected override IQueue Queue { get { return _context.QueueManger.GetQueue(Name, Map); } }

        /// <summary>
        /// 禁止外部实例化
        /// </summary>
        private TableSet() { }
        public TableSet(TableContext context)
        {
            _context = context;
            Map = typeof(TEntity);
            SetState = _context.ContextMap.GetState(this.GetType()).Value;
            Name = SetState.SetAtt.Name;
        }

        #region Copy

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="acTEntity">对新职的赋值</param>
        public void Copy(Action<TEntity> acTEntity = null)
        {
            var lst = ToList();
            foreach (var info in lst)
            {
                if (acTEntity != null) acTEntity(info);
                Insert(info);
            }
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="act">对新职的赋值</param>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="ID">o => o.ID.Equals(ID)</param>
        public void Copy<T>(int? ID, Action<TEntity> act = null)
        {
            Where<T>(o => o.ID.Equals(ID));
            Copy(act);
        }

        /// <summary>
        ///     复制数据
        /// </summary>
        /// <param name="act">对新职的赋值</param>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="lstIDs">o => IDs.Contains(o.ID)</param>
        public void Copy<T>(List<T> lstIDs, Action<TEntity> act = null)
        {
            Where<T>(o => lstIDs.Contains((T)o.ID));
            Copy(act);
        }

        #endregion

        #region Update

        /// <summary>
        /// 修改（支持延迟加载）
        /// 如果设置了主键ID，并且entity的ID设置了值，那么会自动将ID的值转换成条件 entity.ID == 值
        /// </summary>
        /// <param name="entity"></param>
        public TEntity Update(TEntity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity", "更新操作时，参数不能为空！"); }

            //  判断是否启用合并提交

            //Queue.LazyQuery((queue) => queue.SqlBuilder.Update(entity));

            if (_context.IsMergeCommand)
            {
                Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.Update(entity);
                QueueManger.Append();
            }
            else
            {
                Queue.SqlBuilder.Update(entity).Execute();
            }
            return entity;
        }

        /// <summary>
        ///     更改实体类
        /// </summary>
        /// <param name="info">实体类</param>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="ID">条件，等同于：o=>o.ID == ID 的操作</param>
        public TEntity Update<T>(TEntity info, T ID)
        {
            return Where<T>(o => o.ID.Equals(ID)).Update(info);
        }

        /// <summary>
        ///     更改实体类
        /// </summary>
        /// <param name="info">实体类</param>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="lstIDs">条件，等同于：o=> IDs.Contains(o.ID) 的操作</param>
        public TEntity Update<T>(TEntity info, List<T> lstIDs)
        {
            return Where<T>(o => lstIDs.Contains(o.ID)).Update(info);
        }

        /// <summary>
        ///     更改实体类
        /// </summary>
        /// <param name="info">实体类</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="where">查询条件</param>
        public TEntity Update(TEntity info, Expression<Func<TEntity, bool>> where)
        {
            return Where(where).Update(info);
        }
        #endregion

        #region Insert
        /// <summary>
        /// 插入（支持延迟加载）
        /// </summary>
        /// <param name="entity"></param>
        public TEntity Insert(TEntity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity", "插入操作时，参数不能为空！"); }
            //  判断是否启用合并提交
            if (_context.IsMergeCommand)
            {
                Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.Insert(entity);
                QueueManger.Append();
            }
            else
            {
                Queue.SqlBuilder.Insert(entity).Execute();
            }
            return entity;
        }
        /// <summary>
        /// 插入（不支持延迟加载）
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <param name="identity">返回新增的</param>
        public TEntity Insert(TEntity entity, out int identity)
        {
            if (entity == null) { throw new ArgumentNullException("entity", "插入操作时，参数不能为空！"); }

            identity =Queue.SqlBuilder.InsertIdentity(entity).ExecuteQuery<int>();

            return entity;
        }
        /// <summary>
        /// 插入（不支持延迟加载）
        /// </summary>
        /// <param name="lst"></param>
        public List<TEntity> Insert(List<TEntity> lst)
        {
            if (lst == null) { throw new ArgumentNullException("lst", "插入操作时，lst参数不能为空！"); }

            // 如果是MSSQLSER，则启用BulkCopy
            if (QueueManger.DataBase.DataType == Data.DataBaseType.SqlServer)
            {
                QueueManger.DataBase.ExecuteSqlBulkCopy(Name, ConvertHelper.ToTable(lst));
                return lst;
            }
            lst.ForEach(entity =>
            {
                Queue.SqlBuilder.Insert(entity).Execute();
            });
            return lst;
        }
        #endregion

        #region Delete
        /// <summary>
        /// 删除（支持延迟加载）
        /// </summary>
        public void Delete()
        {
            //  判断是否启用合并提交
            if (_context.IsMergeCommand)
            {
                Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.Delete();
                QueueManger.Append();
            }
            else
            {
                Queue.SqlBuilder.Delete().Execute();
            }
        }
        /// <summary>
        ///     删除数据
        /// </summary>
        /// <param name="ID">条件，等同于：o=>o.ID.Equals(ID) 的操作</param>
        /// <typeparam name="T">ID</typeparam>
        public void Delete<T>(int? ID)
        {
            Where<T>(o => o.ID.Equals(ID)).Delete();
        }

        /// <summary>
        ///     删除数据
        /// </summary>
        /// <param name="lstIDs">条件，等同于：o=> IDs.Contains(o.ID) 的操作</param>
        /// <typeparam name="T">ID</typeparam>
        public void Delete<T>(List<T> lstIDs)
        {
            Where<T>(o => lstIDs.Contains(o.ID)).Delete();
        }
        #endregion

        #region AddUp
        /// <summary>
        /// 添加或者减少某个字段（支持延迟加载）
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValue">要+=的值</param>
        public void AddUp<T>(Expression<Func<TEntity, T>> fieldName, T fieldValue) where T : struct
        {
            Append(fieldName, fieldValue).AddUp();
        }

        /// <summary>
        /// 添加或者减少某个字段（支持延迟加载）
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldValue">要+=的值</param>
        public void AddUp<T>(Expression<Func<TEntity, T?>> fieldName, T fieldValue)
            where T : struct
        {
            Append(fieldName, fieldValue).AddUp();
        }
        /// <summary>
        /// 添加或者减少某个字段（支持延迟加载）
        /// </summary>
        public void AddUp()
        {
            if (Queue.ExpAssign == null) { throw new ArgumentNullException("ExpAssign", "+=字段操作时，必须先执行AddUp的另一个重载版本！"); }

            //  判断是否启用合并提交
            if (_context.IsMergeCommand)
            {
                Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.AddUp();
                QueueManger.Append();
            }
            else
            {
                Queue.SqlBuilder.AddUp().Execute();
            }
        }

        /// <summary>
        ///     更新单个字段值
        /// </summary>
        /// <typeparam name="T">更新的值类型</typeparam>
        /// <param name="select"></param>
        /// <param name="fieldValue">要更新的值</param>
        /// <param name="ID">o => o.ID.Equals(ID)</param>
        public void AddUp<T>(int? ID, Expression<Func<TEntity, T>> select, T fieldValue)
            where T : struct
        {
            Where<T>(o => o.ID.Equals(ID)).AddUp(select, fieldValue);
        }

        /// <summary>
        ///     更新单个字段值
        /// </summary>
        /// <typeparam name="T">更新的值类型</typeparam>
        /// <param name="select"></param>
        /// <param name="fieldValue">要更新的值</param>
        /// <param name="ID">o => o.ID.Equals(ID)</param>
        public void AddUp<T>(int? ID, Expression<Func<TEntity, T?>> select, T fieldValue)
            where T : struct
        {
            Where<T>(o => o.ID.Equals(ID)).AddUp(select, fieldValue);
        }

        /// <summary>
        ///     更新单个字段值
        /// </summary>
        /// <param name="fieldValue">要更新的值</param>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="ID">o => o.ID.Equals(ID)</param>
        public void AddUp<T>(int? ID, T fieldValue)
            where T : struct
        {
            AddUp<T>(ID, (Expression<Func<TEntity, T>>)null, fieldValue);
        }
        #endregion
    }
}
