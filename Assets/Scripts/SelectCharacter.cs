using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SelectManager : MonoBehaviour
{
    public TMP_InputField nameInput;

    public void StartGame()
    {
        string playerName = nameInput.text;

        if (playerName.Trim() == "")
        {
            Debug.Log("Chưa nhập tên nhân vật");
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        SceneManager.LoadScene("LoadGame");
    }

    public void BackToStart()
    {
        SceneManager.LoadScene("StartGame");
    }
}