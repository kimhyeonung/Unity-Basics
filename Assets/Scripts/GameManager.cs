using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerController player;
    public EnemyManager enemyManager;
    public MissileManager missileManager;
    public Text scoreText;
    public Text waveText;
    public Text healthText;
    public Text noticeText;
    public Text resultText;
    public GameObject menu;
    public bool IsPlaying { get; private set; }
    public int Score { get; private set; }
    public int Wave { get; private set; }
    public bool Finished { get; private set; }
    float nextWave;
    bool waiting;

    void Start()
    {
        Time.timeScale = 1f;
        menu.SetActive(true);
        resultText.text = "FOREST WARDEN\n\nProtect the clearing";
        player.ResetPlayer();
        Refresh();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        Finished = false;
        IsPlaying = true;
        Score = 0;
        Wave = 0;
        player.ResetPlayer();
        enemyManager.ResetPool();
        missileManager.ResetPool();
        menu.SetActive(false);
        nextWave = Time.time + 1f;
        waiting = true;
        Refresh();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.enterKey.wasPressedThisFrame && !IsPlaying) StartGame();
            if (keyboard.rKey.wasPressedThisFrame) StartGame();
        }
        if (!IsPlaying) return;
        if (waiting)
        {
            noticeText.text = "WAVE " + (Wave + 1);
            if (Time.time >= nextWave)
            {
                Wave++;
                enemyManager.SpawnWave(Wave);
                waiting = false;
                Refresh();
            }
        }
        else
        {
            noticeText.text = "";
            if (enemyManager.AliveCount == 0)
            {
                if (Wave >= 3) Finish(true);
                else
                {
                    waiting = true;
                    nextWave = Time.time + 2.5f;
                }
            }
        }
        healthText.text = "HP " + Mathf.CeilToInt(player.health.HP) + " / 100";
    }

    public void AddScore(int amount)
    {
        Score += amount;
        Refresh();
    }

    void Refresh()
    {
        scoreText.text = "SCORE  " + Score;
        waveText.text = "WAVE  " + Wave + " / 3";
        healthText.text = "HP " + Mathf.CeilToInt(player.health.HP) + " / 100";
    }

    public void Finish(bool won)
    {
        IsPlaying = false;
        Finished = true;
        Time.timeScale = 0f;
        noticeText.text = "";
        menu.SetActive(true);
        resultText.text = (won ? "CLEARING SAVED" : "GAME OVER") + "\n\nScore  " + Score + "   |   Wave  " + Wave;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
