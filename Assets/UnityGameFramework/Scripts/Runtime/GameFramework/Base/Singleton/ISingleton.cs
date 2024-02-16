using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFramework
{
    public interface ISingleton : IDisposable 
    {
        public bool IsDisposed { get; set; }
        UniTask Initialize();
    }
}

