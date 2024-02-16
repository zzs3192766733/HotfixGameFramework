using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class UIAbstractSystem : AbstractSystem
    {
        public virtual void OnUIInit(object userData)
        {
            
        }
        
        public virtual void OnUIRecycle()
        {
            
        }
        
        public virtual void OnUIOpen(object userData)
        {
            
        }
        
        public virtual void OnUIClose(object userData)
        {
            
        }

        public virtual void OnUIPause()
        {
            
        }
        
        public virtual void OnUIResume()
        {
            
        }
        
        public virtual void OnUICover()
        {
            
        }

        public virtual void OnUIReveal()
        {
            
        }

        public virtual void OnUIRefocus(object userData)
        {
            
        }

        public virtual void OnUIUpdate(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        public virtual void OnUIDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            
        }

        public virtual void InternalSetUIVisible(bool visible)
        {
            
        }
    }

    public abstract class UIAbstractSystem<T> : UIAbstractSystem where T : UIFormLogic
    {
        protected T currUIFormLogic;
        protected UIAbstractSystem(T uiFormLogic)
        {
            currUIFormLogic = uiFormLogic;
            GameEntry.GetComponent<UIComponent>().UIGameModelSys.RegisterSystem(this);
        }
    }
}