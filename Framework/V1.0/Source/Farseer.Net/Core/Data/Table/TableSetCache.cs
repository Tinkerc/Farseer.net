using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FS.Core.Infrastructure;
using FS.Extend;

namespace FS.Core.Data.Table
{
    public abstract class TableSetCache<TEntity> :  where TEntity : class, new()
    {
        private readonly TableSet<TEntity> _set;
        private List<TEntity> _lstCurrentCache;

        /// <summary>
        /// 禁止外部实例化
        /// </summary>
        private TableSetCache() { }
        public TableSetCache(TableContext context)
        {
            _set = new TableSet<TEntity>(context);

            // 缓存
            if (_set._setState.SetAtt.IsCache)
            {
                _lstCurrentCache = CacheManger.GetSetCache<TEntity>(_set._setState, () =>
                {
                    _set.Queue.SqlBuilder.ToList();
                    return _set.QueueManger.ExecuteTable(_set.Queue).ToList<TEntity>();
                });
            }
        }

        #region 条件
        /// <summary>
        ///     查询条件
        /// </summary>
        /// <param name="where">查询条件</param>
        public TableSetCache<TEntity> Where(Expression<Func<TEntity, bool>> where)
        {
            _set.Where(where);
            return this;
        }

        /// <summary>
        ///     查询条件
        /// </summary>
        private List<TEntity> WhereCache()
        {
            var lst = CacheManger.GetSetCache<TEntity>(_set._setState);
            if (_set.Queue.ExpWhere != null)
            {
                lst = lst.Where(((Expression<Func<TEntity, bool>>)_set.Queue.ExpWhere).Compile()).ToList();
                _set.QueueManger.Clear();
            }
            return lst;
        }

        /// <summary>
        /// 字段累加（字段 = 字段 + 值）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="fieldName">字段选择器</param>
        /// <param name="fieldValue">值</param>
        public TableSetCache<TEntity> Append<T>(Expression<Func<TEntity, T>> fieldName, T fieldValue) where T : struct
        {
            _set.Append(fieldName, fieldValue);
            return this;
        }

        /// <summary>
        /// 字段累加（字段 = 字段 + 值）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="fieldName">字段选择器</param>
        /// <param name="fieldValue">值</param>
        public TableSetCache<TEntity> Append<T>(Expression<Func<TEntity, T?>> fieldName, T fieldValue) where T : struct
        {
            _set.Append(fieldName, fieldValue);
            return this;
        }

        /// <summary>
        /// 字段累加（字段 = 字段 + 值）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="fieldName">字段选择器</param>
        /// <param name="fieldValue">值</param>
        public TableSetCache<TEntity> Append<T>(Expression<Func<TEntity, object>> fieldName, T fieldValue) where T : struct
        {
            _set.Append(fieldName, fieldValue);
            return this;
        }
        #endregion

        #region ToList
        /// <summary>
        /// 查询多条记录（不支持延迟加载）
        /// </summary>
        /// <param name="top">限制显示的数量</param>
        /// <param name="isDistinct">返回当前条件下非重复数据</param>
        public List<TEntity> ToList(int top = 0, bool isDistinct = false)
        {
            return isDistinct ? WhereCache().Distinct().ToList() : WhereCache();
        }

        /// <summary>
        /// 查询多条记录（不支持延迟加载）
        /// </summary>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="pageIndex">分页索引</param>
        /// <param name="isDistinct">返回当前条件下非重复数据</param>
        /// <returns></returns>
        public List<TEntity> ToList(int pageSize, int pageIndex, bool isDistinct = false)
        {
            return isDistinct ? WhereCache().Distinct().ToList(pageSize, pageIndex) : WhereCache().ToList(pageSize, pageIndex);
        }

        /// <summary>
        /// 查询多条记录（不支持延迟加载）
        /// </summary>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="pageIndex">分页索引</param>
        /// <param name="recordCount">总记录数量</param>
        /// <param name="isDistinct">返回当前条件下非重复数据</param>
        public List<TEntity> ToList(int pageSize, int pageIndex, out int recordCount, bool isDistinct = false)
        {
            var lst = WhereCache();
            recordCount = lst.Count;
            return isDistinct ? lst.Distinct().ToList(pageSize, pageIndex) : lst.ToList(pageSize, pageIndex);
        }

        /// <summary>
        ///     获取分页、Top、全部的数据方法(根据pageSize、pageIndex自动识别使用场景)
        /// </summary>
        /// <param name="lstIDs">条件，等同于：o=> IDs.Contains(o.ID) 的操作</param>
        /// <typeparam name="T">ID</typeparam>
        public List<TEntity> ToList<T>(List<T> lstIDs)
        {
            _set.Where<T>(o => lstIDs.Contains(o.ID));
            return WhereCache();
        }
        #endregion

