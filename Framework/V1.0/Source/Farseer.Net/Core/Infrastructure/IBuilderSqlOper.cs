namespace FS.Core.Infrastructure
{
    /// <summary>
    /// 表支持的SQL方法
    /// </summary>
    public interface IBuilderSqlOper : IBuilderSqlQuery
    {
        /// <summary>
        /// 删除
        /// </summary>
        IQueue Delete();
        /// <summary>
        /// 插入
        /// </summary>
        IQueue Insert<TEntity>(TEntity entity) where TEntity : class,new();
        /// <summary>
        /// 插入
        /// </summary>
        IQueue InsertIdentity<TEntity>(TEntity entity) where TEntity : class,new();
        /// <summary>
        /// 修改
        /// </summary>
        IQueue Update<TEntity>(TEntity entity) where TEntity : class,new();
        /// <summary>
        /// 添加或者减少某个字段
        /// </summary>
        IQueue AddUp();
    }
}
