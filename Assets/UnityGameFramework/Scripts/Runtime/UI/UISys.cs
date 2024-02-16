namespace UnityGameFramework.Runtime
{
    public class UISys : AbsGameModuleMgr<UISys>
    {
        protected override void Init()
        {
            //不需要进行System的注册，框架会自动为UI注册对应的System
            //只需要处理相应的Model和Utility即可
            //RegisterModel();
            //RegisterUtility();
        }
        
        
        protected override void UnLoad(int assemblyName)
        {
            
        }

        protected override void Load(int assemblyName)
        {
            
        }
    }
}

