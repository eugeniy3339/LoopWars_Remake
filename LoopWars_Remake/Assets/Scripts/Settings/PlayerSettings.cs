using Unity.Services.Authentication;
using UnityEngine;

public static class PlayerSettings
{
    public static string name
    {
        get
        {
            string name;
            if (PlayerPrefs.HasKey("PlayerName"))
                name = PlayerPrefs.GetString("PlayerName");
            else
                name = "Player";

            return name;
        }
        set
        {
            PlayerPrefs.SetString("PlayerName", value);
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsAuthorized)
                AuthenticationService.Instance.UpdatePlayerNameAsync(value);
            PlayerPrefs.Save();
        }
    }
}
