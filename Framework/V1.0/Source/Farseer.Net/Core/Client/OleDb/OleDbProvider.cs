using System.Data.Common;
using FS.Core.Data;
using FS.Core.Infrastructure;

namespace FS.Core.Client.OleDb
{
    public class OleDbProvider : DbProvider
    {
        public override DbProviderFactory GetDbProviderFactory
        {
            get { return DbProviderFactories.GetFactory("System.Data.OleDb"); }
        }
        public override ISqlBuilder CreateSqlBuilder(BaseQueueManger queueManger, Queue queue)
        {
            return new SqlBuilder(queueManger, queue);
        }
    }
}
