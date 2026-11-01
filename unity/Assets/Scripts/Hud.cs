using UnityEngine;
using UnityEngine.SceneManagement;

// 화면 글자: 모은 별 개수, 다 모으면 걸린 시간. R 키나 AGAIN 버튼으로 다시 시작.
public class Hud : MonoBehaviour
{
    public static Hud I;
    int total, got;
    float startTime, clearTime = -1f;
    GUIStyle big, small, btn;

    void Awake()
    {
        I = this;
        total = FindObjectsByType<Pickup>(FindObjectsSortMode.None).Length;
        startTime = Time.time;
    }

    public void Add()
    {
        got++;
        if (got >= total) clearTime = Time.time - startTime;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(0);
    }

    void Shadow(Rect r, string text, GUIStyle s)
    {
        Color c = s.normal.textColor;
        s.normal.textColor = new Color(0f, 0f, 0f, 0.55f);
        GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, s);
        s.normal.textColor = c;
        GUI.Label(r, text, s);
    }

    void OnGUI()
    {
        if (big == null)
        {
            big = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(22, Screen.height / 14), fontStyle = FontStyle.Bold };
            big.normal.textColor = Color.white;
            small = new GUIStyle(big) { fontSize = Mathf.Max(14, Screen.height / 28) };
            btn = new GUIStyle(GUI.skin.button) { fontSize = small.fontSize, fontStyle = FontStyle.Bold };
        }
        Shadow(new Rect(18, 10, Screen.width, 100), "STARS  " + got + " / " + total, big);
        if (clearTime < 0f) return;

        big.alignment = TextAnchor.MiddleCenter;
        Shadow(new Rect(0, Screen.height * 0.30f, Screen.width, 80), "CLEAR!  " + clearTime.ToString("0.0") + "s", big);
        if (GUI.Button(new Rect(Screen.width / 2f - 90, Screen.height * 0.48f, 180, 54), "AGAIN", btn)) SceneManager.LoadScene(0);
        big.alignment = TextAnchor.UpperLeft;
    }
}
