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
    private WorldData worldData;
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
        worldData = (WorldData)SerializationManager.LoadData(Application.persistentDataPath + "/worlds/" + worldName + "/worldData.wld");
        worldName = worldData.worldName;
        worldEditorMenu.SetActive(true);
        worldSelectorMenu.SetActive(false);

        LoadWorldForEditing();
    }

    public void CreateNewWorld()
    {
        int worldCount = Directory.GetDirectories(Application.persistentDataPath + "/worlds").Length;
        worldData = new WorldData(
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
        worldName = worldData.worldName;

        Directory.CreateDirectory(Application.persistentDataPath + "/worlds/" + worldData.worldName);
        SerializationManager.SaveWorld(worldData.worldName, worldData);

        LoadWorldForEditing();
    }

    private void LoadWorldForEditing ()
    {
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
        worldData = null;
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
        editorMenuCanvas.SetActive(false);
        editorCanvas.SetActive(true);
        CreatorManager.current.palette = new Color[]
        {
            new Color(worldData.reds[0], worldData.greens[0], worldData.blues[0]),
            new Color(worldData.reds[1], worldData.greens[1], worldData.blues[1]),
            new Color(worldData.reds[2], worldData.greens[2], worldData.blues[2]),
            new Color(worldData.reds[3], worldData.greens[3], worldData.blues[3]),
            new Color(worldData.reds[4], worldData.greens[4], worldData.blues[4]),
            new Color(worldData.reds[5], worldData.greens[5], worldData.blues[5]),
            new Color(worldData.reds[6], worldData.greens[6], worldData.blues[6]),
            new Color(worldData.reds[7], worldData.greens[7], worldData.blues[7]),
            new Color(worldData.reds[8], worldData.greens[8], worldData.blues[8]),
            new Color(worldData.reds[9], worldData.greens[9], worldData.blues[9]),
            new Color(worldData.reds[10], worldData.greens[10], worldData.blues[10])
        };
        CreatorManager.current.materials = new Material[11];
        for (int i = 0; i < 11; i++)
        {
            CreatorManager.current.materials[i] = new Material(Resources.Load<Material>("Materials/TwotoneMat"));
            CreatorManager.current.materials[i].SetColor("_TopColor", CreatorManager.current.palette[i]);
            CreatorManager.current.materials[i].SetColor("_FrontColor", Color.Lerp(CreatorManager.current.palette[i], CreatorManager.current.palette[0], 0.45f));
            CreatorManager.current.materials[i].SetColor("_SideColor", Color.Lerp(CreatorManager.current.palette[i], CreatorManager.current.palette[0], 0.6f));
        }
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

        worldData.reds[index] = color.r;
        worldData.greens[index] = color.g;
        worldData.blues[index] = color.b;
    }

    public void ChangeWorldName(TMP_InputField inputField)
    {
        worldName = inputField.text;
    }

    public void SaveWorld()
    {
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
        return worldData.worldName;
    }

    public void LoadLevel (string levelPath)
    {
        editorMenuCanvas.SetActive(false);
        editorCanvas.SetActive(true);
        CreatorManager.current.palette = new Color[]
        {
            new Color(worldData.reds[0], worldData.greens[0], worldData.blues[0]),
            new Color(worldData.reds[1], worldData.greens[1], worldData.blues[1]),
            new Color(worldData.reds[2], worldData.greens[2], worldData.blues[2]),
            new Color(worldData.reds[3], worldData.greens[3], worldData.blues[3]),
            new Color(worldData.reds[4], worldData.greens[4], worldData.blues[4]),
            new Color(worldData.reds[5], worldData.greens[5], worldData.blues[5]),
            new Color(worldData.reds[6], worldData.greens[6], worldData.blues[6]),
            new Color(worldData.reds[7], worldData.greens[7], worldData.blues[7]),
            new Color(worldData.reds[8], worldData.greens[8], worldData.blues[8]),
            new Color(worldData.reds[9], worldData.greens[9], worldData.blues[9]),
            new Color(worldData.reds[10], worldData.greens[10], worldData.blues[10])
        };
        CreatorManager.current.LoadLevel(levelPath);
        CreatorManager.current.OpenLevel();
    }

    public void ReturnToTitleScene()
    {
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
    }
}