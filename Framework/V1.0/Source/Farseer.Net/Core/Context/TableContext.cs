﻿using System;
using FS.Configs;
using FS.Core.Data;
using FS.Core.Infrastructure;

namespace FS.Core.Context
{
    /// <summary>
    /// 表上下文
    /// </summary>
    public class TableContext : IDisposable
    {
        /// <summary>
        /// 通过数据库配置，连接数据库
        /// </summary>
        /// <param name="dbIndex">数据库选项</param>
        /// <param name="tableName">表名称</param>
        protected internal TableContext(int dbIndex = 0, string tableName = null) : this(DbFactory.CreateConnString(dbIndex), DbConfigs.ConfigInfo.DbList[dbIndex].DataType, DbConfigs.ConfigInfo.DbList[dbIndex].CommandTimeout, tableName) { }

        /// <summary>
        /// 通过自定义数据链接符，连接数据库
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="commandTimeout">SQL执行超时时间</param>
        /// <param name="tableName">表名称</param>
        protected internal TableContext(string connectionString, DataBaseType dbType = DataBaseType.SqlServer, int commandTimeout = 30, string tableName = null) : this(new DbExecutor(connectionString, dbType, commandTimeout), tableName) { }

        /// <summary>
        /// 事务
        /// </summary>
        /// <param name="database">数据库执行</param>
        /// <param name="tableName">表名称</param>
        protected internal TableContext(DbExecutor database, string tableName = null)
        {
            Database = database;
            TableName = tableName;
            IsMergeCommand = true;
        }

        /// <summary>
        /// 数据库
        /// </summary>
        internal protected DbExecutor Database { get; private set; }

        /// <summary>
        /// 数据库查询支持
        /// </summary>
        internal protected IQuery QueryProvider { get; set; }

        /// <summary>
        /// 合并执行命令
        /// </summary>
        internal protected bool IsMergeCommand { get; protected set; }

        /// <summary>
        /// 表名
        /// </summary>
        internal protected string TableName { get; protected set; }

        /// <summary>
        /// 保存修改
        /// IsMergeCommand=true时：只提交一次SQL到数据库
        /// </summary>
        public int SaveChanges()
        {
            if (QueryProvider.Param.Count > QueryProvider.DbProvider.ParamsMaxLength) { throw new Exception(string.Format("SQL参数过多，当前数据库类型，最多支持：{0}个，目前生成了{1}个", QueryProvider.DbProvider.ParamsMaxLength, QueryProvider.Param.Count)); }
            return QueryProvider.Commit();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Database.Dispose();
            Database = null;
        }
    }
}