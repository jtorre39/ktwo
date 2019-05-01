﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// Is commanded by the WaveManager
// to display specific UI related things to
// to the player.

// In order to get it synced across the client
// and the server, we need to network spawn a prefab
// of this script onto the client. This is currently
// done by the GameManager.

// Will exist on both the client and the server.
// Client should not run any methods besides
// ones run by the RpcClient tagged methods.
public class WaveUIManager : NetworkBehaviour
{
    public static WaveUIManager instance;

    public const float SERVER_CLIENT_ERROR_ALLOWANCE = 0.1f;
    [Tooltip("In seconds")]
    public const float NETWORK_TIME_SYNC_RATE = 3;

    public const string WAVE_BEGUN_MESSAGE = "K'tah has Awoken";
    public const string WAVE_ENDED_MESSAGE = "K'tah Slumbers for Now...";
    public const string GAME_OVER_MESSAGE = "K'tah has claimed all victims. Score: {0}";

    public const string WAVE_FORMAT_STRING = "Wave {0} | {1}:{2}";
    public const string RESPITE_FORMAT_STRING = "Respite | {0}:{1}";
    public const string SCORE_FORMAT_STRING = "Points | {0}";

    [Tooltip("Should reference WaveAnnouncement in the scene")]
    Text waveAnnouncement;

    [Tooltip("Should reference WaveTimerUI in the scene")]
    Text timerUI;

    [Tooltip("Should reference WaveScore in the scene")]
    Text scoreUI;

    public float fadeInDuration = 2;
    public float fadeOutDuration = 2;

    // Clients do not have a WaveManager, thus have a local timer that
    // the server-side UIManager will periodically make sure is correctly
    // synced up to the timer in the WaveManager.
    float clientSideTimer = 0;
    float timeSinceLastSync = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (isServer)
        {
            RpcInitialize();
        }
    }

    void Initialize()
    {
        var timerGameObject = GameObject.Find("WaveTimerUI");
        timerGameObject.transform.localScale = Vector3.one;
        timerUI = timerGameObject.GetComponent<Text>();

        var scoreGameObject = GameObject.Find("WaveScoreUI");
        scoreGameObject.transform.localScale = Vector3.one;
        scoreUI = scoreGameObject.GetComponent<Text>();

        var announcementObject = GameObject.Find("WaveAnnouncementUI");
        announcementObject.transform.localScale = Vector3.one;
        waveAnnouncement = scoreGameObject.GetComponent<Text>();
    }

    IEnumerator FadeTextInAndOut(string text)
    {
        waveAnnouncement.gameObject.SetActive(true);

        float currentTime = 0;
        waveAnnouncement.text = text;
        while (currentTime < fadeInDuration)
        {
            currentTime += Time.deltaTime;
            waveAnnouncement.color = new Color(
                waveAnnouncement.color.r,
                waveAnnouncement.color.g,
                waveAnnouncement.color.b,
                Mathf.Lerp(0, 1, currentTime / fadeInDuration)
            );
            yield return null;
        }

        currentTime = 0;

        while (currentTime < fadeOutDuration)
        {
            currentTime += Time.deltaTime;
            waveAnnouncement.color = new Color(
                waveAnnouncement.color.r,
                waveAnnouncement.color.g,
                waveAnnouncement.color.b,
                Mathf.Lerp(1, 0, currentTime / fadeInDuration)
            );
            yield return null;
        }

        waveAnnouncement.gameObject.SetActive(false);
    }

    public void OnAllPlayersDead()
    {
        RpcShowGameOverText(WaveManager.instance.currentPoints);
    }

    public void OnWaveEnd()
    {
        StopAllCoroutines();
        RpcWaveEnd();
    }

    public void OnWaveBegin()
    {
        StopAllCoroutines();
        RpcWaveBegin();
    }

    public void OnRespiteBegin()
    {
        RpcRespiteTimerBegin();
    }

    IEnumerator UpdateWaveTimer()
    {
        clientSideTimer = 0;
        while (clientSideTimer < WaveManager.SECONDS_IN_A_WAVE)
        {
            if (timeSinceLastSync > NETWORK_TIME_SYNC_RATE)
            {
                RpcSyncTime(WaveManager.instance.currentWaveTime);   
            }
            timeSinceLastSync += Time.deltaTime;
            clientSideTimer += Time.deltaTime;
            FormatWaveTimerUI(WAVE_FORMAT_STRING, isWave: true);
            yield return null;
        }
    }

    IEnumerator UpdateRespiteTimer()
    {
        clientSideTimer = WaveManager.SECONDS_IN_A_RESPITE;
        while (clientSideTimer > 0)
        {
            if (timeSinceLastSync > NETWORK_TIME_SYNC_RATE)
            {
                RpcSyncTime(WaveManager.instance.currentWaveTime);   
            }
            timeSinceLastSync += Time.deltaTime;
            clientSideTimer -= Time.deltaTime;
            FormatWaveTimerUI(RESPITE_FORMAT_STRING, isWave: false);
            yield return null;
        }
    }

    // Is wave is false for respite, true otherwise
    void FormatWaveTimerUI(string format, bool isWave)
    {
        string minutes = ((int)(clientSideTimer / 60)).ToString("00");
        string seconds = ((int)(clientSideTimer % 60)).ToString("00");

        timerUI.text = isWave ? string.Format(format, minutes, seconds) :
            string.Format(format, WaveManager.instance.currentWave, minutes, seconds);
    }

    void UpdatescoreUI(int score)
    {
        scoreUI.text = string.Format(SCORE_FORMAT_STRING, score);
    }

    [ClientRpc]
    void RpcWaveBegin()
    {
        StartCoroutine(FadeTextInAndOut(WAVE_BEGUN_MESSAGE));
    }

    [ClientRpc]
    void RpcWaveEnd()
    {
        StartCoroutine(FadeTextInAndOut(WAVE_ENDED_MESSAGE));
    }

    [ClientRpc]
    void RpcRespiteTimerBegin()
    {
        StartCoroutine(UpdateRespiteTimer());
    }

    [ClientRpc]
    void RpcWaveTimerBegin()
    {
        StartCoroutine(UpdateWaveTimer());
    }

    [ClientRpc]
    void RpcSyncTime(float serverTime)
    {
        if (Mathf.Abs(serverTime - clientSideTimer) > SERVER_CLIENT_ERROR_ALLOWANCE)
        {
            clientSideTimer = serverTime;
        }
    }

    [ClientRpc]
    void RpcShowGameOverText(int score)
    {
        waveAnnouncement.text = string.Format(GAME_OVER_MESSAGE, score);
        waveAnnouncement.gameObject.SetActive(true);
    }

    [ClientRpc]
    void RpcUpdateScoreText(int score)
    {
        UpdatescoreUI(score);
    }

    [ClientRpc]
    public void RpcInitialize()
    {
        Initialize();
    }
}
