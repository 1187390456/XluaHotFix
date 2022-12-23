using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.UI;

public class GameInit : MonoBehaviour
{
    public static GameInit Instance;

    private string downLoadPath;

    private void Awake()
    {
        Instance = this;
        downLoadPath = PathUtil.GetAssetBundleOutPath();

        // 检测资源进行比对更新
        StartCoroutine(DownLoadResTest());

        // 开始游戏主逻辑
        gameObject.AddComponent<AssetBundleManager>();
        gameObject.AddComponent<LuaManager>();

        // lua文件编码必须会UTF-8 没有Bom
        LuaManager.Instance.DoString("require 'LGameInit'");
        LuaManager.Instance.CallLuaFunction("LGameInit", "Init");
    }

    public void TestFunc()
    {
        var prefabs = AssetBundleManager.Instance.LoadAsset("main", "UI", "Update") as GameObject;
        var canvas = GameObject.Find("Canvas");

        var target = Instantiate(prefabs, canvas.transform);
        target.transform.Find("Content").GetComponent<Text>().text = "666111";
    }

    public void TestFunc1()
    {
        var prefabs = AssetBundleManager.Instance.LoadAsset("main", "UI", "Update") as GameObject;
        var canvas = GameObject.Find("Canvas");

        var target = Instantiate(prefabs, canvas.transform);
        target.transform.Find("Content").GetComponent<Text>().text = "更新完成";
    }

    /// <summary>
    /// 检测资源 测试用
    /// </summary>
    private IEnumerator DownLoadResTest()
    {
        string url = "E:/LuaServer";

        string fileUrl = url + "/files.txt";

        // 判断本地是否有这个文件 并拷贝

        if (!Directory.Exists(downLoadPath)) Directory.CreateDirectory(downLoadPath);

        // 读取文件内容

        string[] lines = File.ReadAllLines(fileUrl);

        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue; // 空行
            string[] kv = lines[i].Split('|'); // 分割
            string fileName = kv[0];
            string localFile = (downLoadPath + "/" + fileName).Trim();
            if (!File.Exists(localFile)) // 本地不存在这个文件 进行下载
            {
                string dir = Path.GetDirectoryName(localFile);
                Directory.CreateDirectory(dir);

                // 开始网络下载
                string tmpUrl = url + "/" + fileName;
                string tmpText = File.ReadAllText(tmpUrl);
                File.WriteAllText(localFile, tmpText);
            }
            else // 有文件 比对md5 效验是否有更新
            {
                string md5 = kv[1];
                string localMd5 = GetFileMd5(localFile);
                if (md5 == localMd5)
                {
                    // 无更新
                }
                else
                {
                    // 更新了 删除本地文件
                    File.Delete(localFile);

                    // 下载新文件
                    string tmpUrl = url + "/" + fileName;
                    string tmpText = File.ReadAllText(tmpUrl);
                    File.WriteAllText(localFile, tmpText);
                }
            }
        }
        yield return new WaitForEndOfFrame();

        Debug.Log("更新完成");
    }

    /// <summary>
    /// 检测资源
    /// </summary>
    private IEnumerator DownLoadRes()
    {
        string url = "http://127.0.0.1";

        string fileUrl = url + "files.txt";

        WWW www = new WWW(fileUrl);

        yield return www;

        // 网络检测
        if (www.error != null) Debug.LogError(www.error);

        // 判断本地是否有这个文件 并拷贝

        if (!Directory.Exists(downLoadPath)) Directory.CreateDirectory(downLoadPath);

        // 下载写入本地
        File.WriteAllBytes(downLoadPath + "/files.txt", www.bytes);

        // 读取文件内容

        string filesText = www.text;
        string[] lines = filesText.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue; // 空行
            string[] kv = lines[i].Split('|'); // 分割
            string fileName = kv[0];
            string localFile = (downLoadPath + "/" + fileName).Trim();
            if (!File.Exists(localFile)) // 本地不存在这个文件 进行下载
            {
                string dir = Path.GetDirectoryName(localFile);
                Directory.CreateDirectory(dir);

                // 开始网络下载
                string tmpUrl = url + fileName;

                www = new WWW(fileUrl);
                yield return www;
                if (www.error != null) Debug.LogError(www.error);

                File.WriteAllBytes(localFile, www.bytes);
            }
            else // 有文件 比对md5 效验是否有更新
            {
                string md5 = kv[1];
                string localMd5 = GetFileMd5(localFile);
                if (md5 == localMd5)
                {
                    // 无更新
                }
                else
                {
                    // 更新了 删除本地文件
                    File.Delete(localFile);

                    // 下载新文件
                    string dir = Path.GetDirectoryName(localFile);
                    Directory.CreateDirectory(dir);
                    string tmpUrl = url + fileName;
                    www = new WWW(fileUrl);
                    yield return www;
                    if (www.error != null) Debug.LogError(www.error);
                    File.WriteAllBytes(localFile, www.bytes);
                }
            }
        }
        yield return new WaitForEndOfFrame();

        Debug.Log("更新完成");
    }

    /// <summary>
    /// 获取文件md5
    /// </summary>
    private static string GetFileMd5(string filePath)
    {
        FileStream fs = new FileStream(filePath, FileMode.Open);
        MD5 md5 = new MD5CryptoServiceProvider();

        byte[] result = md5.ComputeHash(fs);
        fs.Close();

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            sb.Append(result[i].ToString("x2"));
        }
        return sb.ToString();
    }
}