using System;
using System.Collections.Generic;
using System.IO;
using UGFExtensions.UGUIExtension;
using UnityEditor;
using UnityEngine;

namespace UGFExtensions.Editor
{
    /// <summary>
    /// 实体与界面代码生成器
    /// </summary>
    public class EntityAndUIFormCodeGenerator : EditorWindow
    {
        private enum GenCodeType
        {
            Entity,
            UIForm
        }

        [SerializeField] private List<GameObject> m_GameObjects = new List<GameObject>();

        private SerializedObject m_SerializedObject;
        private SerializedProperty m_SerializedProperty;

        private GenCodeType m_GenCodeType;

        /// <summary>
        /// 是否生成主体逻辑代码
        /// </summary>
        private bool m_IsGenMainLogicCode = true;

        /// <summary>
        /// 是否生成自动绑定组件代码
        /// </summary>
        private bool m_IsGenAutoBindCode = true;

        /// <summary>
        /// 是否生成实体数据代码
        /// </summary>
        private bool m_IsGenEntityDataCode = true;

        /// <summary>
        /// 是否生成UI系统代码
        /// </summary>
        private bool m_IsGenUIFormSystemCode = true;

        //各种类型的代码生成后的路径

        private const string EntityLogicCodePath = "Assets/GameMain/Scripts/HotFix/Entity/EntityLogic";
        private const string EntityDataCodePath = "Assets/GameMain/Scripts/HotFix/Entity/EntityData";


        private const string UILogicFormCodePath = "Assets/GameMain/Scripts/HotFix/UI/UILogicForm";
        private const string UIFormSystemCodePath = "Assets/GameMain/Scripts/HotFix/UI/UIFormSystem";

        [MenuItem("Tools/实体与界面代码生成器", priority = 1)]
        public static void OpenCodeGeneratorWindow()
        {
            EntityAndUIFormCodeGenerator window = GetWindow<EntityAndUIFormCodeGenerator>(true, "实体与界面代码生成器");
            window.minSize = new Vector2(300f, 300f);
        }

        private void OnEnable()
        {
            m_SerializedObject = new SerializedObject(this);
            m_SerializedProperty = m_SerializedObject.FindProperty("m_GameObjects");
        }

        private void OnGUI()
        {
            //绘制GameObject列表
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SerializedProperty, true);
            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedObject.ApplyModifiedProperties();
            }

