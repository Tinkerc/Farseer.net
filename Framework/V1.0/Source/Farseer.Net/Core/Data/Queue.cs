using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FS.Core.Infrastructure;
using FS.Extends;
using FS.Mapping.Context;
using FS.Utils;

namespace FS.Core.Data
{
    /// <summary>
    /// 每一次的数据库查询，将生成一个新的实例
    /// </summary>
    public class Queue
    {
        /// <summary>
        /// 当前队列的ID
        /// </summary>
        public Guid ID { get; set; }
        /// <summary>
        /// 当前组索引
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// 表名/视图名/存储过程名
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 当前生成的参数
        /// </summary>
        public List<DbParameter> Param { get; set; }
        /// <summary>
        /// 实体类映射
        /// </summary>
        public FieldMap FieldMap { get; private set; }
        /// <summary>
        /// 当前生成的SQL语句
        /// </summary>
        public StringBuilder Sql { get; set; }
        /// <summary>
        /// SQL生成器
        /// </summary>
        public IBuilderSqlOper SqlBuilder { get; private set; }
        /// <summary>
        /// 延迟执行的委托
        /// </summary>
        public Action<Queue> LazyAct { get; set; }
        /// <summary>
        /// 排序表达式树
        /// </summary>
        public Dictionary<Expression, bool> ExpOrderBy { get; private set; }
        /// <summary>
        /// 字段筛选表达式树
        /// </summary>
        public List<Expression> ExpSelect { get; private set; }
        /// <summary>
        /// 条件表达式树
        /// </summary>
        public Expression ExpWhere { get; private set; }
        /// <summary>
        /// 赋值表达式树
        /// </summary>
        public Dictionary<Expression, object> ExpAssign { get; private set; }
        /// <summary>
        /// 队列管理模块
        /// </summary>
        private readonly BaseQueueManger _queueManger;
        public Queue(int index, string name, FieldMap map, BaseQueueManger queueManger)
        {
            ID = Guid.NewGuid();
            Index = index;
            Name = name;
            Param = new List<DbParameter>();
            FieldMap = map;
            _queueManger = queueManger;
            SqlBuilder = queueManger.DbProvider.CreateBuilderSqlOper(queueManger, this);
        }

        /// <summary>
        /// 添加筛选
        /// </summary>
        /// <param name="select"></param>
        public void AddSelect(Expression select)
        {
            if (ExpSelect == null) { ExpSelect = new List<Expression>(); }
            if (select != null) { ExpSelect.Add(select); }
        }
        /// <summary>
        ///     添加条件
        /// </summary>
        /// <param name="where">查询条件</param>
        public void AddWhere<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class
        {
            ExpWhere = ExpWhere == null ? ExpWhere = where : ((Expression<Func<TEntity, bool>>)ExpWhere).AndAlso(where);
        }

        /// <summary>
        /// 添加排序
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="isAsc"></param>
        public void AddOrderBy(Expression exp, bool isAsc)
        {
            if (ExpOrderBy == null) { ExpOrderBy = new Dictionary<Expression, bool>(); }
            if (exp != null) { ExpOrderBy.Add(exp, isAsc); }
        }
        /// <summary>
        /// 字段累加（字段 = 字段 + 值）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="fieldName">字段选择器</param>
        /// <param name="fieldValue">值</param>
        public void AddAssign(Expression fieldName, object fieldValue)
        {
            if (ExpAssign == null) { ExpAssign = new Dictionary<Expression, object>(); }
            if (fieldName != null) { ExpAssign.Add(fieldName, fieldValue); }
        }
        /// <summary>
        /// 复制条件
        /// </summary>
        /// <param name="queue">队列</param>
        public void Copy(Queue queue)
        {
            ExpOrderBy = queue.ExpOrderBy;
            ExpSelect = queue.ExpSelect;
            ExpWhere = queue.ExpWhere;
        }

