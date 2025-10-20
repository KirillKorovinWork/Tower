using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button firstScene;
    public Button secondScene;
    void Start()
    {
        firstScene.onClick.AddListener(FirstScene);
        secondScene.onClick.AddListener(SecondScene);
    }
    void FirstScene()
    {
        SceneManager.LoadScene(1);
    }
    void SecondScene()
    {
        SceneManager.LoadScene(2);
    }
}
