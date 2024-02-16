using Cysharp.Threading.Tasks;
using GameFramework;
using GameMain.Runtime;
using UGFExtensions.Await;
using UnityGameFramework.Runtime;

namespace GameMain.Hotfix
{

    public class GameApp
    {
        /// <summary>
        /// 热更域App主入口。
        /// </summary>
        /// <param name="objects"></param>
        public static void Entrance(object[] objects)
        {
            AssemblyManager.LoadAssembly("GameMain.Hotfix".GetHashCode(), typeof(GameApp).Assembly);
            Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
            Log.Warning("======= Entrance GameApp =======");
            StartGameLogic().Forget();
        }

        /// <summary>
        /// 开始游戏业务层逻辑。
        /// <remarks>显示UI、加载场景等。</remarks>
        /// </summary>
        private static async UniTaskVoid StartGameLogic()
        {
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 关闭游戏。
        /// </summary>
        /// <param name="shutdownType">关闭游戏框架类型。</param>
        public static void Shutdown(ShutdownType shutdownType)
        {
            if (shutdownType == ShutdownType.None)
            {

            }
            else if (shutdownType == ShutdownType.Restart)
            {

            }
            else
            {

            }

            UnityGameFramework.Runtime.GameEntry.Shutdown(shutdownType);
        }
    }
}