        /// <summary>
        /// 执行SQL，返回影响行数
        /// </summary>
        public int Execute()
        {
            var param = Param == null ? null : Param.ToArray();
            var result = Sql.Length < 1 ? 0 : _queueManger.DataBase.ExecuteNonQuery(CommandType.Text, Sql.ToString(), param);

            _queueManger.Clear();
            return result;
        }
        /// <summary>
        /// 返回DataTable
        /// </summary>
        public DataTable ExecuteTable()
        {
            var param = Param == null ? null : Param.ToArray();
            var table = _queueManger.DataBase.GetDataTable(CommandType.Text, Sql.ToString(), param);
            _queueManger.Clear();
            return table;
        }
        /// <summary>
        /// 返回单条数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        public TEntity ExecuteInfo<TEntity>() where TEntity : class, new()
        {
            var param = Param == null ? null : Param.ToArray();
            TEntity t;
            using (var reader = _queueManger.DataBase.GetReader(CommandType.Text, Sql.ToString(), param))
            {
                t = reader.ToInfo<TEntity>();
            }
            _queueManger.DataBase.Close(false);

            _queueManger.Clear();
            return t;
        }
        /// <summary>
        /// 查询单个字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public T ExecuteQuery<T>(T defValue = default(T))
        {
            var param = Param == null ? null : Param.ToArray();
            var value = _queueManger.DataBase.ExecuteScalar(CommandType.Text, Sql.ToString(), param);
            var t = value.ConvertType(defValue);
            _queueManger.Clear();
            return t;
        }

        /// <summary>
        /// 将OutPut参数赋值到实体
        /// </summary>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="entity">实体类</param>
        private void SetParamToEntity<TEntity>(TEntity entity) where TEntity : class,new()
        {
            if (entity == null) { return; }
            var map = CacheManger.GetFieldMap(typeof(TEntity));
            foreach (var kic in map.MapList.Where(o => o.Value.FieldAtt.IsOutParam))
            {
                kic.Key.SetValue(entity, ConvertHelper.ConvertType(Param.Find(o => o.ParameterName == _queueManger.DbProvider.ParamsPrefix + kic.Value.FieldAtt.Name).Value, kic.Key.PropertyType), null);
            }
        }
        /// <summary>
        /// 存储过程创建SQL 输入、输出参数化
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        private List<DbParameter> CreateParam<TEntity>(TEntity entity) where TEntity : class,new()
        {
            Param = new List<DbParameter>();
            if (entity == null) { return Param; }

            foreach (var kic in FieldMap.MapList.Where(o => o.Value.FieldAtt.IsInParam || o.Value.FieldAtt.IsOutParam))
            {
                var obj = kic.Key.GetValue(entity, null);

                Param.Add(_queueManger.DbProvider.CreateDbParam(kic.Value.FieldAtt.Name, obj, kic.Key.PropertyType, kic.Value.FieldAtt.IsOutParam));
            }
            return Param;
        }

        public int Execute<TEntity>(TEntity entity) where TEntity : class,new()
        {
            var param = CreateParam(entity).ToArray();
            var result = _queueManger.DataBase.ExecuteNonQuery(CommandType.StoredProcedure, Name, param);
            SetParamToEntity(entity);

            _queueManger.Clear();
            return result;
        }
        public List<TEntity> ExecuteList<TEntity>(TEntity entity) where TEntity : class,new()
        {
            var param = CreateParam(entity).ToArray();
            List<TEntity> lst;
            using (var reader = _queueManger.DataBase.GetReader(CommandType.StoredProcedure, Name, param))
            {
                lst = reader.ToList<TEntity>();
            }
            _queueManger.DataBase.Close(false);
            SetParamToEntity(entity);
            _queueManger.Clear();
            return lst;
        }
        public TEntity ExecuteInfo<TEntity>(TEntity entity) where TEntity : class,new()
        {
            var param = CreateParam(entity).ToArray();
            TEntity t;
            using (var reader = _queueManger.DataBase.GetReader(CommandType.StoredProcedure, Name, param))
            {
                t = reader.ToInfo<TEntity>();
            }
            _queueManger.DataBase.Close(false);

            SetParamToEntity(entity);
            _queueManger.Clear();
            return t;
        }
        public T ExecuteValue<TEntity, T>(TEntity entity, T defValue = default(T)) where TEntity : class, new()
        {
            var param = CreateParam(entity).ToArray();
            var value = _queueManger.DataBase.ExecuteScalar(CommandType.StoredProcedure, Name, param);
            var t = value.ConvertType(defValue);

            SetParamToEntity(entity);
            _queueManger.Clear();
            return t;
        }

        #region 释放
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        private void Dispose(bool disposing)
        {
            //释放托管资源
            if (disposing)
            {
                if (Sql != null) { Sql.Clear(); Sql = null; }

                ExpOrderBy = null;
                ExpSelect = null;
                ExpWhere = null;
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
        #endregion
    }
}
