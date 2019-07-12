#region  <<版本注释>>
/* ========================================================== 
// <copyright file="StaticMemberDynamicWrapper.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：StaticMemberDynamicWrapper 
* 创 建 者：Administrator 
* 创建时间：2019/5/16 17:09:44 
* =============================================================*/
#endregion

using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Learn.UnitTest.DynamicType
{
    /// <summary>
    /// 自定义一个动态类的读取和写入
    /// </summary>
    public sealed class StaticMemberDynamicWrapper:DynamicObject
    {
        /// <summary>
        /// 描述参数类型的字段
        /// </summary>
        private readonly TypeInfo m_type;

        /// <summary>
        /// 初始化需告知类型
        /// </summary>
        /// <param name="type"></param>
        public StaticMemberDynamicWrapper(Type type) { m_type = type.GetTypeInfo(); }

        //public override IEnumerable<String> GetDynamicMemberNames()
        //{
        //    return m_type.DeclaredMembers.Select(mi => mi.Name);
        //}

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var field = FindField(binder.Name);
            if (field != null) { result = field.GetValue(null); return true; }

            var prop = FindProperty(binder.Name, true);
            if (prop != null) { result = prop.GetValue(null, null); return true; }
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var field = FindField(binder.Name);
            if (field != null) { field.SetValue(null, value); return true; }

            var prop = FindProperty(binder.Name, false);
            if (prop != null) { prop.SetValue(null, value, null); return true; }
            return false;
        }

        public override Boolean TryInvokeMember(InvokeMemberBinder binder, Object[] args, out Object result)
        {
            MethodInfo method = FindMethod(binder.Name, args.Select(a => a.GetType()).ToArray());
            if (method == null) { result = null; return false; }
            result = method.Invoke(null, args);
            return true;
        }

        #region 私有扩展方法
        private MethodInfo FindMethod(String name, Type[] paramTypes)
        {
            return m_type.DeclaredMethods.FirstOrDefault(mi => mi.IsPublic && mi.IsStatic && mi.Name == name && ParametersMatch(mi.GetParameters(), paramTypes));
        }

        private Boolean ParametersMatch(ParameterInfo[] parameters, Type[] paramTypes)
        {
            if (parameters.Length != paramTypes.Length) return false;
            for (Int32 i = 0; i < parameters.Length; i++)
                if (parameters[i].ParameterType != paramTypes[i]) return false;
            return true;
        }

        private FieldInfo FindField(String name)
        {
            return m_type.DeclaredFields.FirstOrDefault(fi => fi.IsPublic && fi.IsStatic && fi.Name == name);
        }

        private PropertyInfo FindProperty(String name, Boolean get)
        {
            if (get)
                return m_type.DeclaredProperties.FirstOrDefault(
                   pi => pi.Name == name && pi.GetMethod != null &&
                   pi.GetMethod.IsPublic && pi.GetMethod.IsStatic);

            return m_type.DeclaredProperties.FirstOrDefault(
               pi => pi.Name == name && pi.SetMethod != null &&
                  pi.SetMethod.IsPublic && pi.SetMethod.IsStatic);
        }
        #endregion
    }
}
