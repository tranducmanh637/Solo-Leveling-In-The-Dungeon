using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingBar : MonoBehaviour
{
    public Image fill;
    public float loadingSpeed = 0.3f;

    private float progress = 0;

    void Update()
    {
        progress += Time.deltaTime * loadingSpeed;

        fill.fillAmount = progress;

        if (progress >= 1f)
        {
            SceneManager.LoadScene("HomeGame");
        }
    }
}