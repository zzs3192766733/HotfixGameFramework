using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        private IGameModule _uiGameModelSys;
        public IGameModule UIGameModelSys => _uiGameModelSys;
    }
}

