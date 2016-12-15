using System;

namespace Fanmaquar.Common
{
    /// <summary>
    /// Extension methods to make working with Enum values easier
    /// http://somewebguy.wordpress.com/2010/02/23/enumeration-extensions-2/
    /// </summary>
    public static class EnumerationExtensions
    {

        #region Extension Methods

        /// <summary>
        /// Includes an enumerated type and returns the new value
        /// </summary>
        public static T Include<T>(this Enum value, T append)
        {
            Type type = value.GetType();

            //determine the values
            object result = value;
            _Value parsed = new _Value(append, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) | (long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;
            }

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>
        /// Removes an enumerated type and returns the new value
        /// </summary>
        public static T Remove<T>(this Enum value, T remove)
        {
            Type type = value.GetType();

            //determine the values
            object result = value;
            _Value parsed = new _Value(remove, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) & ~(long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;
            }

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>
        /// Checks if an enumerated type contains a value
        /// </summary>
        public static bool Has<T>(this Enum value, T check)
        {
            Type type = value.GetType();

            //determine the values
            object result = value;
            _Value parsed = new _Value(check, type);
            if (parsed.Signed is long)
            {
                return (Convert.ToInt64(value) & (long)parsed.Signed) == (long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                return (Convert.ToUInt64(value) & (ulong)parsed.Unsigned) == (ulong)parsed.Unsigned;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an enumerated type is missing a value
        /// </summary>
        public static bool Missing<T>(this Enum obj, T value)
        {
            return !Has<T>(obj, value);
        }

        #endregion

        #region Helper Classes

        //class to simplfy narrowing values between
        //a ulong and long since either value should
        //cover any lesser value
        private class _Value
        {

            //cached comparisons for tye to use
            private static Type _UInt64 = typeof(ulong);
            private static Type _UInt32 = typeof(long);

            public long? Signed;
            public ulong? Unsigned;

            public _Value(object value, Type type)
            {

                //make sure it is even an enum to work with
                if (!type.IsEnum)
                {
                    throw new ArgumentException("Value provided is not an enumerated type!");
                }

                //then check for the enumerated value
                Type compare = Enum.GetUnderlyingType(type);

                //if this is an unsigned long then the only
                //value that can hold it would be a ulong
                if (compare.Equals(_Value._UInt32) || compare.Equals(_Value._UInt64))
                {
                    this.Unsigned = Convert.ToUInt64(value);
                }
                //otherwise, a long should cover anything else
                else
                {
                    this.Signed = Convert.ToInt64(value);
                }

            }

        }

        #endregion

        #region 全局静态方法
        //http://www.cnblogs.com/xiaosonl/archive/2009/06/17/1505312.html

        /// <summary>
        /// 添加某项权限（使用与运算来实现）
        /// </summary>
        /// <param name="oldRights">旧权限定义</param>
        /// <param name="rightsAdd">新的权限定义</param>
        /// <returns>添加某项权限后的值</returns>
        public static long AddRights(this long oldRights, long rightsAdd)
        {
            return oldRights | rightsAdd;
        }

        /// <summary>
        /// 移除某项权限（使用与运算+非运算来实现）
        /// </summary>
        /// <param name="oldRights">旧权限定义</param>
        /// <param name="rightsRemove">要移除的权限定义</param>
        /// <returns>移除某项权限后的值</returns>
        public static long RemoveRights(this long oldRights, long rightsRemove)
        {
            return oldRights &= ~rightsRemove;
        }

        /// <summary>
        /// 权限的判断（使用与运算, 当判断用一用户是否具有该操作权限时, 要把用户的的权限与操作权限进行与运算, 如果得到的结果仍是操作权限, 则表示用户具有该权限:）
        /// </summary>
        /// <param name="rightsDefine">权限定义</param>
        /// <param name="rightsCheck">检测权限</param>
        /// <returns>
        /// 	如果有则返回true，否则为false。
        /// </returns>
        public static bool HasRights(this long rightsDefine, long rightsCheck)
        {
            return (rightsCheck & rightsDefine) == rightsCheck;
        }

        #endregion
    }
}
