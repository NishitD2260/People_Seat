using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private int sceneToLoad = 1;
    [SerializeField] private Image loadingImage;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(.6f);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);

        if (loadingImage != null)
        {
            while (!asyncOperation.isDone)
            {
                loadingImage.fillAmount = asyncOperation.progress;
                yield return null;
            }
        }
    }
}