            //绘制自动生成代码类型的弹窗
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("自动生成的代码类型：", GUILayout.Width(140f));
            m_GenCodeType = (GenCodeType)EditorGUILayout.EnumPopup(m_GenCodeType, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            //绘制代码生成路径文本
            EditorGUILayout.LabelField("自动生成的代码路径：", GUILayout.Width(140f));
            EditorGUILayout.BeginVertical();
            switch (m_GenCodeType)
            {
                case GenCodeType.Entity:
                    EditorGUILayout.LabelField("实体逻辑(Logic)代码路径：", EntityLogicCodePath);
                    EditorGUILayout.LabelField("实体数据(Data)代码路径：", EntityDataCodePath);
                    break;
                case GenCodeType.UIForm:
                    EditorGUILayout.LabelField("窗体逻辑(Logic)代码路径：", UILogicFormCodePath);
                    EditorGUILayout.LabelField("窗体系统(System)代码路径：", UIFormSystemCodePath);
                    break;
            }

            EditorGUILayout.EndVertical();

            //绘制各个选项
            m_IsGenMainLogicCode = GUILayout.Toggle(m_IsGenMainLogicCode, "生成主体逻辑代码", GUILayout.Width(150f));
            m_IsGenAutoBindCode = GUILayout.Toggle(m_IsGenAutoBindCode, "生成自动绑定组件代码", GUILayout.Width(150f));

            if (m_GenCodeType == GenCodeType.Entity)
            {
                m_IsGenEntityDataCode = GUILayout.Toggle(m_IsGenEntityDataCode, "生成实体数据代码");
            }
            else
            {
                m_IsGenUIFormSystemCode = GUILayout.Toggle(m_IsGenUIFormSystemCode, "生成UI系统代码");
            }

            //绘制生成代码的按钮
            if (GUILayout.Button("生成代码", GUILayout.Width(100f)))
            {
                if (m_GameObjects.Count == 0)
                {
                    EditorUtility.DisplayDialog("警告", "请选择实体或界面的游戏物体", "OK");
                    return;
                }

                if (m_GenCodeType == GenCodeType.Entity)
                {
                    GenEntityCode();
                }
                else
                {
                    GenUIFormCode();
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("提示", "代码生成完毕", "OK");
            }
        }

        private void GenEntityCode()
        {
            //根据是否为热更新实体来决定一些参数
            string codePath = EntityLogicCodePath;
            string nameSpace = "GameMain.Hotfix";

            foreach (GameObject go in m_GameObjects)
            {
                if (m_IsGenMainLogicCode)
                {
                    GenEntityMainLogicCode(codePath, go, nameSpace);
                }

                if (m_IsGenEntityDataCode)
                {
                    GenEntityDataCode(EntityDataCodePath, go, nameSpace);
                }

                if (m_IsGenAutoBindCode)
                {
                    GenAutoBindCode(codePath, go, nameSpace,"Logic");
                }
            }
        }

        private void GenUIFormCode()
        {
            //根据是否为热更新界面来决定一些参数
            string codepath = UILogicFormCodePath;
            string nameSpace = "GameMain.Hotfix";
            string logicBaseClass = "GameMain.Runtime.UIFormLogic";


            foreach (GameObject go in m_GameObjects)
            {
                if (m_IsGenMainLogicCode)
                {
                    GenUIFormMainLogicCode(codepath, go, nameSpace, logicBaseClass);
                }

                if (m_IsGenAutoBindCode)
                {
                    GenAutoBindCode(codepath, go, nameSpace);
                }

                if (m_IsGenUIFormSystemCode)
                {
                    GenUIFormSystemCode(UIFormSystemCodePath, go, nameSpace);
                }
            }
        }

        private void GenEntityMainLogicCode(string codePath, GameObject go, string nameSpace)
        {
            string accessModifier = "protected";
            if (!Directory.Exists($"{codePath}/"))
            {
                Directory.CreateDirectory($"{codePath}/");
            }

            if (File.Exists($"{codePath}/{go.name}Logic.cs"))
            {
                if (!EditorUtility.DisplayDialog(
                        "创建代码提示",
                        $"当前目录下已经存在{go.name}Logic.cs 继续创建将覆盖当前目录下文件，是否执行覆盖操作",
                        "确认",
                        "取消"))
                {
                    return;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter($"{codePath}/{go.name}Logic.cs"))
                    {
                        sw.WriteLine("using UnityEngine;");
                        sw.WriteLine("using GameMain.Runtime;");
                        sw.WriteLine("");

                        sw.WriteLine("//自动生成于：" + DateTime.Now);

                        //命名空间
                        sw.WriteLine("namespace " + nameSpace);
                        sw.WriteLine("{");
                        sw.WriteLine("");

                        //类名
                        sw.WriteLine($"\tpublic partial class {go.name}Logic : EntityLogic<{go.name}Data>");
                        sw.WriteLine("\t{");
                        sw.WriteLine("");

                        //OnInit方法 初始化实体
                        sw.WriteLine($"\t\tprotected override void OnInit(object userData)");
                        sw.WriteLine("\t\t{");
                        sw.WriteLine("\t\t\tbase.OnInit(userData);");
                        if (m_IsGenAutoBindCode)
                        {
                            sw.WriteLine("\t\t\tGetBindComponents(gameObject);");
                        }
                        sw.WriteLine("\t\t}");
                        
                        //OnShow方法 获取实体数据
                        sw.WriteLine($"\t\t{accessModifier} override void OnShow(object userData)");
                        sw.WriteLine("\t\t{");
                        sw.WriteLine("\t\t\tbase.OnShow(userData);");
                        sw.WriteLine("\t\t}");
                        sw.WriteLine("\t}");
                        sw.WriteLine("}");
                    }
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter($"{codePath}/{go.name}Logic.cs"))
                {
                    sw.WriteLine("using UnityEngine;");
                    sw.WriteLine("using GameMain.Runtime;");
                    sw.WriteLine("");

                    sw.WriteLine("//自动生成于：" + DateTime.Now);

                    //命名空间
                    sw.WriteLine("namespace " + nameSpace);
                    sw.WriteLine("{");
                    sw.WriteLine("");

                    //类名
                    sw.WriteLine($"\tpublic partial class {go.name}Logic : EntityLogic<{go.name}Data>");
                    sw.WriteLine("\t{");
                    sw.WriteLine("");

                    //OnInit方法 初始化实体
                    sw.WriteLine($"\t\tprotected override void OnInit(object userData)");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\tbase.OnInit(userData);");
                    if (m_IsGenAutoBindCode)
                    {
                        sw.WriteLine("\t\t\tGetBindComponents(gameObject);");
                    }
                    sw.WriteLine("\t\t}");
                        
                    //OnShow方法 获取实体数据
                    sw.WriteLine($"\t\t{accessModifier} override void OnShow(object userData)");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\tbase.OnShow(userData);");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("\t}");
                    sw.WriteLine("}");
                }
            }
        }

        private void GenEntityDataCode(string codePath, GameObject go, string nameSpace)
        {
            string dataBaseClass = "EntityData";
            string entityDataName = go.name + "Data";

            if (!Directory.Exists($"{codePath}/"))
            {
                Directory.CreateDirectory($"{codePath}/");
            }

            if (File.Exists($"{codePath}/{entityDataName}.cs"))
            {
                if (!EditorUtility.DisplayDialog(
                        "创建代码提示",
                        $"当前目录下已经存在{entityDataName}.cs 继续创建将覆盖当前目录下文件，是否执行覆盖操作",
                        "确认",
                        "取消"))
                {
                    return;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter($"{codePath}/{entityDataName}.cs"))
                    {
                        sw.WriteLine("using UnityEngine;");
                        sw.WriteLine("using System;");
                        sw.WriteLine("using GameFramework;");
                        sw.WriteLine("using GameMain.Runtime;");
                        sw.WriteLine("");

                        sw.WriteLine("//自动生成于：" + DateTime.Now);

                        //命名空间
                        sw.WriteLine("namespace " + nameSpace);
                        sw.WriteLine("{");
                        sw.WriteLine("");

                        //类名
                        sw.WriteLine("\t[Serializable]");
                        sw.WriteLine($"\tpublic class {entityDataName} : {dataBaseClass}");
                        sw.WriteLine("\t{");
                        sw.WriteLine("");

                        sw.WriteLine("\t\t//此处手动添加创建数据方法");
                        sw.WriteLine($"\t\t//public static {entityDataName} Create(int id,int typeId)");
                        sw.WriteLine("\t\t//{");
                        sw.WriteLine($"\t\t\t//{entityDataName} data = ReferencePool.Acquire<{entityDataName}>();");
                        sw.WriteLine("\t\t\t//return data;");
                        sw.WriteLine("\t\t//}");


                        //Clear方法
                        sw.WriteLine("\t\tpublic override void Clear()");
                        sw.WriteLine("\t\t{");
                        sw.WriteLine("\t\t\tbase.Clear();");
                        sw.WriteLine("\t\t}");
                        sw.WriteLine("");
                        sw.WriteLine("\t}");
                        sw.WriteLine("}");
                    }
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter($"{codePath}/{entityDataName}.cs"))
                {
                    sw.WriteLine("using UnityEngine;");
                    sw.WriteLine("using System;");
                    sw.WriteLine("using GameFramework;");
                    sw.WriteLine("using GameMain.Runtime;");
                    sw.WriteLine("");

                    sw.WriteLine("//自动生成于：" + DateTime.Now);

                    //命名空间
                    sw.WriteLine("namespace " + nameSpace);
                    sw.WriteLine("{");
                    sw.WriteLine("");

                    //类名
                    sw.WriteLine("\t[Serializable]");
                    sw.WriteLine($"\tpublic class {entityDataName} : {dataBaseClass}");
                    sw.WriteLine("\t{");
                    sw.WriteLine("");

                    sw.WriteLine("\t\t//此处手动添加创建数据方法");
                    sw.WriteLine($"\t\t//public static {entityDataName} Create(int id,int typeId)");
                    sw.WriteLine("\t\t//{");
                    sw.WriteLine($"\t\t\t//{entityDataName} data = ReferencePool.Acquire<{entityDataName}>();");
                    sw.WriteLine("\t\t\t//return null");
                    sw.WriteLine("\t\t//}");


                    //Clear方法
                    sw.WriteLine("\t\tpublic override void Clear()");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\tbase.Clear();");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("");
                    sw.WriteLine("\t}");
                    sw.WriteLine("}");
                }
            }
        }

        private void GenUIFormSystemCode(string codePath, GameObject go, string nameSpace)
        {
            if (!Directory.Exists($"{codePath}/"))
            {
                Directory.CreateDirectory($"{codePath}/");
            }

            if (File.Exists($"{codePath}/{go.name}System.cs"))
            {
                if (!EditorUtility.DisplayDialog(
                        "创建代码提示",
                        $"当前目录下已经存在{go.name}System.cs 继续创建将覆盖当前目录下文件，是否执行覆盖操作",
                        "确认",
                        "取消"))
                {
                    return;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter($"{codePath}/{go.name}System.cs"))
                    {
                        sw.WriteLine("using UnityEngine;");
                        sw.WriteLine("using UnityGameFramework.Runtime;");
                        sw.WriteLine("");
                        sw.WriteLine("//自动生成于：" + DateTime.Now);

                        //命名空间
                        sw.WriteLine("namespace " + nameSpace);
                        sw.WriteLine("{");
                        sw.WriteLine("");

                        //类名
                        sw.WriteLine($"\tpublic class {go.name}System : UIAbstractSystem<{go.name}>");
                        sw.WriteLine("\t{");
                        sw.WriteLine("");

                        //构造函数
                        sw.WriteLine($"\t\tpublic {go.name}System({go.name} uiFormLogic):base(uiFormLogic)");
                        sw.WriteLine("\t\t{");
                        sw.WriteLine("\t\t}");

                        //初始化函数
                        sw.WriteLine("");
                        sw.WriteLine("\t\t//系统初始化调用，非UI界面初始化(OnUIInit)");
                        sw.WriteLine("\t\tprotected override void OnInit()");
                        sw.WriteLine("\t\t{");
                        sw.WriteLine("\t\t\t");
                        sw.WriteLine("\t\t}");

                        //释放函数
                        sw.WriteLine("");
                        sw.WriteLine("\t\tpublic override void Dispose()");
                        sw.WriteLine("\t\t{");
                        sw.WriteLine("\t\t\t");
                        sw.WriteLine("\t\t}");
                        sw.WriteLine("\t}");
                        sw.WriteLine("}");
                    }
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter($"{codePath}/{go.name}System.cs"))
                {
                    sw.WriteLine("using UnityEngine;");
                    sw.WriteLine("using UnityGameFramework.Runtime;");
                    sw.WriteLine("");
                    sw.WriteLine("//自动生成于：" + DateTime.Now);

                    //命名空间
                    sw.WriteLine("namespace " + nameSpace);
                    sw.WriteLine("{");
                    sw.WriteLine("");

                    //类名
                    sw.WriteLine($"\tpublic class {go.name}System : UIAbstractSystem<{go.name}>");
                    sw.WriteLine("\t{");
                    sw.WriteLine("");

                    //构造函数
                    sw.WriteLine($"\t\tpublic {go.name}System({go.name} uiFormLogic):base(uiFormLogic)");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t}");

                    //初始化函数
                    sw.WriteLine("");
                    sw.WriteLine("\t\t//系统初始化调用，非UI界面初始化(OnUIInit)");
                    sw.WriteLine("\t\tprotected override void OnInit()");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\t");
                    sw.WriteLine("\t\t}");

                    //释放函数
                    sw.WriteLine("");
                    sw.WriteLine("\t\tpublic override void Dispose()");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\t");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("\t}");
                    sw.WriteLine("}");
                }
            }
        }


        private void GenUIFormMainLogicCode(string codePath, GameObject go, string nameSpace, string logicBaseClass)
        {
            string initParam = string.Empty;
            string baseInitParam = string.Empty;
            string accessModifier = "protected";

            if (!Directory.Exists($"{codePath}/"))
            {
                Directory.CreateDirectory($"{codePath}/");
            }

            if (File.Exists($"{codePath}/{go.name}.cs"))
            {
                if (!EditorUtility.DisplayDialog(
                        "创建代码提示",
                        $"当前目录下已经存在{go.name}.cs 继续创建将覆盖当前目录下文件，是否执行覆盖操作",
                        "确认",
                        "取消"))
                {
                    return;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter($"{codePath}/{go.name}.cs"))
                    {
                        sw.WriteLine("using UnityGameFramework.Runtime;");
                        sw.WriteLine("using UnityEngine;");
                        sw.WriteLine("");
                        sw.WriteLine("//自动生成于：" + DateTime.Now);

                        //命名空间
                        sw.WriteLine("namespace " + nameSpace);
                        sw.WriteLine("{");
                        sw.WriteLine("");

                        //类名
                        sw.WriteLine($"\tpublic partial class {go.name} : {logicBaseClass}");
                        sw.WriteLine("\t{");
                        sw.WriteLine("");

                        //GetSystem
                        sw.WriteLine($"\t\t{accessModifier} override UIAbstractSystem GetSystem({initParam})");
                        sw.WriteLine("\t\t{");
                        if (m_IsGenAutoBindCode)
                        {
                            sw.WriteLine("\t\t\tGetBindComponents(gameObject);");
                        }
                        sw.WriteLine($"\t\t\treturn new {go.name}System(this);");
                        sw.WriteLine("\t\t}");
                        sw.WriteLine("\t}");
                        sw.WriteLine("}");
                    }
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter($"{codePath}/{go.name}.cs"))
                {
                    sw.WriteLine("using UnityGameFramework.Runtime;");
                    sw.WriteLine("using UnityEngine;");
                    sw.WriteLine("");
                    sw.WriteLine("//自动生成于：" + DateTime.Now);

                    //命名空间
                    sw.WriteLine("namespace " + nameSpace);
                    sw.WriteLine("{");
                    sw.WriteLine("");

                    //类名
                    sw.WriteLine($"\tpublic partial class {go.name} : {logicBaseClass}");
                    sw.WriteLine("\t{");
                    sw.WriteLine("");

                    //GetSystem
                    sw.WriteLine($"\t\t{accessModifier} override UIAbstractSystem GetSystem({initParam})");
                    sw.WriteLine("\t\t{");
                    if (m_IsGenAutoBindCode)
                    {
                        sw.WriteLine("\t\t\tGetBindComponents(gameObject);");
                    }
                    sw.WriteLine($"\t\t\treturn new {go.name}System(this);");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("\t}");
                    sw.WriteLine("}");
                }
            }
        }

        private void GenAutoBindCode(string codePath, GameObject go, string nameSpace, string nameEx = "")
        {
            ComponentAutoBindTool bindTool = go.GetComponent<ComponentAutoBindTool>();
            if (bindTool == null)
            {
                return;
            }

            if (!Directory.Exists($"{codePath}/BindComponents/"))
            {
                Directory.CreateDirectory($"{codePath}/BindComponents/");
            }

            using (StreamWriter sw = new StreamWriter($"{codePath}/BindComponents/{go.name}{nameEx}.BindComponents.cs"))
            {
                sw.WriteLine("using UnityEngine;");
                sw.WriteLine("using UGFExtensions;");
                if (m_GenCodeType == GenCodeType.UIForm)
                {
                    sw.WriteLine("using UnityEngine.UI;");
                }

                sw.WriteLine("");

                sw.WriteLine("//自动生成于：" + DateTime.Now);

                //命名空间
                sw.WriteLine("namespace " + nameSpace);
                sw.WriteLine("{");
                sw.WriteLine("");

                //类名
                sw.WriteLine($"\tpublic partial class {go.name}{nameEx}");
                sw.WriteLine("\t{");
                sw.WriteLine("");


                foreach (ComponentAutoBindTool.BindData data in bindTool.BindDatas)
                {
                    sw.WriteLine($"\t\t[HideInInspector] public {data.BindCom.GetType().Name} m_{data.Name};");
                }

                sw.WriteLine("");

                sw.WriteLine("\t\tprivate void GetBindComponents(GameObject go)");
                sw.WriteLine("\t\t{");

                //获取绑定的组件
                sw.WriteLine($"\t\t\tComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();");
                sw.WriteLine("");

                //根据索引获取

                for (int i = 0; i < bindTool.BindDatas.Count; i++)
                {
                    ComponentAutoBindTool.BindData data = bindTool.BindDatas[i];
                    string filedName = $"m_{data.Name}";
                    sw.WriteLine(
                        $"\t\t\t{filedName} = autoBindTool.GetBindComponent<{data.BindCom.GetType().Name}>({i});");
                }

                sw.WriteLine("\t\t}");

                sw.WriteLine("");

                sw.WriteLine("\t}");

                sw.WriteLine("}");
            }
        }
    }
}