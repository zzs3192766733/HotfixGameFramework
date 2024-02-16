using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 数据中心系统。
    /// </summary>
    public class DataCenterSys:AbsGameModuleMgr<DataCenterSys>,IUpdateSingleton
    {
        protected override void Init()
        {
            
        }

        protected override void UnLoad(int assemblyName)
        {
            
        }

        protected override void Load(int assemblyName)
        {
            
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            TProfiler.BeginFirstSample("DataCenterSys");
            TProfiler.EndFirstSample();
        }
    }
}