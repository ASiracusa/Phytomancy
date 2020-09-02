using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditorMenuManager : MonoBehaviour
{
    public static EditorMenuManager current;
    private string worldName;

    private GameObject editorCanvas;
    private GameObject editorMenuCanvas;
    private GameObject worldSelectorMenu;
    private GameObject worldEditorMenu;

    void Start()
    {
        current = this;

        editorCanvas = GameObject.Find("EditorCanvas");
        editorMenuCanvas = GameObject.Find("EditorMenuCanvas");
        worldSelectorMenu = GameObject.Find("EditorMenuCanvas/WorldSelectorMenu");
        worldEditorMenu = GameObject.Find("EditorMenuCanvas/WorldEditorMenu");

        worldEditorMenu.SetActive(false);

        GenerateWorldListUI();
    }

    private void GenerateWorldListUI ()
    {
        foreach (Transform t in GameObject.Find("EditorMenuCanvas/WorldSelectorMenu/WorldsPanel").transform)
        {
            Destroy(t.gameObject);
        }

        string[] worldNames = Directory.GetDirectories(Application.persistentDataPath + "/worlds");

        GameObject worldAssetModel = Resources.Load<GameObject>("Prefabs/WorldEditorUIElement");
        for (int i = 0; i < worldNames.Length; i++)
        {
            print(worldNames[i]);
            GameObject worldAsset = Instantiate(worldAssetModel, GameObject.Find("EditorMenuCanvas/WorldSelectorMenu/WorldsPanel").transform);
            worldAsset.transform.localPosition = new Vector3(0, 135 - 35 * i, 0);
            worldAsset.transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = worldNames[i].Substring(Application.persistentDataPath.Length + 8);

            int _i = i;
            worldAsset.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { ChooseWorldForEditing(worldNames[_i].Substring(Application.persistentDataPath.Length + 8)); });
        }
    }

    public void ChooseWorldForEditing (string worldName)
    {
        WorldManager.current.worldData = (WorldData)SerializationManager.LoadData(Application.persistentDataPath + "/worlds/" + worldName + "/worldData.wld");
        worldName = WorldManager.current.worldData.worldName;
        worldEditorMenu.SetActive(true);
        worldSelectorMenu.SetActive(false);

        LoadWorldForEditing();
    }

    public void CreateNewWorld()
    {
        int worldCount = Directory.GetDirectories(Application.persistentDataPath + "/worlds").Length;
        WorldManager.current.worldData = new WorldData(
            "_world" + worldCount,
            new float[]
            {
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f
            },
            new float[]
            {
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f
            },
            new float[]
            {
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f
            },
            new string[] { }
        );
        worldName = WorldManager.current.worldData.worldName;

        Directory.CreateDirectory(Application.persistentDataPath + "/worlds/" + WorldManager.current.worldData.worldName);
        SerializationManager.SaveWorld(WorldManager.current.worldData.worldName, WorldManager.current.worldData);

        LoadWorldForEditing();
    }

    private void LoadWorldForEditing ()
    {
        WorldData worldData = WorldManager.current.worldData;

        foreach (Transform t in GameObject.Find("EditorMenuCanvas/WorldEditorMenu/Levels/LevelsPanel").transform)
        {
            Destroy(t.gameObject);
        }

        Transform palettes = GameObject.Find("EditorMenuCanvas/WorldEditorMenu/Palettes").transform;
        for (int i = 1; i < palettes.childCount; i++)
        {
            int _i = i;
            palettes.GetChild(i).gameObject.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate { ChangePaletteColor(palettes.GetChild(_i).gameObject, _i - 1); });
            palettes.GetChild(i).gameObject.GetComponent<TMP_InputField>().text = "#" + ColorUtility.ToHtmlStringRGB(new Color(worldData.reds[i - 1], worldData.greens[i - 1], worldData.blues[i - 1]));
        }

        GameObject.Find("EditorMenuCanvas/WorldEditorMenu/Levels/WorldName").GetComponent<TMP_InputField>().text = worldData.worldName;
        worldName = worldData.worldName;
        GameObject.Find("EditorMenuCanvas/WorldEditorMenu/Levels/WorldName").GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate { ChangeWorldName(GameObject.Find("EditorMenuCanvas/WorldEditorMenu/Levels/WorldName").GetComponent<TMP_InputField>()); });
        
        string[] levelNames = Directory.GetFiles(Application.persistentDataPath + "/worlds\\" + worldData.worldName);

        GameObject levelAssetModel = Resources.Load<GameObject>("Prefabs/WorldEditorUIElement");
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

            GameObject worldAsset = Instantiate(levelAssetModel, GameObject.Find("EditorMenuCanvas/WorldEditorMenu/Levels/LevelsPanel").transform);
            worldAsset.transform.localPosition = new Vector3(0, 135 - 35 * levelNameOffset, 0);
            worldAsset.transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = levelName;

            int _i = i;
            worldAsset.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { LoadLevel(levelNames[_i]); });
        }
    }
    
    public void ReturnToWorldSelectorMenu ()
    {
        WorldManager.current.worldData = null;
        GenerateWorldListUI();
    }

    public void ReturnToWorldEditorMenu ()
    {
        CreatorManager.current.LeaveLevel();
        editorMenuCanvas.SetActive(true);
        worldEditorMenu.SetActive(true);
        editorCanvas.SetActive(false);
    }

    public void MakeNewLevel ()
    {
        WorldData worldData = WorldManager.current.worldData;

        editorMenuCanvas.SetActive(false);
        editorCanvas.SetActive(true);

        WorldManager.current.GenerateMaterials();

        CreatorManager.current.GenerateNewLevel();
        CreatorManager.current.OpenLevel();
    }

    public void ChangePaletteColor (GameObject paletteBox, int index)
    {
        Debug.Log("we be changin boys");
        Color color;
        ColorUtility.TryParseHtmlString(paletteBox.GetComponent<TMP_InputField>().text, out color);

        if (color == null)
        {
            color = Color.black;
        }
        paletteBox.GetComponent<Image>().color = color;
        
        WorldManager.current.worldData.reds[index] = color.r;
        WorldManager.current.worldData.greens[index] = color.g;
        WorldManager.current.worldData.blues[index] = color.b;
    }

    public void ChangeWorldName(TMP_InputField inputField)
    {
        worldName = inputField.text;
    }

    public void SaveWorld()
    {
        WorldData worldData = WorldManager.current.worldData;

        if (!worldName.Equals(worldData.worldName))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/worlds/" + worldName);
            foreach (string filePath in Directory.GetFiles(Application.persistentDataPath + "/worlds/" + worldData.worldName))
            {
                string[] backslashed = filePath.Split('\\');
                string ending = backslashed[backslashed.Length - 1];
                File.Move(filePath, Application.persistentDataPath + "/worlds/" + worldName + "/" + ending);
            }
            Directory.Delete(Application.persistentDataPath + "/worlds/" + worldData.worldName);
            worldData.worldName = worldName;
        }

        Directory.CreateDirectory(Application.persistentDataPath + "/worlds/" + worldData.worldName);
        SerializationManager.SaveWorld(worldData.worldName, worldData);
    }

    public string GetWorldName()
    {
        return WorldManager.current.worldData.worldName;
    }

    public void LoadLevel (string levelPath)
    {
        WorldData worldData = WorldManager.current.worldData;

        editorMenuCanvas.SetActive(false);
        editorCanvas.SetActive(true);

        WorldManager.current.GenerateMaterials();
        
        CreatorManager.current.LoadLevel(levelPath);
        CreatorManager.current.OpenLevel();
    }

    public void ReturnToTitleScene()
    {
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
    }
}