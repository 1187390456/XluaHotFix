using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : MonoBehaviour
{
    public static LuaManager Instance;

    private LuaEnv luaEnv = new LuaEnv();

    private void Awake()
    {
        Instance = this;

        luaEnv.AddLoader(CustomLoader);
    }

    /// <summary>
    /// 执行lua代码
    /// </summary>
    public void DoString(string chunk, string chunkName = "chunk", LuaTable env = null)
    {
        luaEnv.DoString(chunk, chunkName, env);
    }

    /// <summary>
    /// 调用lua方法
    /// </summary>
    public object[] CallLuaFunction(string luaName, string methodName, params object[] args)
    {
        LuaTable table = luaEnv.Global.Get<LuaTable>(luaName);
        LuaFunction function = table.Get<LuaFunction>(methodName);
        return function.Call(args);
    }

    private Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>(); // 路径 对应 lua字节数据

    /// <summary>
    /// 自定义Lua Loader
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private byte[] CustomLoader(ref string fileName)
    {
        // 获取lua所在的目录
        string luaPath = PathUtil.GetAssetBundleOutPath() + "/Lua";

        if (!Directory.Exists(luaPath)) Directory.CreateDirectory(luaPath);

        if (dict.ContainsKey(fileName)) return dict[fileName];

        return ProcessDir(new DirectoryInfo(luaPath), fileName);
    }

    /// <summary>
    /// 处理当前文件系统 返回一个字节数据
    /// </summary>
    private byte[] ProcessDir(FileSystemInfo fileSystemInfo, string fileName)
    {
        DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
        FileSystemInfo[] files = directoryInfo.GetFileSystemInfos();

        foreach (var item in files)
        {
            FileInfo file = item as FileInfo;
            if (file == null) ProcessDir(item, fileName); // 不是文件
            else
            {
                string tempName = item.Name.Split('.')[0];
                if (item.Extension == ".meta" || tempName != fileName) continue; // 不需要的文件

                byte[] bytes = File.ReadAllBytes(file.FullName);
                dict.Add(fileName, bytes);

                return bytes;
            }
        }
        return null;
    }

    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
                typeof(System.Object),
                typeof(UnityEngine.Object),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Color),
                typeof(Ray),
                typeof(Bounds),
                typeof(Ray2D),
                typeof(Time),
                typeof(GameObject),
                typeof(Component),
                typeof(Behaviour),
                typeof(Transform),
                typeof(Resources),
                typeof(TextAsset),
                typeof(Keyframe),
                typeof(AnimationCurve),
                typeof(AnimationClip),
                typeof(MonoBehaviour),
                typeof(ParticleSystem),
                typeof(SkinnedMeshRenderer),
                typeof(Renderer),
                typeof(WWW),
                typeof(Light),
                typeof(Mathf),
                typeof(System.Collections.Generic.List<int>),
                typeof(Action<string>),
                typeof(UnityEngine.Debug),
                typeof(UnityEngine.UI.Text)
            };
}