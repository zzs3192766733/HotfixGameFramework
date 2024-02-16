using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using YooAsset;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Resource")]
    public class ResourceComponent : GameFrameworkComponent
    {
        #region Propreties
        private const int DefaultPriority = 0;
        
        /// <summary>
        /// 当前最新的包裹版本。
        /// </summary>
        public string PackageVersion { set; get; }
        
        private IResourceManager m_ResourceManager;
        private bool m_ForceUnloadUnusedAssets = false;
        private bool m_PreorderUnloadUnusedAssets = false;
        private bool m_PerformGCCollect = false;
        private AsyncOperation m_AsyncOperation = null;
        private float m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;
        
        [SerializeField]
        private float m_MinUnloadUnusedAssetsInterval = 60f;

        [SerializeField]
        private float m_MaxUnloadUnusedAssetsInterval = 300f;
        
        /// <summary>
        /// 资源包名称。
        /// </summary>
        public string PackageName = "DefaultPackage";
        
        /// <summary>
        /// 资源系统运行模式。
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

        /// <summary>
        /// 下载文件校验等级。
        /// </summary>
        public EVerifyLevel VerifyLevel = EVerifyLevel.Middle;
        
        [SerializeField]
        private ReadWritePathType m_ReadWritePathType = ReadWritePathType.Unspecified;
        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        [SerializeField]
        public long Milliseconds = 30;

        private GameFrameworkMultiDictionary<object, AssetOperationHandle> m_MulAssetOperationDic;

        public int m_DownloadingMaxNum = 2;
        /// <summary>
        /// 获取或设置同时最大下载数目。
        /// </summary>
        public int DownloadingMaxNum
        {
            get
            {
                return m_DownloadingMaxNum;
            }
            set
            {
                m_DownloadingMaxNum = value;
            }
        }

        public int m_FailedTryAgain = 3;
        public int FailedTryAgain
        {
            get
            {
                return m_FailedTryAgain;
            }
            set
            {
                m_FailedTryAgain = value;
            }
        }
        
        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion
        {
            get
            {
                return m_ResourceManager.ApplicableGameVersion;
            }
        }
        
        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion
        {
            get
            {
                return m_ResourceManager.InternalResourceVersion;
            }
        }
        
        /// <summary>
        /// 获取资源读写路径类型。
        /// </summary>
        public ReadWritePathType ReadWritePathType
        {
            get
            {
                return m_ReadWritePathType;
            }
        }
        
        /// <summary>
        /// 获取或设置无用资源释放的最小间隔时间，以秒为单位。
        /// </summary>
        public float MinUnloadUnusedAssetsInterval
        {
            get
            {
                return m_MinUnloadUnusedAssetsInterval;
            }
            set
            {
                m_MinUnloadUnusedAssetsInterval = value;
            }
        }

        /// <summary>
        /// 获取或设置无用资源释放的最大间隔时间，以秒为单位。
        /// </summary>
        public float MaxUnloadUnusedAssetsInterval
        {
            get
            {
                return m_MaxUnloadUnusedAssetsInterval;
            }
            set
            {
                m_MaxUnloadUnusedAssetsInterval = value;
            }
        }
        
        /// <summary>
        /// 获取无用资源释放的等待时长，以秒为单位。
        /// </summary>
        public float LastUnloadUnusedAssetsOperationElapseSeconds
        {
            get
            {
                return m_LastUnloadUnusedAssetsOperationElapseSeconds;
            }
        }
        
        /// <summary>
        /// 获取资源只读路径。
        /// </summary>
        public string ReadOnlyPath
        {
            get
            {
                return m_ResourceManager.ReadOnlyPath;
            }
        }

        /// <summary>
        /// 获取资源读写路径。
        /// </summary>
        public string ReadWritePath
        {
            get
            {
                return m_ResourceManager.ReadWritePath;
            }
        }
        
        /// <summary>
        /// 资源下载器，用于下载当前资源版本所有的资源包文件。
        /// </summary>
        public ResourceDownloaderOperation Downloader { get; set; }

        #endregion
        
        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_ResourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (m_ResourceManager == null)
            {
                Log.Fatal("YooAssetsManager component is invalid.");
                return;
            }

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");
#if !UNITY_EDITOR
                PlayMode = EPlayMode.OfflinePlayMode;
#endif
            }
            
            m_ResourceManager.SetReadOnlyPath(Application.streamingAssetsPath);
            if (m_ReadWritePathType == ReadWritePathType.TemporaryCache)
            {
                m_ResourceManager.SetReadWritePath(Application.temporaryCachePath);
            }
            else
            {
                if (m_ReadWritePathType == ReadWritePathType.Unspecified)
                {
                    m_ReadWritePathType = ReadWritePathType.PersistentData;
                }

                m_ResourceManager.SetReadWritePath(Application.persistentDataPath);
            }
            
            //初始化句柄
            InitializedOperationHandle();
            
            m_ResourceManager.PackageName = PackageName;
            m_ResourceManager.PlayMode = PlayMode;
            m_ResourceManager.VerifyLevel = VerifyLevel;
            m_ResourceManager.Milliseconds = Milliseconds;
            m_ResourceManager.InstanceRoot = transform;
            m_ResourceManager.HostServerURL = SettingsUtils.GetResDownLoadPath();
            m_ResourceManager.Initialize();
            Log.Info($"AssetsComponent Run Mode：{PlayMode}");
        }

        /// <summary>
        /// 初始化句柄
        /// </summary>
        private void InitializedOperationHandle()
        {
            m_MulAssetOperationDic = new GameFrameworkMultiDictionary<object, AssetOperationHandle>();
        }

        /// <summary>
        /// 初始化操作。
        /// </summary>
        /// <returns></returns>
        public InitializationOperation InitPackage()
        {
            m_ResourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (m_ResourceManager == null)
            {
                Log.Fatal("YooAssetsManager component is invalid.");
                return null;
            }
            return m_ResourceManager.InitPackage();
        }

        #region 加载资源
        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <typeparam name="T">要加载的资源类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            AssetOperationHandle assetOperationHandle = m_ResourceManager.LoadAssetGetOperation<T>(assetName);
            m_MulAssetOperationDic.Add(assetOperationHandle.AssetObject,assetOperationHandle);
            return assetOperationHandle.AssetObject as T;
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <typeparam name="T">要加载的资源类型。</typeparam>
        /// <returns>UniTask资源实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            AssetOperationHandle operationHandle = m_ResourceManager.LoadAssetAsyncGetOperation<T>(assetName);
            await operationHandle.ToUniTask(this);
            m_MulAssetOperationDic.Add(operationHandle.AssetObject,operationHandle);
            return operationHandle.AssetObject as T;
        }
        
        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <typeparam name="T">要加载的资源类型。</typeparam>
        /// <returns>UniTask资源实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string assetName,CancellationToken cancellationToken) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            AssetOperationHandle operationHandle = m_ResourceManager.LoadAssetAsyncGetOperation<T>(assetName);
            await operationHandle.ToUniTask(cancellationToken:cancellationToken).SuppressCancellationThrow();
            m_MulAssetOperationDic.Add(operationHandle.AssetObject,operationHandle);
            return operationHandle.AssetObject as T;
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAssetAsync(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData = null)
        {
            LoadAssetAsync(assetName, assetType, DefaultPriority, loadAssetCallbacks, userData);
        }
        
        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public async void LoadAssetAsync(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return;
            }

            if (loadAssetCallbacks == null)
            {
                Log.Warning("Asset CallBack is invalid.");
                return;
            }

            float duration = Time.time;
            ResourcePackage assetPackage = m_ResourceManager.GetCurrResourcePackage();
            AssetInfo assetInfo = assetPackage.GetAssetInfo(assetName);

            if (assetInfo == null)
            {
                string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", assetName);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotExist, errorMessage, userData);
                    return;
                }

                throw new GameFrameworkException(errorMessage);
            }

            OperationHandleBase handleBase;

            handleBase = assetPackage.LoadAssetAsync(assetName, assetType);
            
            await handleBase.ToUniTask(this);

            AssetOperationHandle handle = (AssetOperationHandle)handleBase;
            if (handle == null || handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
            {
                string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", assetName);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(assetName, LoadResourceStatus.NotReady, errorMessage, userData);
                    return;
                }

                throw new GameFrameworkException(errorMessage);
            }
            else
            {
                if (loadAssetCallbacks.LoadAssetSuccessCallback != null)
                {
                    duration = Time.time - duration;
                    loadAssetCallbacks.LoadAssetSuccessCallback(assetName, handle.AssetObject, duration, userData);
                    m_MulAssetOperationDic.Add(handle.AssetObject,handle);
                }
            }
        }

        /// <summary>
        /// 此函数的句柄需要自己管理
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <typeparam name="T">资源句柄</typeparam>
        /// <returns></returns>
        public AssetOperationHandle LoadAssetSyncGetHandle<T>(string assetName) where T : UnityEngine.Object
        {
            return m_ResourceManager.LoadAssetGetOperation<T>(assetName);
        }

        /// <summary>
        /// 此函数的句柄需要自己管理
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <typeparam name="T">资源句柄</typeparam>
        /// <returns></returns>
        public AssetOperationHandle LoadAssetAsyncGetHandle<T>(string assetName) where T : UnityEngine.Object
        {
            return m_ResourceManager.LoadAssetAsyncGetOperation<T>(assetName);
        }
        #endregion

        #region 卸载资源
        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            if (asset == null)
                return;
            if (m_MulAssetOperationDic.TryGetValue(asset,
                    out GameFrameworkLinkedListRange<AssetOperationHandle> handles))
            {
                if (handles.Count > 0)
                {
                    AssetOperationHandle handle = handles.First.Value;
                    m_MulAssetOperationDic.Remove(asset, handle);
                    handle.Release();
                }
            }
            //底层未对资源卸载管理，在此处处理
            //m_ResourceManager.UnloadAsset(asset);
        }
        #endregion

        public UpdatePackageVersionOperation UpdatePackageVersionAsync(bool appendTimeTicks = true, int timeout = 60)
        {
            var package = m_ResourceManager.GetCurrResourcePackage();
            return package.UpdatePackageVersionAsync(appendTimeTicks,timeout);
        }

        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60)
        {
            var package = m_ResourceManager.GetCurrResourcePackage();
            return package.UpdatePackageManifestAsync(packageVersion,timeout);
        }
        
        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain)
        {
            var package = m_ResourceManager.GetCurrResourcePackage();
            Downloader = package.CreateResourceDownloader(downloadingMaxNumber,failedTryAgain);
            return Downloader;
        }

        /// <summary>
        /// 清理包裹未使用的缓存文件
        /// </summary>
        public ClearUnusedCacheFilesOperation ClearUnusedCacheFilesAsync()
        {
            var package = m_ResourceManager.GetCurrResourcePackage();
            return package.ClearUnusedCacheFilesAsync();
        }
       
        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            m_ForceUnloadUnusedAssets = true;
            if (performGCCollect)
            {
                m_PerformGCCollect = true;
            }
        }

        public void ClearSandbox()
        {
            YooAssets.ClearSandbox();
        }

        private void Update()
        {
            m_LastUnloadUnusedAssetsOperationElapseSeconds += Time.unscaledDeltaTime;
            if (m_AsyncOperation == null && (m_ForceUnloadUnusedAssets || m_LastUnloadUnusedAssetsOperationElapseSeconds >= m_MaxUnloadUnusedAssetsInterval || m_PreorderUnloadUnusedAssets && m_LastUnloadUnusedAssetsOperationElapseSeconds >= m_MinUnloadUnusedAssetsInterval))
            {
                Log.Info("Unload unused assets...");
                m_ForceUnloadUnusedAssets = false;
                m_PreorderUnloadUnusedAssets = false;
                m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;
                m_AsyncOperation = Resources.UnloadUnusedAssets();
            }
            
            if (m_AsyncOperation is { isDone: true })
            {
                m_ResourceManager.UnloadUnusedAssets();
                m_AsyncOperation = null;
                if (m_PerformGCCollect)
                {
                    Log.Info("GC.Collect...");
                    m_PerformGCCollect = false;
                    GC.Collect();
                }
            }
        }
    }
}
