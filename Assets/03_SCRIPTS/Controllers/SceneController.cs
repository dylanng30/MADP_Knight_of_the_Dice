using System.Collections;
using MADP.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MADP.Controllers
{
    public class SceneController : PersistentSingleton<SceneController>
    {
        [SerializeField] private Image _loadingProgress;
        
        private string sceneToLoad;

        public void LoadLevel(string sceneName)
        {
            sceneToLoad = sceneName;
            SceneManager.LoadScene("LoadingScene");
        }

        public IEnumerator LoadSceneAsync(string sceneName)
        {
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
        }

        private void UpdateLoadingProgress(float progress)
        {
            if(_loadingProgress != null)
                _loadingProgress.fillAmount = progress;
        }
    }
}