        #region ToSelectList
        /// <summary>
        ///     返回筛选后的列表
        /// </summary>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <typeparam name="T">实体类的属性</typeparam>
        /// <param name="select">字段选择器</param>
        public List<T> ToSelectList<T>(Func<TEntity, T> select)
        {
            return ToSelectList(0, select);
        }

        /// <summary>
        ///     返回筛选后的列表
        /// </summary>
        /// <param name="top">限制显示的数量</param>
        /// <param name="select">字段选择器</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <typeparam name="T">实体类的属性</typeparam>
        public List<T> ToSelectList<T>(int top, Func<TEntity, T> select)
        {
            return WhereCache().Select(select).Take(top).ToList();
        }

        /// <summary>
        ///     返回筛选后的列表
        /// </summary>
        /// <param name="select">字段选择器</param>
        /// <param name="lstIDs">o => IDs.Contains(o.ID)</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <typeparam name="T">实体类的属性</typeparam>
        public List<T> ToSelectList<T>(List<T> lstIDs, Func<TEntity, T> select)
        {
            _set.Where<T>(o => lstIDs.Contains(o.ID));
            return ToSelectList(select);
        }

        /// <summary>
        ///     返回筛选后的列表
        /// </summary>
        /// <param name="select">字段选择器</param>
        /// <param name="lstIDs">o => IDs.Contains(o.ID)</param>
        /// <param name="top">限制显示的数量</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <typeparam name="T">实体类的属性</typeparam>
        public List<T> ToSelectList<T>(List<T> lstIDs, int top, Func<TEntity, T> select)
        {
            _set.Where<T>(o => lstIDs.Contains(o.ID));
            return ToSelectList(top, select);
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// 获取单条记录
        /// </summary>
        public TEntity ToEntity()
        {
            return WhereCache().FirstOrDefault();
        }

        /// <summary>
        ///     获取单条记录
        /// </summary>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="ID">条件，等同于：o=>o.ID.Equals(ID) 的操作</param>
        public TEntity ToEntity<T>(T ID)
        {
            _set.Where<T>(o => o.ID.Equals(ID)).ToEntity();
        }
        #endregion

        #region Count

        /// <summary>
        /// 查询数量（不支持延迟加载）
        /// </summary>
        public int Count(bool isDistinct = false)
        {
            return ToList(0, isDistinct).Count;
        }

        /// <summary>
        ///     获取数量
        /// </summary>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="lstIDs">条件，等同于：o=> IDs.Contains(o.ID) 的操作</param>
        public int Count<T>(List<T> lstIDs)
        {
            return ToList(lstIDs).Count;
        }

        #endregion

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
            _set.Where<T>(o => o.ID.Equals(ID));
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
            _set.Where<T>(o => lstIDs.Contains(o.ID));
            Copy(act);
        }
        #endregion

        #region IsHaving
        /// <summary>
        /// 查询数据是否存在（不支持延迟加载）
        /// </summary>
        public bool IsHaving()
        {
            return Count() > 0;
        }

        /// <summary>
        ///     判断是否存在记录
        /// </summary>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="ID">条件，等同于：o=>o.ID == ID 的操作</param>
        public bool IsHaving<T>(T ID)
        {
            return _set.Where<T>(o => o.ID.Equals(ID)).Count() > 0;
        }

        /// <summary>
        ///     判断是否存在记录
        /// </summary>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="lstIDs">条件，等同于：o=> IDs.Contains(o.ID) 的操作</param>
        public bool IsHaving<T>(List<T> lstIDs)
        {
            return Count(lstIDs) > 0;
        }
        #endregion

        #region Update

