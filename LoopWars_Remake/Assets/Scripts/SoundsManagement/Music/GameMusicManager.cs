using Unity.Netcode;
using UnityEngine;

public class GameMusicManager : NetworkBehaviour
{
    private NetworkVariable<int> curMusicIndex = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        curMusicIndex.OnValueChanged += OnCurMusicIndexChanged;
        OnCurMusicIndexChanged(0, curMusicIndex.Value);
    }

    private void OnCurMusicIndexChanged(int oldMusicIndex, int curMusicIndex)
    {
        SoundScriptableObject curMusic = MusicMapsPairsListScriptableObject.Instance.musicMapsPairs[curMusicIndex].music;
        if (MusicManager.curMusic == curMusic) return;
        MusicManager.StartMusicWithoutLoop(curMusic);
    }

    private void OnMusicStarted(SoundScriptableObject music)
    {
        if(NetworkManager.Singleton.IsServer)
            curMusicIndex.Value = MusicMapsPairsListScriptableObject.Instance.GetIndex(music);
    }

    private void OnAllPlayersLoaded()
    {
        if (NetworkManager.Singleton.IsServer)
            MusicManager.StartMusicWithLoop();
    }

    private void OnEnable()
    {
        MusicManager.SetCurMusicList(MusicMapsPairsListScriptableObject.Instance);
        GameManager.onAllThePlayersLoaded += OnAllPlayersLoaded;
        MusicManager.onMusicStarted += OnMusicStarted;
    }

    private void OnDisable()
    {
        GameManager.onAllThePlayersLoaded -= OnAllPlayersLoaded;
        MusicManager.onMusicStarted -= OnMusicStarted;
    }
}
