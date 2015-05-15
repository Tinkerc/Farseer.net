using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
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
            return ConvertHelper.ConvertType(sourceValue, defValue);
        }

        /// <summary>
        ///     将值转换成类型对象的值
        /// </summary>
        /// <param name="sourceValue">要转换的值</param>
        /// <param name="defType">要转换的对象的类型</param>
        public static object ConvertType(this object sourceValue, Type defType)
        {
            return ConvertHelper.ConvertType(sourceValue, defType);
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
            return ConvertHelper.ToList(str, defValue, splitString);
        }

        /// <summary>
        ///     DataTable转换为实体类
        /// </summary>
        /// <param name="dt">源DataTable</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        public static List<TEntity> ToList<TEntity>(this DataTable dt) where TEntity : class, new()
        {
            return ConvertHelper.ToList<TEntity>(dt);
        }
        /// <summary>
        ///     IDataReader转换为实体类
        /// </summary>
        /// <param name="reader">源IDataReader</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        public static List<TEntity> ToList<TEntity>(this IDataReader reader) where TEntity : class, new()
        {
            return ConvertHelper.ToList<TEntity>(reader);
        }

        /// <summary>
        ///     数据填充
        /// </summary>
        /// <param name="reader">源IDataReader</param>
        /// <typeparam name="TEntity">实体类</typeparam>
        public static TEntity ToInfo<TEntity>(this IDataReader reader) where TEntity : class, new()
        {
            return ConvertHelper.ToInfo<TEntity>(reader);
        }

        /// <summary>
        ///     And 操作
        /// </summary>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="left">左树</param>
        /// <param name="right">右树</param>
        public static Expression<Func<TEntity, bool>> AndAlso<TEntity>(this Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right) where TEntity : class
        {
            if (left == null) { return right; }
            if (right == null) { return left; }

            var param = left.Parameters[0];
            return Expression.Lambda<Func<TEntity, bool>>(ReferenceEquals(param, right.Parameters[0]) ? Expression.AndAlso(left.Body, right.Body) : Expression.AndAlso(left.Body, Expression.Invoke(right, param)), param);
        }
    }
}
