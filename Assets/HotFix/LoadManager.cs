using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public GameObject prefabs;

    private Slider slider;
    private Text loadTips;
    private Text progress;

    private float toProgress;
    private float nowProgress;

    private void Awake()
    {
        slider = prefabs.transform.Find("Slider").GetComponent<Slider>();
        loadTips = slider.transform.Find("LoadText").GetComponent<Text>();
        progress = prefabs.transform.Find("Progress").GetComponent<Text>();
    }

    public void Load() => StartCoroutine(StarLoad());

    private IEnumerator StarLoad()
    {
        prefabs.SetActive(true);

        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if (operation.progress < 0.9f) toProgress = operation.progress;
            else toProgress = 100;
            if (nowProgress < toProgress) nowProgress++;

            slider.value = nowProgress / 100.0f;
            progress.text = nowProgress + "%";

            if (nowProgress == 100)
            {
                progress.text = "加载完成! 请按任意键继续!";
                if (Input.anyKeyDown) operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}