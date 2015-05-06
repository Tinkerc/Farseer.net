using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FS.Core.Infrastructure;
using FS.Utils;

namespace FS.Extends
{
    public static class Extend
    {
        /// <summary>
        ///     将对象转换为T类型
        /// </summary>
        /// <param name="sourceValue">要转换的源对象</param>
        /// <param name="defValue">转换失败时，代替的默认值</param>
        /// <typeparam name="T">基本类型</typeparam>
        public static T ConvertType<T>(this object sourceValue, T defValue = default(T))
        {
            if (sourceValue == null) { return defValue; }

            var returnType = typeof(T);
            var sourceType = sourceValue.GetType();

            // 相同类型，则直接返回原型
            if (Type.GetTypeCode(returnType) == Type.GetTypeCode(sourceType)) { return (T)sourceValue; }

            var val = ConvertType(sourceValue, returnType);
            return val != null ? (T)val : defValue;
        }

        /// <summary>
        ///     将值转换成类型对象的值
        /// </summary>
        /// <param name="sourceValue">要转换的值</param>
        /// <param name="defType">要转换的对象的类型</param>
        public static object ConvertType(this object sourceValue, Type defType)
        {
            if (sourceValue == null) { return null; }

            // 对   Nullable<> 类型处理
            if (defType.IsGenericType && defType.GetGenericTypeDefinition() == typeof(Nullable<>)) { return ConvertType(sourceValue, Nullable.GetUnderlyingType(defType)); }
            // 对   List 类型处理
            if (defType.IsGenericType && defType.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                var objString = sourceValue.ToString();
                // List参数类型
                var argumType = defType.GetGenericArguments()[0];

                switch (Type.GetTypeCode(argumType))
                {
                    case TypeCode.Boolean: { return ToList(objString, false); }
                    case TypeCode.DateTime: { return ToList(objString, DateTime.MinValue); }
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single: { return ToList(objString, 0m); }
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16: { return ToList<ushort>(objString, 0); }
                    case TypeCode.UInt32: { return ToList<uint>(objString, 0); }
                    case TypeCode.UInt64: { return ToList<ulong>(objString, 0); }
                    case TypeCode.Int16: { return ToList<short>(objString, 0); }
                    case TypeCode.Int64: { return ToList<long>(objString, 0); }
                    case TypeCode.Int32: { return ToList(objString, 0); }
                    case TypeCode.Empty:
                    case TypeCode.Char:
                    case TypeCode.String: { return ToList(objString, ""); }
                }
            }

            return ConvertHelper.ConvertType(sourceValue, sourceValue.GetType(), defType);
        }

        /// <summary>
        ///     将字符串转换成List型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="splitString">分隔符为NullOrEmpty时，则直接拆份为Char</param>
        /// <param name="defValue">默认值(单项转换失败时，默认值为NullOrEmpty时，则不添加，否则替换为默认值)</param>
        /// <typeparam name="T">基本类型</typeparam>
        public static List<T> ToList<T>(this string str, T defValue, string splitString = ",")
        {
            var lst = new List<T>();
            if (string.IsNullOrWhiteSpace(str)) { return lst; }

            //判断是否带分隔符，如果没有。则直接拆份单个Char
            if (string.IsNullOrWhiteSpace(splitString))
            {
                for (var i = 0; i < str.Length; i++) { lst.Add(ConvertType(str.Substring(i, 1), defValue)); }
            }
            else
            {
                var strArray = splitString.Length == 1 ? str.Split(splitString[0]) : str.Split(new string[1] { splitString }, StringSplitOptions.None);
                lst.AddRange(strArray.Select(item => ConvertType(item, defValue)));
            }
            return lst;
        }

