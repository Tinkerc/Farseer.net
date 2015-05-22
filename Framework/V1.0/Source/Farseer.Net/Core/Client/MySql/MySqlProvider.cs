using System.Data.Common;
using FS.Core.Data;
using FS.Core.Infrastructure;

namespace FS.Core.Client.MySql
{
    public class MySqlProvider : DbProvider
    {
        public override DbProviderFactory GetDbProviderFactory
        {
            get { return DbProviderFactories.GetFactory("MySql.Data.MySqlClient"); }
        }

        public override string KeywordAegis(string fieldName)
        {
            return string.Format("`{0}`", fieldName);
        }
        public override ISqlBuilder CreateSqlBuilder(BaseQueueManger queueManger, Queue queue)
        {
            return new SqlBuilder(queueManger, queue);
        }
    }
}
