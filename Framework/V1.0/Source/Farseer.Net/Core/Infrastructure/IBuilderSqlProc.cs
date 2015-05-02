﻿namespace FS.Core.Infrastructure
{
    /// <summary>
    /// 视图支持的SQL方法
    /// </summary>
    public interface IBuilderSqlProc
    {
        /// <summary>
        /// 查询单条记录
        /// </summary>
        void CreateParam<TEntity>(TEntity entity) where TEntity : class,new();
    }
}