        /// <summary>
        ///     将集合类转换成DataTable
        /// </summary>
        /// <param name="lst">集合</param>
        /// <returns></returns>
        public static DataTable ToTable<TEntity>(this List<TEntity> lst) where TEntity : class
        {
            var dt = new DataTable();
            if (lst.Count == 0) { return dt; }
            var map = CacheManger.GetFieldMap(lst[0].GetType());
            var lstFields = map.MapList.Where(o => o.Value.FieldAtt.IsMap).ToList();
            foreach (var field in lstFields)
            {
                var type = field.Key.PropertyType;
                // 对   List 类型处理
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetGenericArguments()[0];
                }
                dt.Columns.Add(field.Value.FieldAtt.Name, type);
            }

            foreach (var info in lst)
            {
                dt.Rows.Add(dt.NewRow());
                foreach (var field in lstFields)
                {
                    var value = ConvertHelper.GetValue(info, field.Key.Name, (object)null);
                    if (value == null) { continue; }
                    if (!dt.Columns.Contains(field.Value.FieldAtt.Name)) { dt.Columns.Add(field.Value.FieldAtt.Name); }
                    dt.Rows[dt.Rows.Count - 1][field.Value.FieldAtt.Name] = value;
                }
            }
            return dt;
        }

        /// <summary>
        ///     DataTable转换为实体类
        /// </summary>
        /// <param name="dt">源DataTable</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        public static List<TEntity> ToList<TEntity>(this DataTable dt) where TEntity : class, new()
        {
            var list = new List<TEntity>();
            var map = CacheManger.GetFieldMap(typeof(TEntity));
            foreach (DataRow dr in dt.Rows)
            {
                // 赋值字段
                var t = new TEntity();
                foreach (var kic in map.MapList)
                {
                    if (!kic.Key.CanWrite) { continue; }
                    var filedName = !DbProvider.IsField(kic.Value.FieldAtt.Name) ? kic.Key.Name : kic.Value.FieldAtt.Name;
                    if (dr.Table.Columns.Contains(filedName))
                    {
                        kic.Key.SetValue(t, ConvertType(dr[filedName], kic.Key.PropertyType), null);
                    }
                }
                list.Add(t);
            }
            return list;
        }
        /// <summary>
        ///     IDataReader转换为实体类
        /// </summary>
        /// <param name="reader">源IDataReader</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        public static List<TEntity> ToList<TEntity>(this IDataReader reader) where TEntity : class, new()
        {
            var list = new List<TEntity>();
            var map = CacheManger.GetFieldMap(typeof(TEntity));

            while (reader.Read())
            {
                var t = (TEntity)Activator.CreateInstance(typeof(TEntity));

                //赋值字段
                foreach (var kic in map.MapList)
                {
                    if (ConvertHelper.HaveName(reader, kic.Value.FieldAtt.Name))
                    {
                        if (!kic.Key.CanWrite) { continue; }
                        kic.Key.SetValue(t, reader[kic.Value.FieldAtt.Name].ConvertType(kic.Key.PropertyType), null);
                    }
                }

                list.Add(t);
            }
            reader.Close();
            return list;
        }

        /// <summary>
        ///     数据填充
        /// </summary>
        /// <param name="reader">源IDataReader</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        public static TEntity ToInfo<TEntity>(this IDataReader reader) where TEntity : class, new()
        {
            var map = CacheManger.GetFieldMap(typeof(TEntity));

            var t = (TEntity)Activator.CreateInstance(typeof(TEntity));
            var isHaveValue = false;

            if (reader.Read())
            {
                //赋值字段
                foreach (var kic in map.MapList)
                {
                    if (ConvertHelper.HaveName(reader, kic.Value.FieldAtt.Name))
                    {
                        if (!kic.Key.CanWrite) { continue; }
                        kic.Key.SetValue(t, reader[kic.Value.FieldAtt.Name].ConvertType(kic.Key.PropertyType), null);
                        isHaveValue = true;
                    }
                }
            }
            reader.Close();
            return isHaveValue ? t : null;
        }
        /// <summary>
        ///     将集合类转换成DataTable
        /// </summary>
        /// <param name="list">集合</param>
        /// <returns></returns>
        public static DataTable ToTable(this IList list)
        {
            var result = new DataTable();
            if (list.Count <= 0) { return result; }

            var propertys = list[0].GetType().GetProperties();
            foreach (var pi in propertys) { result.Columns.Add(pi.Name, pi.PropertyType); }

            foreach (var info in list)
            {
                var tempList = new ArrayList();
                foreach (var obj in propertys.Select(pi => pi.GetValue(info, null))) { tempList.Add(obj); }
                var array = tempList.ToArray();
                result.LoadDataRow(array, true);
            }
            return result;
        }

