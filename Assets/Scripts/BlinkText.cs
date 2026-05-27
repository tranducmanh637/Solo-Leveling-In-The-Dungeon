using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    public float blinkSpeed = 1f;

    private TMP_Text textUI;

    void Start()
    {
        textUI = GetComponent<TMP_Text>();
    }

    void Update()
    {
        Color c = textUI.color;

        c.a = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        textUI.color = c;
    }
}