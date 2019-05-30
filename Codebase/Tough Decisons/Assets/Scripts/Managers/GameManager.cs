using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    
    public List<string> pauseDisabledScenes;
    
    private bool gamePaused;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        else 
            Destroy(this);

        gamePaused = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            SetGamePaused(!gamePaused);
    }

    // Is the game currently paused
    public bool IsGamePaused()
    {
        return gamePaused;
    }

    // Handles all External and Internal vars to pause game
    // Also prevents and scene in public list from being able to pause
    public void SetGamePaused(bool paused)
    {
        if (pauseDisabledScenes.Contains(SceneManager.GetActiveScene().name))
            return;
        
        if (!gamePaused && paused)
        {
            gamePaused = true;

            Time.timeScale = 0;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            UIManager.instance.EnableUIScreen(UIManager.UIScreen.PAUSE);
        }
        
        else if (gamePaused && !paused)
        {
            gamePaused = false;

            Time.timeScale = 1;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            UIManager.instance.DisableUIScreen(UIManager.UIScreen.PAUSE);
        }
    }
}
