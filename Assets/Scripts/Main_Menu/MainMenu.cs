using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadGame()
    {
        // Load the game scene
        // SceneManager.LoadScene("Game");
        SceneManager.LoadScene(1);
    }
}
