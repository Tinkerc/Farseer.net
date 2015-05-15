using System.Data.Common;
using FS.Core.Client.MySql.SqlBuilder;
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
