using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;
    
    public enum UIScreen { GAMEPLAY, PAUSE, RESTART, LOADING }

    [Header("In-Game Menus")] 
    public GameObject gameplayScreenPrefab;
    public GameObject pauseScreenPrefab;
    public GameObject restartScreenPrefab;

    [Header("External Menus")] 
    public GameObject loadingScreenPrefab;

    // Instance of each menu made from their related prefab
    private Dictionary<UIScreen, GameObject> uiScreens;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        else 
            Destroy(this);

        
        uiScreens = new Dictionary<UIScreen, GameObject>();
    }

    public void EnableUIScreen(UIScreen screen)
    {
        if (!uiScreens.ContainsKey(screen) || uiScreens[screen] == null)
            uiScreens.Add(screen, Instantiate(GetScreenFromType(screen)));
        
        uiScreens[screen].SetActive(true);
    }

    public void DisableUIScreen(UIScreen screen)
    {
        if (!uiScreens.ContainsKey(screen) || uiScreens[screen] == null)
            uiScreens.Add(screen, Instantiate(GetScreenFromType(screen)));
        
        uiScreens[screen].SetActive(false);
    }

    private GameObject GetScreenFromType(UIScreen screen)
    {
        switch (screen)
        {
            case UIScreen.GAMEPLAY:
                return gameplayScreenPrefab;
            
            case UIScreen.PAUSE:
                return pauseScreenPrefab;
            
            case UIScreen.RESTART:
                return restartScreenPrefab;
            
            case UIScreen.LOADING:
                return loadingScreenPrefab;
        }

        return null;
    }
}
