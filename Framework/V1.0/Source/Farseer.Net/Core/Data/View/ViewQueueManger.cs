using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using FS.Core.Infrastructure;
using FS.Extends;
using FS.Mapping.Context;

namespace FS.Core.Data.View
{
    public class ViewQueueManger : IQueueManger
    {
        /// <summary>
        /// 数据库操作
        /// </summary>
        public DbExecutor DataBase { get; private set; }
        /// <summary>
        /// 数据库提供者（不同数据库的特性）
        /// </summary>
        public DbProvider DbProvider { get; set; }
        /// <summary>
        /// 映射关系
        /// </summary>
        public ContextMap ContextMap { get; set; }
        private Queue _queue;

        public ViewQueueManger(DbExecutor database, ContextMap contextMap)
        {
            DataBase = database;
            ContextMap = contextMap;
            DbProvider = DbProvider.CreateInstance(database.DataType);
            Clear();
        }

        /// <summary>
        /// 获取当前队列（不存在，则创建）
        /// </summary>
        /// <param name="map">字段映射</param>
        /// <param name="name">表名称</param>
        public IQueue GetQueue(string name, FieldMap map)
        {
            return _queue ?? (_queue = new Queue(0, name, map, this));
        }

        public int Commit() { return -1; }
        public void Append()
        {
            throw new NotImplementedException();
        }

        public List<DbParameter> Param
        {
            get
            {
                return _queue.Param;
            }
        }

        public void Clear()
        {
            _queue = null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        private void Dispose(bool disposing)
        {
            //释放托管资源
            if (disposing)
            {
                DataBase.Dispose();
                DataBase = null;
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
