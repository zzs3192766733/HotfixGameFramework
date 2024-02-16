using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameFramework
{
    public static class AssemblyManager
    {
        public static event Action<int> OnLoadAssemblyEvent;
        public static event Action<int> OnUnLoadAssemblyEvent;
        public static event Action<int> OnReLoadAssemblyEvent;
        private static Dictionary<int, AssemblyInfo> AssemblyList = new Dictionary<int, AssemblyInfo>();
        public static void LoadAssembly(int assemblyName, Assembly assembly)
        {
            bool isReload = false;
            if (!AssemblyList.TryGetValue(assemblyName, out AssemblyInfo assemblyInfo))
            {
                assemblyInfo = new AssemblyInfo();
                AssemblyList.Add(assemblyName, assemblyInfo);
            }
            else
            {
                isReload = true;
                assemblyInfo.UnLoad();
                if (OnUnLoadAssemblyEvent != null)
                {
                    OnUnLoadAssemblyEvent(assemblyName);
                }
            }

            assemblyInfo.Load(assembly);

            if (OnLoadAssemblyEvent != null)
            {
                OnLoadAssemblyEvent(assemblyName);
            }

            if (isReload && OnReLoadAssemblyEvent != null)
            {
                OnReLoadAssemblyEvent(assemblyName);
            }
        }
        public static IEnumerable<int> ForeachAssemblyName()
        {
            foreach (int assemblyName in AssemblyList.Keys)
            {
                yield return assemblyName;
            }
        }
        public static IEnumerable<Type> Foreach()
        {
            foreach (AssemblyInfo assemblyInfo in AssemblyList.Values)
            {
                foreach (Type type in assemblyInfo.AssemblyTypeList)
                {
                    yield return type;
                }
            }
        }
        public static IEnumerable<Type> Foreach(int assemblyName)
        {
            if (!AssemblyList.TryGetValue(assemblyName, out AssemblyInfo assemblyInfo))
                yield break;
            foreach (Type type in assemblyInfo.AssemblyTypeList) { yield return type; }
        }
        public static IEnumerable<Type> Foreach(Type findType)
        {
            foreach (AssemblyInfo assemblyInfo in AssemblyList.Values)
            {
                if (!assemblyInfo.AssemblyTypeGroupList.TryGetValue(findType, out GameFrameworkLinkedListRange<Type> assemblyLoad))
                    yield break;

                foreach (Type type in assemblyLoad)
                {
                    yield return type;
                }
            }
        }
        public static IEnumerable<Type> Foreach(int assemblyName, Type findType)
        {
            if (!AssemblyList.TryGetValue(assemblyName, out AssemblyInfo assemblyInfo))
                yield break;

            if (!assemblyInfo.AssemblyTypeGroupList.TryGetValue(findType, out GameFrameworkLinkedListRange<Type> classList))
                yield break;

            foreach (Type type in classList)
                yield return type;
        }
        public static Assembly GetAssembly(int assemblyName)
        {
            return !AssemblyList.TryGetValue(assemblyName, out AssemblyInfo assemblyInfo) ? null : assemblyInfo.Assembly;
        }
        public static void Dispose()
        {
            foreach (AssemblyInfo assemblyInfo in AssemblyList.Values)
            {
                assemblyInfo.UnLoad();
            }

            AssemblyList.Clear();

            if (OnLoadAssemblyEvent != null)
            {
                foreach (var @delegate in OnLoadAssemblyEvent.GetInvocationList())
                {
                    OnLoadAssemblyEvent -= @delegate as Action<int>;
                }
            }

            if (OnUnLoadAssemblyEvent != null)
            {
                foreach (var @delegate in OnUnLoadAssemblyEvent.GetInvocationList())
                {
                    OnUnLoadAssemblyEvent -= @delegate as Action<int>;
                }
            }

            if (OnReLoadAssemblyEvent != null)
            {
                foreach (var @delegate in OnReLoadAssemblyEvent.GetInvocationList())
                {
                    OnReLoadAssemblyEvent -= @delegate as Action<int>;
                }
            }

            OnLoadAssemblyEvent = null;
            OnUnLoadAssemblyEvent = null;
            OnReLoadAssemblyEvent = null;
        }



        #region GetTypeCache


        private static readonly System.Reflection.Assembly[] s_Assemblies = null;
        private static readonly Dictionary<string, Type> s_CachedTypes = new Dictionary<string, Type>(StringComparer.Ordinal);
        static AssemblyManager()
        {
            s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// 获取已加载的程序集。
        /// </summary>
        /// <returns>已加载的程序集。</returns>
        public static System.Reflection.Assembly[] GetAssemblies()
        {
            return s_Assemblies;
        }

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <returns>已加载的程序集中的所有类型。</returns>
        public static Type[] GetTypes()
        {
            List<Type> results = new List<Type>();
            foreach (System.Reflection.Assembly assembly in s_Assemblies)
            {
                results.AddRange(assembly.GetTypes());
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <param name="results">已加载的程序集中的所有类型。</param>
        public static void GetTypes(List<Type> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (System.Reflection.Assembly assembly in s_Assemblies)
            {
                results.AddRange(assembly.GetTypes());
            }
        }

        /// <summary>
        /// 获取已加载的程序集中的指定类型。
        /// </summary>
        /// <param name="typeName">要获取的类型名。</param>
        /// <returns>已加载的程序集中的指定类型。</returns>
        public static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new GameFrameworkException("Type name is invalid.");
            }

            Type type = null;
            if (s_CachedTypes.TryGetValue(typeName, out type))
            {
                return type;
            }

            type = Type.GetType(typeName);
            if (type != null)
            {
                s_CachedTypes.Add(typeName, type);
                return type;
            }

            foreach (System.Reflection.Assembly assembly in s_Assemblies)
            {
                type = Type.GetType(Utility.Text.Format("{0}, {1}", typeName, assembly.FullName));
                if (type != null)
                {
                    s_CachedTypes.Add(typeName, type);
                    return type;
                }
            }

            return null;
        }

        #endregion
    }
}
