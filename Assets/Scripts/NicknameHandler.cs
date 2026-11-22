using UnityEngine;

public class NicknameHandler : MonoBehaviour
{
    public static NicknameHandler Instance;
    public string nickname = "mingebag"; // i miss old garry's mod
    private void Awake()
    {
        if(Instance == null) { Instance = this; DontDestroyOnLoad(this); } else { Destroy(this); }
        nickname = PlayerPrefs.GetString("Nickname", "mingebag");
        Debug.Log($"NicknameHandler: awake, name is {nickname}");
    }

    public void SetNick(string nick)
    {
        nickname = nick;
        PlayerPrefs.SetString("Nickname", nick);
    }
}
