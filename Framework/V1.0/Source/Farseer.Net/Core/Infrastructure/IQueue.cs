using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using FS.Core.Data;
using FS.Mapping.Context;

namespace FS.Core.Infrastructure
{
    /// <summary>
    /// 每一次的数据库查询，将生成一个新的实例
    /// </summary>
    public interface IQueue : IDisposable
    {
        /// <summary>
        /// 当前队列的ID
        /// </summary>
        Guid ID { get; set; }
        /// <summary>
        /// 当前组索引
        /// </summary>
        int Index { get; }
        /// <summary>
        /// 当前生成的参数
        /// </summary>
        List<DbParameter> Param { get; set; }
        /// <summary>
        /// 表名/视图名/存储过程名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 实体类映射
        /// </summary>
        FieldMap FieldMap { get; }
        StringBuilder Sql { get; set; }
        Dictionary<Expression, bool> ExpOrderBy { get; }
        List<Expression> ExpSelect { get; }
        Expression ExpWhere { get; }
        Dictionary<Expression, object> ExpAssign { get; }
        IBuilderSqlOper SqlBuilder { get; set; }
        Action<IQueue> LazyAct { get; set; }

        void LazyQuery(Action<IQueue> act);
        /// <summary>
        /// 添加筛选
        /// </summary>
        /// <param name="select"></param>
        void AddSelect(Expression select);

        /// <summary>
        ///     添加条件
        /// </summary>
        /// <param name="where">查询条件</param>
        void AddWhere<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class;
        /// <summary>
        /// 添加排序
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="isAsc"></param>
        void AddOrderBy(Expression exp, bool isAsc);
        /// <summary>
        /// 字段累加（字段 = 字段 + 值）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="fieldName">字段选择器</param>
        /// <param name="fieldValue">值</param>
        void AddAssign(Expression fieldName, object fieldValue);
        void Copy(IQueue queue);
        int Execute();
        DataTable ExecuteTable();
        TEntity ExecuteInfo<TEntity>() where TEntity : class, new();
        T ExecuteQuery<T>(T defValue = default(T));
    }
}
