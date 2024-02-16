using cfg;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using UnityGameFramework.Runtime;

public class LuBanDataTableComponent : UnityGameFramework.Runtime.GameFrameworkComponent
{
#if  UNITY_EDITOR
    [SerializeField] private List<string> fileNameList = new List<string>();
    [SerializeField] private List<long> sizeList = new List<long>();
#endif
    private Tables _tables;
    public Tables AllTables => _tables;



    public void InitializeLoadData()
    {
        _tables = new Tables(Loader);
    }

    private JSONNode Loader(string fileName)
    {
        TextAsset textAsset = GameEntry.GetComponent<ResourceComponent>().LoadAsset<TextAsset>(fileName);
        string json = textAsset.text;
        if (textAsset != null)
        {
#if UNITY_EDITOR
            fileNameList.Add(fileName);
            sizeList.Add(textAsset.dataSize);
#endif
            GameEntry.GetComponent<ResourceComponent>().UnloadAsset(textAsset);
            return JSON.Parse(json);
        }
        return null;
    }



    private void OnDestroy()
    {
        _tables = null;
    }
}
