﻿using System.Data.Common;
using FS.Core.Data;
using FS.Core.Infrastructure;

namespace FS.Core.Client.Oracle
{
    public class OracleProvider : DbProvider
    {
        public override string ParamsPrefix
        {
            get { return ":"; }
        }

        public override string KeywordAegis(string fieldName)
        {
            return fieldName;
        }

        public override DbProviderFactory GetDbProviderFactory
        {
            get { return DbProviderFactories.GetFactory("System.Data.OracleClient"); }
        }
        public override ISqlBuilder CreateSqlBuilder(BaseQueueManger queueManger, Queue queue)
        {
            return new SqlBuilder(queueManger, queue);
        }
    }
}
