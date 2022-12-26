using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Networking;
using XLua;

[LuaCallCSharp]
public class GameInit : MonoBehaviour
{
    public static GameInit Instance;

    private string downLoadPath;

    private void Awake()
    {
        Instance = this;
        downLoadPath = PathUtil.GetAssetBundleOutPath();

        gameObject.AddComponent<AssetBundleManager>();
        gameObject.AddComponent<LuaManager>();

        StartCoroutine(InitGame());
    }

    //  游戏初始化
    private IEnumerator InitGame()
    {
        // 下载资源进行对比
        yield return StartCoroutine(DownLoadRes());

        yield return new WaitUntil(() => File.Exists(downLoadPath + "/Lua/LGameInit.Lua"));
        // 游戏开始逻辑
        LuaManager.Instance.DoString("require 'LGameInit'");
        LuaManager.Instance.CallLuaFunction("LGameInit", "Init");
    }

    public void TestFunc()
    {
        StartCoroutine(AssetBundleManager.Instance.StarLoadAsset("main", "UI", "Update", (value) =>
        {
            var prefabs = value as GameObject;
            var canvas = GameObject.Find("Canvas");
            var target = Instantiate(prefabs, canvas.transform);
            target.transform.Find("Content").GetComponent<Text>().text = "还没更新呢";
        }));
    }

    public void TestFunc1()
    {
        StartCoroutine(AssetBundleManager.Instance.StarLoadAsset("main", "UI", "Update", (value) =>
        {
            var prefabs = value as GameObject;
            var canvas = GameObject.Find("Canvas");
            var target = Instantiate(prefabs, canvas.transform);
            target.transform.Find("Content").GetComponent<Text>().text = "更新完成";
        }));
    }

    /// <summary>
    /// 检测资源 测试用
    /// </summary>
    private IEnumerator DownLoadResTest()
    {
        string url = "D:/LuaServer";

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

            Debug.Log(url + "/" + fileName);

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
        // 获取远程Md5文件
        string url = "https://xuchenming-0gg48xrbe11e5c2d-1309555563.tcloudbaseapp.com/Windows/";
        string fileUrl = url + "files.txt";
        UnityWebRequest www = UnityWebRequest.Get(fileUrl);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success) Debug.Log(www.error);
        // 判断本地是否有这个文件 并拷贝
        if (!Directory.Exists(downLoadPath)) Directory.CreateDirectory(downLoadPath);
        // 下载写入本地
        File.WriteAllBytes(downLoadPath + "/files.txt", www.downloadHandler.data);
        // 读取文件内容
        string filesText = www.downloadHandler.text;
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
                StartCoroutine(DownFileAndSave(url + fileName, localFile)); // 开始网络下载
            }
            else // 有文件 比对md5 效验是否有更新
            {
                string md5 = kv[1].Trim();
                string localMd5 = GetFileMd5(localFile).Trim();

                if (md5 != localMd5)   // 更新了 删除本地文件 下载新的
                {
                    File.Delete(localFile);
                    StartCoroutine(DownFileAndSave(url + fileName, localFile)); // 开始网络下载
                }
            }
        }
        yield return null;

        Debug.Log("更新完成");
    }

    /// <summary>
    /// 下载文件并保存在本地
    /// </summary>
    private IEnumerator DownFileAndSave(string url, string savePath)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError) Debug.Log(www.error);
        File.WriteAllBytes(savePath, www.downloadHandler.data);
        yield return new WaitUntil(() => File.Exists(savePath));
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