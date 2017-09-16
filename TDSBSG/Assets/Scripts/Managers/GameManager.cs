﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    Toolbox toolbox;
    EventManager em;
    bool isPaused = false;
    bool pausingAvailable = false;
    ERobotType robotTypeToSpawnPlayerAs = ERobotType.DEFAULT;
    int mainMenuIndex = -1;
    int firstLevelIndex = -1;
    int lastLevelIndex = -1;
    [SerializeField]
    bool loopLevels = false;
    bool startingLevel = false;
    float startingLevelPeriodTimer = 0f;
    float startingLevelPeriodDuration = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();

        Vector3 sceneIndices = em.BroadcastRequestSceneIndices();
        mainMenuIndex = (int)sceneIndices.x;
        firstLevelIndex = (int)sceneIndices.y;
        lastLevelIndex = (int)sceneIndices.z;

        em.OnInputEvent += OnInputEvent;
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        em.OnRequestPauseStateChange += OnRequestPauseStateChange;
        em.OnLevelCompleted += OnLevelCompleted;
        em.OnPlayerCatched += OnPlayerCatched;
        em.OnRequestSpawningRobotType += OnRequestSpawningRobotType;
        em.OnRequestLoadLevel += OnRequestLoadLevel;
    }

    private void OnDisable()
    {
        em.OnInputEvent -= OnInputEvent;
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        em.OnRequestPauseStateChange -= OnRequestPauseStateChange;
        em.OnLevelCompleted -= OnLevelCompleted;
        em.OnPlayerCatched -= OnPlayerCatched;
        em.OnRequestSpawningRobotType -= OnRequestSpawningRobotType;
        em.OnRequestLoadLevel += OnRequestLoadLevel;
    }

    private ERobotType OnRequestSpawningRobotType()
    {
        return robotTypeToSpawnPlayerAs;
    }

    private void PauseGame()
    {
        Time.timeScale = 0.0f;
        isPaused = true;
        em.BroadcastPauseStateChange(true);
    }

    private void ResumeGame()
    {
        Time.timeScale = 1.0f;
        isPaused = false;
        em.BroadcastPauseStateChange(false);
    }

    private void TogglePauseGame()
    {
        if (pausingAvailable)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void OnInputEvent(EInputType newInput)
    {
        if (newInput == EInputType.PAUSE_KEYDOWN)
        {
            TogglePauseGame();
        }
    }

    void OnRequestLoadLevel(int sceneBuildIndex)
    {
        pausingAvailable = false;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (mainMenuIndex == -1 || firstLevelIndex == -1 || lastLevelIndex == -1)
        {
            Vector3 sceneIndices = em.BroadcastRequestSceneIndices();
            mainMenuIndex = (int)sceneIndices.x;
            firstLevelIndex = (int)sceneIndices.y;
            lastLevelIndex = (int)sceneIndices.z;
        }

        if (scene.buildIndex == mainMenuIndex)
        {
            ResumeGame();
        }
        else if (scene.buildIndex == firstLevelIndex)
        {
            startingLevelPeriodTimer = startingLevelPeriodDuration;
            startingLevel = true;
            robotTypeToSpawnPlayerAs = ERobotType.DEFAULT;
            em.BroadcastSpawnPlayer(robotTypeToSpawnPlayerAs);
            em.BroadcastInitializeGame();
            em.BroadcastStartGame();
            ResumeGame();
        }
        else if (scene.buildIndex > firstLevelIndex/* && scene.buildIndex <= lastLevelIndex*/)
        {
            startingLevelPeriodTimer = startingLevelPeriodDuration;
            startingLevel = true;
            em.BroadcastSpawnPlayer(robotTypeToSpawnPlayerAs);
            em.BroadcastInitializeGame();
            em.BroadcastStartGame();
            ResumeGame();
        }
    }

    void OnRequestPauseStateChange(bool newState)
    {
        if (newState != isPaused)
        {
            if (newState)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    void OnLevelCompleted(int sceneIndex, ERobotType lastPossessedRobotType)
    {
        robotTypeToSpawnPlayerAs = lastPossessedRobotType;
        LoadNextLevel();
    }

    void LoadNextLevel()
    {
        int currentSceneIndex = em.BroadcastRequestCurrentSceneIndex();
        if (currentSceneIndex != -1)
        {
            if (currentSceneIndex < lastLevelIndex)
            {
                currentSceneIndex++;
                em.BroadcastRequestLoadLevel(currentSceneIndex);
            }
            else if (loopLevels)
            {
                currentSceneIndex = firstLevelIndex;
                em.BroadcastRequestLoadLevel(currentSceneIndex);
            }
            else
            {
                //TODO: Implement proper game completed screen
                Debug.Log("Game completed! Loading main menu");
                em.BroadcastRequestLoadLevel(mainMenuIndex);
            }
        }
        else
        {
            Debug.Log("Scene index not found!");
        }

    }

    private void OnPlayerCatched()
    {
        int currentSceneIndex = em.BroadcastRequestCurrentSceneIndex();
        em.BroadcastRequestLoadLevel(currentSceneIndex);
    }

    private void FixedUpdate()
    {
        if (startingLevel)
        {
            startingLevelPeriodTimer -= Time.fixedDeltaTime;

            if(startingLevelPeriodTimer <= 0)
            {
                startingLevel = false;
                pausingAvailable = true;
            }
        }
    }
}
