using GameFramework;
using UnityEngine;

/// <summary>
/// 游戏入口。
/// </summary>
namespace GameMain.Runtime
{
    public partial class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            GameEntry[] allGameEntries = GameObject.FindObjectsOfType<GameEntry>();
            if (allGameEntries.Length > 1)
            {
                DestroyImmediate(this.gameObject);
                return;
            }

            SingletonSystem.Initialize();
            AssemblyManager.LoadAssembly("GameMain.Runtime".GetHashCode(), typeof(GameEntry).Assembly);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GameModule.InitFrameWorkComponents();
            GameModule.InitCustomComponents();
        }

        private void Update()
        {
            //单例模块Update,与框架层的Update不同单独Mono调用
            SingletonSystem.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }
}