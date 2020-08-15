using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMenuManager : MonoBehaviour
{
    public static PlayerMenuManager current;

    private WorldData worldData;
    
    private GameObject levelMenuCanvas;
    private GameObject worldSelectorMenu;
    private GameObject levelSelectorMenu;

    void Start()
    {
        current = this;

        levelMenuCanvas = GameObject.Find("LevelMenuCanvas");
        worldSelectorMenu = GameObject.Find("LevelMenuCanvas/WorldSelectorMenu");
        levelSelectorMenu = GameObject.Find("LevelMenuCanvas/LevelSelectorMenu");

        levelSelectorMenu.SetActive(false);

        GenerateWorldListUI();
    }

    private void GenerateWorldListUI()
    {
        foreach (Transform t in GameObject.Find("LevelMenuCanvas/WorldSelectorMenu/WorldsPanel").transform)
        {
            Destroy(t.gameObject);
        }

        string[] worldNames = Directory.GetDirectories(Application.persistentDataPath + "/worlds");

        GameObject worldAssetModel = Resources.Load<GameObject>("Prefabs/LevelPlayerUIElement");
        for (int i = 0; i < worldNames.Length; i++)
        {
            GameObject worldAsset = Instantiate(worldAssetModel, GameObject.Find("LevelMenuCanvas/WorldSelectorMenu/WorldsPanel").transform);
            worldAsset.transform.localPosition = new Vector3(0, 135 - 35 * i, 0);
            worldAsset.transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = worldNames[i].Substring(Application.persistentDataPath.Length + 8);

            int _i = i;
            worldAsset.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { ChooseWorldForEditing(worldNames[_i].Substring(Application.persistentDataPath.Length + 8)); });
        }
    }

    public void ChooseWorldForEditing(string _worldName)
    {
        Debug.Log("here uwu");
        worldData = (WorldData)SerializationManager.LoadData(Application.persistentDataPath + "/worlds/" + _worldName + "/worldData.wld");
        levelSelectorMenu.SetActive(true);
        worldSelectorMenu.SetActive(false);

        LoadWorldForEditing();
    }

    private void LoadWorldForEditing()
    {
        foreach (Transform t in GameObject.Find("LevelMenuCanvas/LevelSelectorMenu/LevelsPanel").transform)
        {
            Destroy(t.gameObject);
        }
        
        string[] levelNames = Directory.GetFiles(Application.persistentDataPath + "/worlds\\" + worldData.worldName);

        GameObject levelAssetModel = Resources.Load<GameObject>("Prefabs/LevelPlayerUIElement");
        int levelNameOffset = -1;
        for (int i = 0; i < levelNames.Length; i++)
        {
            string levelName = levelNames[i].Substring(Application.persistentDataPath.Length + 9 + worldData.worldName.Length, levelNames[i].Length - 4 - Application.persistentDataPath.Length - 9 - worldData.worldName.Length);
            if (levelName.Equals("worldData"))
            {
                continue;
            }
            else
            {
                levelNameOffset++;
            }

            GameObject worldAsset = Instantiate(levelAssetModel, GameObject.Find("LevelMenuCanvas/LevelSelectorMenu/LevelsPanel").transform);
            worldAsset.transform.localPosition = new Vector3(0, 135 - 35 * levelNameOffset, 0);
            worldAsset.transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = levelName;

            int _i = i;
            worldAsset.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { LoadLevel(levelNames[_i]); });
        }
    }

    private void LoadLevel (string levelPath)
    {
        levelMenuCanvas.SetActive(false);

        LevelManager.current.OpenLevel(worldData, levelPath);
    }
    
    public void ReturnToLevelSelector()
    {
        levelMenuCanvas.SetActive(true);
    }

    public void ReturnToTitleScene()
    {
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
    }
}