        /// <summary>
        ///     将泛型集合类转换成DataTable
        /// </summary>
        /// <param name="list">集合</param>
        /// <param name="propertyName">需要返回的列的列名</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToTable(this IList list, params string[] propertyName)
        {
            var propertyNameList = new List<string>();
            if (propertyName != null)
                propertyNameList.AddRange(propertyName);

            var result = new DataTable();
            if (list.Count <= 0) { return result; }
            var propertys = list[0].GetType().GetProperties();
            foreach (var pi in propertys)
            {
                if (propertyNameList.Count == 0)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }
                else
                {
                    if (propertyNameList.Contains(pi.Name)) { result.Columns.Add(pi.Name, pi.PropertyType); }
                }
            }

            foreach (var info in list)
            {
                var tempList = new ArrayList();
                foreach (var pi in propertys)
                {
                    if (propertyNameList.Count == 0)
                    {
                        var obj = pi.GetValue(info, null);
                        tempList.Add(obj);
                    }
                    else
                    {
                        if (!propertyNameList.Contains(pi.Name)) continue;
                        var obj = pi.GetValue(info, null);
                        tempList.Add(obj);
                    }
                }
                var array = tempList.ToArray();
                result.LoadDataRow(array, true);
            }
            return result;
        }

        /// <summary>
        ///     将DataRow转成实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="dr">源DataRow</param>
        public static TEntity ToInfo<TEntity>(this DataRow dr) where TEntity : class,new()
        {
            var map = CacheManger.GetFieldMap(typeof(TEntity));
            var t = (TEntity)Activator.CreateInstance(typeof(TEntity));

            //赋值字段
            foreach (var kic in map.MapList)
            {
                if (dr.Table.Columns.Contains(kic.Value.FieldAtt.Name))
                {
                    if (!kic.Key.CanWrite) { continue; }
                    kic.Key.SetValue(t, ConvertType(dr[kic.Value.FieldAtt.Name], kic.Key.PropertyType), null);
                }
            }
            return t ?? new TEntity();
        }

        /// <summary>
        ///     对IOrderedQueryable进行分页
        /// </summary>
        /// <typeparam name="TSource">实体类</typeparam>
        /// <param name="source">List列表</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="pageIndex">索引</param>
        public static List<TSource> ToList<TSource>(this IOrderedQueryable<TSource> source, int pageSize, int pageIndex = 1)
        {
            return source.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }

        /// <summary>
        ///     对IOrderedQueryable进行分页
        /// </summary>
        /// <typeparam name="TSource">实体类</typeparam>
        /// <param name="source">List列表</param>
        /// <param name="recordCount">记录总数</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="pageIndex">索引</param>
        public static List<TSource> ToList<TSource>(this IOrderedQueryable<TSource> source, out int recordCount, int pageSize, int pageIndex = 1)
        {
            recordCount = source.Count();

            #region 计算总页数
            var allCurrentPage = 1;

            if (pageIndex < 1) { pageIndex = 1; }
            if (pageSize < 0) { pageSize = 0; }
            if (pageSize != 0)
            {
                allCurrentPage = (recordCount / pageSize);
                allCurrentPage = ((recordCount % pageSize) != 0 ? allCurrentPage + 1 : allCurrentPage);
                allCurrentPage = (allCurrentPage == 0 ? 1 : allCurrentPage);
            }
            if (pageIndex > allCurrentPage) { pageIndex = allCurrentPage; }
            #endregion

            return source.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
        }
    }
}
