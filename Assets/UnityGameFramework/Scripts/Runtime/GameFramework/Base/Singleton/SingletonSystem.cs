using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameFramework
{
    public static class SingletonSystem
    {
        private static readonly Queue<IUpdateSingleton> updateSingletons = new Queue<IUpdateSingleton>();

        private static readonly GameFrameworkOneToManyQueue<int, ISingleton> singletons =
            new GameFrameworkOneToManyQueue<int, ISingleton>();


        public static void Initialize()
        {
            AssemblyManager.OnLoadAssemblyEvent += Load;
            AssemblyManager.OnUnLoadAssemblyEvent += UnLoad;
        }

        private static void UnLoad(int assemblyName)
        {
            if (!singletons.TryGetValue(assemblyName, out var queue))
                return;
            UnLoad(queue);
            singletons.RemoveKey(assemblyName);
        }

        private static void Load(int assemblyName)
        {
            List<UniTask> tasks = new List<UniTask>();
            UnLoad(assemblyName);

            foreach (Type singletonType in AssemblyManager.Foreach(assemblyName, typeof(ISingleton)))
            {
                var instance = (ISingleton)Activator.CreateInstance(singletonType);
                MethodInfo registerMethodInfo = singletonType.BaseType?.GetMethod("RegisterSingleton",BindingFlags.Instance | BindingFlags.NonPublic);
                if (registerMethodInfo == null)
                {
                    registerMethodInfo = singletonType.BaseType?.BaseType?.GetMethod("RegisterSingleton",BindingFlags.Instance | BindingFlags.NonPublic);
                    if (registerMethodInfo == null)
                    {
                        Log.Error("存在多级派生，遍历二级并未发现目标函数!!!");
                        return;
                    }
                }
                MethodInfo initializeMethodInfo =
                    singletonType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
                MethodInfo onLoadMethodInfo =
                    singletonType.GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic);

                if (initializeMethodInfo != null)
                {
                    tasks.Add((UniTask)initializeMethodInfo.Invoke(instance, null));
                }

                registerMethodInfo?.Invoke(instance, new object[] { instance });
                onLoadMethodInfo?.Invoke(instance, new object[] { assemblyName });

                switch (instance)
                {
                    case IUpdateSingleton updateSingleton:
                        updateSingletons.Enqueue(updateSingleton);
                        break;
                    default:
                        break;
                }

                singletons.Enqueue(assemblyName, instance);
            }

            UniTask.WhenAll(tasks);
        }

        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            int updateCount = updateSingletons.Count;
            while (updateCount-- > 0)
            {
                IUpdateSingleton updateSingleton = updateSingletons.Dequeue();
                if (updateSingleton.IsDisposed)
                    continue;
                updateSingletons.Enqueue(updateSingleton);
                try
                {
                    updateSingleton.Update(elapseSeconds, realElapseSeconds);
                }
                catch (GameFrameworkException ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        private static void UnLoad(Queue<ISingleton> queue)
        {
            if (queue == null)
                return;
            while (queue.Count > 0)
            {
                try
                {
                    queue.Dequeue().Dispose();
                }
                catch (GameFrameworkException ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        public static void Dispose()
        {
            foreach (var item in singletons.Values)
            {
                UnLoad(item);
            }

            updateSingletons.Clear();
            singletons.Clear();
            AssemblyManager.OnLoadAssemblyEvent -= Load;
            AssemblyManager.OnUnLoadAssemblyEvent -= UnLoad;
        }
    }
}