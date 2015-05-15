﻿using System.Data.Common;
using FS.Core.Client.SqLite.SqlBuilder;
using FS.Core.Data;
using FS.Core.Infrastructure;

namespace FS.Core.Client.SqLite
{
    public class SqLiteProvider : DbProvider
    {
        public override DbProviderFactory GetDbProviderFactory
        {
            get { return DbProviderFactories.GetFactory("System.Data.SQLite"); }
        }
        public override IBuilderSqlQuery CreateBuilderSqlQuery(BaseQueueManger queueManger, Queue queue)
        {
            return new SqlQuery(queueManger, queue);
        }

        public override IBuilderSqlOper CreateBuilderSqlOper(BaseQueueManger queueManger, Queue queue)
        {
            return new SqlOper(queueManger, queue);
        }
    }
}
