using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameFramework
{
    public class AssemblyInfo
    {
        public Assembly Assembly { get; private set; }
        public readonly List<Type> AssemblyTypeList = new List<Type>();
        public readonly GameFrameworkMultiDictionary<Type, Type> AssemblyTypeGroupList = new GameFrameworkMultiDictionary<Type, Type>();

        public void Load(Assembly assembly)
        {
            Assembly = assembly;
            Type[] assemblyTypes = Assembly.GetTypes();
            foreach (Type type in assemblyTypes)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                Type[] interfaces = type.GetInterfaces();
                foreach (Type interfaceType in interfaces)
                {
                    AssemblyTypeGroupList.Add(interfaceType, type);
                }
            }
            AssemblyTypeList.AddRange(assemblyTypes);
        }

        public void UnLoad()
        {
            AssemblyTypeList.Clear();
            AssemblyTypeGroupList.Clear();
        }
    }
}
