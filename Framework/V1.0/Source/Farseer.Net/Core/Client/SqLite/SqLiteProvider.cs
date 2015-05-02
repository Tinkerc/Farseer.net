﻿using System.Data.Common;
using FS.Core.Client.SqLite.SqlQuery;
using FS.Core.Infrastructure;
using FS.Mapping.Context;

namespace FS.Core.Client.SqLite
{
    public class SqLiteProvider : DbProvider
    {
        public override DbProviderFactory GetDbProviderFactory
        {
            get { return DbProviderFactories.GetFactory("System.Data.SQLite"); }
        }
        public override IBuilderSqlQuery<TEntity> CreateBuilderSqlQuery<TEntity>(ContextMap contextMap, IQueueManger queueManger, IQueueSql queueSql)
        {
            return new SqlQuery<TEntity>(queueManger, queueSql);
        }

        public override IBuilderSqlProc<TEntity> CreateBuilderSqlProc<TEntity>(ContextMap contextMap, IQueueManger queueManger, IQueue queueSql)
        {
            return new SqlProc<TEntity>(queueManger, queueSql);
        }

        public override IBuilderSqlOper<TEntity> CreateBuilderSqlOper<TEntity>(ContextMap contextMap, IQueueManger queueManger, IQueueSql queueSql)
        {
            return new SqlOper<TEntity>(queueManger, queueSql);
        }
    }
}
