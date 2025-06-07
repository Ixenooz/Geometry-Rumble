using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    public static GameManager Instance { get; private set; } // Singleton instance

    public event EventHandler OnStateChanged; // Event to notify state changes
    public event EventHandler OnLocalPlayerReadyChanged; // Event to notify when a local player is ready

    public enum GameState
    {
        WaitingToStart,
        CountownToStart,
        GamePlaying,
        GameOver,
    }

    private GameState state;
    private bool isLocalPlayerReady;
    private float countdownToStartTimer = 3f; // Countdown duration in seconds

    private float gamePlayingTimer;
    private float gamePlayingTimerMax = 120f; // Example: 2 minutes for the game (will be 1200)
    private int playersLeft; // Number of players left in the game
    private int playersMax; // Maximum number of players in the game

    private void Awake()
    {
        Instance = this; // Set the singleton instance

        state = GameState.WaitingToStart; // Initialize the game state
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.CountownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer <= 0f)
                {
                    state = GameState.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax; // Reset the game timer
                    OnStateChanged?.Invoke(this, EventArgs.Empty); // Notify state change
                }
                break;
            case GameState.GamePlaying:
                gamePlayingTimer -= Time.deltaTime; // Decrease the game timer
                if (playersLeft == 1)
                {
                    state = GameState.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty); // Notify state change
                }

                // Logic to handle game playing state (zone shrinking, events...)

                break;
            case GameState.GameOver:
                // Logic to handle game over state (show results, reset game...)
                break;

        }
    }

    public bool isWaitingToStart()
    {
        return state == GameState.WaitingToStart;
    }

    public bool isCountdownToStartActive()
    {
        return state == GameState.CountownToStart;
    }

    public bool isGamePlaying()
    {
        return state == GameState.GamePlaying;
    }

    public bool isGameOver()
    {
        return state == GameState.GameOver;
    }
    
    public float GetCountdownToStart()
    {
        return countdownToStartTimer;
    }

    public float GetGamePlayingTimer()
    {
        return gamePlayingTimer;
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }
}
