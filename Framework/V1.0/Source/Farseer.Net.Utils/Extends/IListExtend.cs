using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FS.Utils.Extends
{
    public static class IListExtend
    {
        /// <summary>
        ///     获取List最后一项
        /// </summary>
        /// <typeparam name="T">任何对象</typeparam>
        /// <param name="lst">List列表</param>
        public static T GetLast<T>(this IList<T> lst)
        {
            return lst.Count > 0 ? lst[lst.Count - 1] : default(T);
        }

        /// <summary>
        /// 生成测试数据
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="lst">列表</param>
        /// <param name="count">生成的数据</param>
        public static List<TEntity> TestData<TEntity>(this IList<TEntity> lst, int count) where TEntity : class,new()
        {
            lst = new List<TEntity>();
            for (var i = 0; i < count; i++)
            {
                lst.Add(new TEntity().FillRandData());
            }
            return lst.ToList();
        }

        /// <summary>
        /// 清除重复的词语（每项中的每个字符对比）
        /// 然后向右横移一位，按最长到最短截取匹配每一项
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static List<string> ClearRepeat(this IList<string> lst)
        {
            for (var index = 0; index < lst.Count; index++) // 迭代所有关键词
            {
                var key = lst[index];
                for (var moveIndex = 0; moveIndex < key.Length; moveIndex += 1)     // 每次移动后2位当前关键词
                {
                    for (var step = key.Length; (step - moveIndex) >= 2; step--)   // 每次减少1位来对比
                    {
                        var clearKey = key.Substring(moveIndex, step - moveIndex);  // 截取的关键词

                        for (var index2 = index + 1; index2 < lst.Count; index2++)  // 清除下一项的所有字符串
                        {
                            lst[index2] = lst[index2].Replace(clearKey, "").Trim();
                        }
                    }
                }
            }

            for (var i = 0; i < lst.Count; i++)
            {
                if (lst[i].IsNullOrEmpty()) { lst.RemoveAt(i); i--; }
            }
            return lst.ToList();
        }

        /// <summary>
        ///     自动填充到指定数量
        /// </summary>
        public static IList Fill(this IList lst, int maxCount, object defValue)
        {
            while (true)
            {
                if (lst.Count >= maxCount) { break; }
                lst.Add(defValue);
            }

            return lst;
        }
    }
}
