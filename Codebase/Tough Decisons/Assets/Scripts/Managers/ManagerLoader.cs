using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerLoader : MonoBehaviour
{

    public GameManager gameManager;
    public UIManager uiManager;

    void Awake()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager);

        if (UIManager.instance == null)
            Instantiate(uiManager);
    }
}
