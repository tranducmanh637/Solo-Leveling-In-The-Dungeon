using UnityEngine;
using UnityEngine.SceneManagement;

public class BattlePortal : MonoBehaviour
{
    public string battleSceneName = "BattleMap";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(battleSceneName);
        }
    }
}