using System.Data.Common;
using FS.Core.Data;
using FS.Core.Infrastructure;

namespace FS.Core.Client.SqlServer
{
    public class SqlServerProvider : DbProvider
    {
        public override DbProviderFactory GetDbProviderFactory
        {
            get { return DbProviderFactories.GetFactory("System.Data.SqlClient"); }
        }

        public override ISqlBuilder CreateSqlBuilder(BaseQueueManger queueManger, Queue queue)
        {
            switch (queueManger.ContextMap.ContextProperty.DataVer)
            {
                case "2000": return new SqlBuilder2000(queueManger, queue);
            }
            return new SqlBuilder(queueManger, queue);
        }
    }
}
