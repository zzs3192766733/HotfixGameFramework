using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"UnityGameFramework.Runtime.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GameMain.Hotfix.GameApp.<StartGameLogic>d__1>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GameMain.Hotfix.GameApp.<StartGameLogic>d__1>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.TaskPool<object>
	// System.Func<int>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GameMain.Hotfix.GameApp.<StartGameLogic>d__1>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GameMain.Hotfix.GameApp.<StartGameLogic>d__1&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GameMain.Hotfix.GameApp.<StartGameLogic>d__1>(GameMain.Hotfix.GameApp.<StartGameLogic>d__1&)
	}
}