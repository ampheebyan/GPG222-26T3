using UnityEngine;

public class NicknameManager : MonoBehaviour
{
    public static NicknameManager Instance;
    public string nickname = "mingebag"; // i miss old garry's mod
    public int maxChars = 16;
    private void Awake()
    {
        if(Instance == null) { Instance = this; DontDestroyOnLoad(this); } else { Destroy(this); }
        nickname = PlayerPrefs.GetString("Nickname", "mingebag");
        if(nickname.Length > maxChars) { nickname = nickname.Substring(0, maxChars); }
        Debug.Log($"NicknameManager: awake, name is {nickname}");
    }

    public void SetNick(string nick)
    {
        if(nick.Length > maxChars) { nick = nick.Substring(0, maxChars); }
        nickname = nick;
        PlayerPrefs.SetString("Nickname", nick);
    }
}
