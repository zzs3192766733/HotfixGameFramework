using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public interface IUpdateSingleton : ISingleton
    {
        public void Update(float elapseSeconds, float realElapseSeconds);
    }
}

