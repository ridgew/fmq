using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Text;

namespace GenericSharpTool
{

    public class DapperDynamicParameter : DynamicObject
    {
        Dictionary<string, object> _innerDict = new Dictionary<string, object>();

        public int Count
        {
            get
            {
                return _innerDict.Count;
            }
        }

        public object this[string memeber]
        {
            get { return _innerDict[memeber]; }
            set { _innerDict[memeber] = value; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name.ToLower();
            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return _innerDict.TryGetValue(name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            _innerDict[binder.Name.ToLower()] = value;


            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }

        /// <summary>
        /// 使用dynamic根据DataTable的列名自动添加属性并赋值
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns> 
        public static Object GetDynamicObjectFromTable(DataTable dt)
        {
            dynamic d = new ExpandoObject();
            //创建属性，并赋值。
            foreach (DataColumn cl in dt.Columns)
            {
                (d as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>(cl.ColumnName, dt.Rows[0][cl.ColumnName]));
            }
            return d;
        }

    }
}

