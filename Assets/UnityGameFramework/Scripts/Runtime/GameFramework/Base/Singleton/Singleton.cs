using Cysharp.Threading.Tasks;

namespace GameFramework
{
    public abstract class Singleton<T> : ISingleton where T : ISingleton, new()
    {
        public bool IsDisposed { get; set; }
        public static T Instance { get; private set; }

        private void RegisterSingleton(ISingleton singleton)
        {
            Instance = (T)singleton;
            IsDisposed = false;
            AssemblyManager.OnLoadAssemblyEvent += Load;
            AssemblyManager.OnUnLoadAssemblyEvent += UnLoad;
        }

        protected abstract void UnLoad(int assemblyName);
        protected abstract void Load(int assemblyName);

        public virtual UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }
        public virtual void Dispose()
        {
            IsDisposed = true;
            Instance = default;
            AssemblyManager.OnLoadAssemblyEvent -= Load;
            AssemblyManager.OnUnLoadAssemblyEvent -= UnLoad;
        }
    }
}

