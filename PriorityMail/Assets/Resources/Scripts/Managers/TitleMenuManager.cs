using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuManager : MonoBehaviour
{
    public static TitleMenuManager current;

    void Start()
    {
        current = this;

        GameObject.Find("TitleMenuCanvas/TitleMenuAnchor/PlayButton").GetComponent<Button>().onClick.AddListener(PlayGame);
        GameObject.Find("TitleMenuCanvas/TitleMenuAnchor/CreateButton").GetComponent<Button>().onClick.AddListener(EditGame);
        GameObject.Find("TitleMenuCanvas/TitleMenuAnchor/ExitButton").GetComponent<Button>().onClick.AddListener(ExitGame);
    }

    private void ExitGame ()
    {
        Application.Quit();
    }

    private void PlayGame ()
    {
        SceneManager.LoadScene("PlayingScene", LoadSceneMode.Single);
    }

    private void EditGame ()
    {
        SceneManager.LoadScene("EditingScene", LoadSceneMode.Single);
    }
}
