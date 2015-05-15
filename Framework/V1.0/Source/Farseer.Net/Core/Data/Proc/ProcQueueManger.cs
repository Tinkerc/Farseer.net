using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using FS.Core.Infrastructure;
using FS.Extends;
using FS.Mapping.Context;
using FS.Utils;

namespace FS.Core.Data.Proc
{
    /// <summary>
    /// 队列管理
    /// </summary>
    public class ProcQueueManger : BaseQueueManger
    {
        /// <summary>
        /// 当前所有持久化列表
        /// </summary>
        private readonly List<Queue> _groupQueueList;
        /// <summary>
        /// 所有队列的参数
        /// </summary>
        public override List<DbParameter> Param
        {
            get
            {
                var lst = new List<DbParameter>();
                _groupQueueList.Where(o => o.Param != null).Select(o => o.Param).ToList().ForEach(lst.AddRange);
                return lst;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库操作</param>
        /// <param name="contextMap">映射关系</param>
        public ProcQueueManger(DbExecutor database, ContextMap contextMap)
            : base(database, contextMap)
        {
            _groupQueueList = new List<Queue>();
        }

        /// <summary>
        /// 获取当前队列（不存在，则创建）
        /// </summary>
        /// <param name="map">字段映射</param>
        /// <param name="name">表名称</param>
        public override Queue CreateQueue(string name, FieldMap map)
        {
            return Queue ?? (Queue = new Queue(_groupQueueList.Count, name, map, this));
        }

        /// <summary>
        /// 延迟执行数据库交互，并提交到队列
        /// </summary>
        /// <param name="act">要延迟操作的委托</param>
        /// <param name="map">字段映射</param>
        /// <param name="name">表名称</param>
        /// <param name="isExecute">是否立即执行</param>
        public override void Append(string name, FieldMap map, Action<Queue> act, bool isExecute)
        {
            CreateQueue(name, map);
            if (isExecute) { act(Queue); return; }
            Queue.LazyAct = act;
            if (Queue != null) { _groupQueueList.Add(Queue); }
            Clear();
        }

        /// <summary>
        /// 提交所有GetQueue，完成数据库交互
        /// </summary>
        public int Commit()
        {
            foreach (var queryQueue in _groupQueueList)
            {
                // 查看是否延迟加载
                if (queryQueue.LazyAct != null) { queryQueue.LazyAct(queryQueue); }
                queryQueue.Dispose();
            }

            // 清除队列
            _groupQueueList.Clear();
            Clear();
            return 0;
        }

        /// <summary>
        /// 将OutPut参数赋值到实体
        /// </summary>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="queue">每一次的数据库查询，将生成一个新的实例</param>
        /// <param name="entity">实体类</param>
        private void SetParamToEntity<TEntity>(Queue queue, TEntity entity) where TEntity : class,new()
        {
            if (entity == null) { return; }
            var map = CacheManger.GetFieldMap(typeof(TEntity));
            foreach (var kic in map.MapList.Where(o => o.Value.FieldAtt.IsOutParam))
            {
                kic.Key.SetValue(entity, ConvertHelper.ConvertType(queue.Param.Find(o => o.ParameterName == DbProvider.ParamsPrefix + kic.Value.FieldAtt.Name).Value, kic.Key.PropertyType), null);
            }
        }
        /// <summary>
        /// 存储过程创建SQL 输入、输出参数化
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="queue"></param>
        /// <param name="entity"></param>
        private List<DbParameter> CreateParam<TEntity>(Queue queue, TEntity entity) where TEntity : class,new()
        {
            queue.Param = new List<DbParameter>();
            if (entity == null) { return queue.Param; }

            foreach (var kic in queue.FieldMap.MapList.Where(o => o.Value.FieldAtt.IsInParam || o.Value.FieldAtt.IsOutParam))
            {
                var obj = kic.Key.GetValue(entity, null);

                queue.Param.Add(DbProvider.CreateDbParam(kic.Value.FieldAtt.Name, obj, kic.Key.PropertyType, kic.Value.FieldAtt.IsOutParam));
            }
            return queue.Param;
        }

        public int Execute<TEntity>(Queue queue, TEntity entity = null) where TEntity : class,new()
        {
            var param = CreateParam(queue, entity).ToArray();
            var result = DataBase.ExecuteNonQuery(CommandType.StoredProcedure, queue.Name, param);
            SetParamToEntity(queue, entity);

            Clear();
            return result;
        }
        public List<TEntity> ExecuteList<TEntity>(Queue queue, TEntity entity = null) where TEntity : class,new()
        {
            var param = CreateParam(queue, entity).ToArray();
            List<TEntity> lst;
            using (var reader = DataBase.GetReader(CommandType.StoredProcedure, queue.Name, param))
            {
                lst = reader.ToList<TEntity>();
            }
            DataBase.Close(false);
            SetParamToEntity(queue, entity);
            Clear();
            return lst;
        }
        public TEntity ExecuteInfo<TEntity>(Queue queue, TEntity entity = null) where TEntity : class,new()
        {
            var param = CreateParam(queue, entity).ToArray();
            TEntity t;
            using (var reader = DataBase.GetReader(CommandType.StoredProcedure, queue.Name, param))
            {
                t = reader.ToInfo<TEntity>();
            }
            DataBase.Close(false);

            SetParamToEntity(queue, entity);
            Clear();
            return t;
        }
        public T ExecuteValue<TEntity, T>(Queue queue, TEntity entity = null, T defValue = default(T)) where TEntity : class, new()
        {
            var param = CreateParam(queue, entity).ToArray();
            var value = DataBase.ExecuteScalar(CommandType.StoredProcedure, queue.Name, param);
            var t = value.ConvertType(defValue);

            SetParamToEntity(queue, entity);
            Clear();
            return t;
        }
    }
}