﻿using System;
using System.Linq.Expressions;
using FS.Core;
using FS.Utils;

// ReSharper disable once CheckNamespace
namespace FS.Extends
{
    /// <summary>
    ///     Expression表达式树扩展
    /// </summary>
    public static class ExpressionExtend
    {

        /// <summary>
        ///     OR 操作
        /// </summary>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="left">左树</param>
        /// <param name="right">右树</param>
        public static Expression<Func<TEntity, bool>> OrElse<TEntity>(this Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right)
            where TEntity : class
        {
            if (left == null) { return right; }

            var param = left.Parameters[0];
            return Expression.Lambda<Func<TEntity, bool>>(ReferenceEquals(param, right.Parameters[0]) ? Expression.OrElse(left.Body, right.Body) : Expression.OrElse(left.Body, Expression.Invoke(right, param)), param);
        }

        /// <summary>
        ///     获取字段名称
        /// </summary>
        /// <param name="select">字段名称</param>
        /// <returns></returns>
        public static string GetUsedName<T1, T2>(this Expression<Func<T1, T2>> select) where T1 : class
        {
            MemberExpression memberExpression;

            var unary = @select.Body as UnaryExpression;
            if (unary != null)
            {
                memberExpression = unary.Operand as MemberExpression;
            }
            else if (@select.Body.NodeType == ExpressionType.Call)
            {
                memberExpression = (MemberExpression)((MethodCallExpression)@select.Body).Object;
            }
            else
            {
                memberExpression = @select.Body as MemberExpression;
            }

            var map = CacheManger.GetFieldMap(typeof(T1));
            var modelInfo = map.GetState((memberExpression.Member).Name);

            return modelInfo.Value.FieldAtt.Name;
        }

        public static Expression<Func<TOuter, TInner>> Combine<TOuter, TMiddle, TInner>(
            Expression<Func<TOuter, TMiddle>> first,
            Expression<Func<TMiddle, TInner>> second)
        {
            return x => second.Compile()(first.Compile()(x));
        }
    }
}