        /// <summary>
        /// 修改（支持延迟加载）
        /// </summary>
        /// <param name="entity"></param>
        public TEntity Update(TEntity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity", "更新操作时，参数不能为空！"); }

            //  判断是否启用合并提交
            if (_set._context.IsMergeCommand)
            {
                _set.Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.Update(entity);
                _set.QueueManger.Append();
            }
            else
            {
                _set.Queue.SqlBuilder.Update(entity);
                _set.QueueManger.Execute(_set.Queue);
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
            _set.Where<T>(o => o.ID.Equals(ID));
            return Update(info);
        }

        /// <summary>
        ///     更改实体类
        /// </summary>
        /// <param name="info">实体类</param>
        /// <typeparam name="T">ID</typeparam>
        /// <param name="lstIDs">条件，等同于：o=> IDs.Contains(o.ID) 的操作</param>
        public TEntity Update<T>(TEntity info, List<T> lstIDs)
        {
            _set.Where<T>(o => lstIDs.Contains(o.ID));
            return Update(info);
        }

        /// <summary>
        ///     更改实体类
        /// </summary>
        /// <param name="info">实体类</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="where">查询条件</param>
        public TEntity Update(TEntity info, Expression<Func<TEntity, bool>> where)
        {
            _set.Where(where);
            return Update(info);
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
            if (_set._context.IsMergeCommand)
            {
                _set.Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.Insert(entity);
                _set.QueueManger.Append();
            }
            else
            {
                _set.Queue.SqlBuilder.Insert(entity);
                _set.QueueManger.Execute(_set.Queue);
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

            _set.Queue.SqlBuilder.InsertIdentity(entity);
            identity = _set.QueueManger.ExecuteQuery<int>(_set.Queue);

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
            if (_set.QueueManger.DataBase.DataType == Data.DataBaseType.SqlServer)
            {
                _set.QueueManger.DataBase.ExecuteSqlBulkCopy(_name, lst.ToTable());
                return lst;
            }
            lst.ForEach(entity =>
            {
                _set.Queue.SqlBuilder.Insert(entity);
                _set.QueueManger.Execute(_set.Queue);
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
            if (_set._context.IsMergeCommand)
            {
                _set.Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.Delete();
                _set.QueueManger.Append();
            }
            else
            {
                _set.Queue.SqlBuilder.Delete();
                _set.QueueManger.Execute(_set.Queue);
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
            if (_set.Queue.ExpAssign == null) { throw new ArgumentNullException("ExpAssign", "+=字段操作时，必须先执行AddUp的另一个重载版本！"); }

            //  判断是否启用合并提交
            if (_set._context.IsMergeCommand)
            {
                _set.Queue.LazyAct = (queryQueue) => queryQueue.SqlBuilder.AddUp();
                _set.QueueManger.Append();
            }
            else
            {
                _set.Queue.SqlBuilder.AddUp();
                _set.QueueManger.Execute(_set.Queue);
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

        #region GetValue
        /// <summary>
        /// 查询单个值（不支持延迟加载）
        /// </summary>
        /// <param name="fieldName">筛选字段</param>
        /// <param name="defValue">不存在时默认值</param>
        public T GetValue<T>(Expression<Func<TEntity, T>> fieldName, T defValue = default(T))
        {
            if (fieldName == null) { throw new ArgumentNullException("fieldName", "查询Value操作时，fieldName参数不能为空！"); }
            Select(fieldName);

            _set.Queue.SqlBuilder.GetValue();
            return _set.QueueManger.ExecuteQuery(Queue, defValue);
#pragma warning(" Warning: version lower than visual studio 2005 ");


        }

        /// <summary>
        ///     获取数量
        /// </summary>
        /// <typeparam name="T1">ID</typeparam>
        /// <typeparam name="T2">字段类型</typeparam>
        /// <param name="ID">条件，等同于：o=>o.ID.Equals(ID) 的操作</param>
        /// <param name="fieldName">筛选字段</param>
        /// <param name="defValue">不存在时默认值</param>
        public T2 GetValue<T1, T2>(T1 ID, Expression<Func<TEntity, T2>> fieldName, T2 defValue = default(T2))
        {
            return Where<T1>(o => o.ID.Equals(ID)).GetValue(fieldName, defValue);
        }
        #endregion

        #region 聚合
        /// <summary>
        /// 累计和（不支持延迟加载）
        /// </summary>
        public T Sum<T>(Expression<Func<TEntity, T>> fieldName, T defValue = default(T))
        {
            if (fieldName == null) { throw new ArgumentNullException("fieldName", "查询Sum操作时，fieldName参数不能为空！"); }
            Select(fieldName);

            _set.Queue.SqlBuilder.Sum();
            return _set.QueueManger.ExecuteQuery(_set.Queue, defValue);
        }
        /// <summary>
        /// 查询最大数（不支持延迟加载）
        /// </summary>
        public T Max<T>(Expression<Func<TEntity, T>> fieldName, T defValue = default(T))
        {
            if (fieldName == null) { throw new ArgumentNullException("fieldName", "查询Max操作时，fieldName参数不能为空！"); }
            Select(fieldName);

            _set.Queue.SqlBuilder.Max();
            return _set.QueueManger.ExecuteQuery(_set.Queue, defValue);
        }
        /// <summary>
        /// 查询最小数（不支持延迟加载）
        /// </summary>
        public T Min<T>(Expression<Func<TEntity, T>> fieldName, T defValue = default(T))
        {
            if (fieldName == null) { throw new ArgumentNullException("fieldName", "查询Min操作时，fieldName参数不能为空！"); }
            Select(fieldName);

            _set.Queue.SqlBuilder.Min();
            return _set.QueueManger.ExecuteQuery(_set.Queue, defValue);
        }
        #endregion
    }
}
