using System;
using System.Collections;
using MADP.Managers;
using MADP.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MADP.Controllers
{
    public class SceneController : PersistentSingleton<SceneController>
    {
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private Image _loadingProgress;

        public Action OnLoadingFinished;
        
        private void Start()
        {
            StartCoroutine(LoadSceneAsync("Menu"));
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            loadingCanvas.gameObject.SetActive(true);
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            float displayProgress = 0f;
            while (!operation.isDone)
            {
                float sceneProgress = Mathf.Clamp01(operation.progress / 0.9f);
                displayProgress = Mathf.MoveTowards(displayProgress, sceneProgress, 3f * Time.deltaTime);
                UpdateLoadingProgress(displayProgress);
                yield return null;
            }
            
            UpdateLoadingProgress(1);
            loadingCanvas.gameObject.SetActive(false);
            yield return null;
            OnLoadingFinished?.Invoke();
        }

        private void UpdateLoadingProgress(float progress)
        {
            if(_loadingProgress != null)
                _loadingProgress.fillAmount = progress;
            
            
            
        }
    }
}