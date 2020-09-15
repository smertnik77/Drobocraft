using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RuntimePreviewGenerator;
using UnityEngine.UI;
using System;
using System.Threading;
public class Main : MonoBehaviour {
    public GameObject PlanePol;
    //public GameObject Music;
    GameObject PanelItem;
    // Use this for initialization
    static bool loaded;
    private Coroutine routine;
    void Awake()
    {
        /*
        if (!loaded)
        {
            AudioSource source = Music.GetComponent<AudioSource>();
            source.volume = 0.02f;
            DontDestroyOnLoad(Music);
			//DontDestroyOnLoad(ModalEscape);
			//DontDestroyOnLoad(ModalOptions);
        }

        else
        {
            Destroy(Music);
			//Destroy(ModalEscape);
			//Destroy(ModalOptions);
        }
        loaded = true;
        */
    }
    void Start () {
        Setup.Start();
        Setup.Pause = false;
        for (int z = 0; z < 31; z++)
            for (int x = 0; x < 31; x++)
            {
                GameObject NO = Instantiate(PlanePol);
                NO.transform.position = new Vector3(x, 0, z);
                NO.name = "PlaneP";
                NO.transform.parent = GameObject.Find("Pole").transform;
            }
        String s = PlayerPrefs.GetString(string.Format("robot{0}", Setup.IdActiveRobot));
        routine = StartCoroutine(Setup.LoadRobot(s));
        for (int i = 0; i < Setup.RobotsCount; i++)
        {
            GameObject PanelItem=Instantiate (GameObject.Find ("testPanelItem"),GameObject.Find ("PanelPreview").transform);
            PanelItem.name = string.Format("PanelItem{0}", i);
            Texture2D t = new Texture2D(200, 170);
            t = Setup.ReadTextureFromPlayerPrefs(string.Format("preview{0}", i));
			if (t == null) {
				t = new Texture2D(200, 170);
				for (int y = 0; y < t.height; y++) {
					for (int x = 0; x < t.width; x++) {
						t.SetPixel(x, y, new Color(34,39,48));
					}
				}
				t.Apply();
			}
            PanelItem.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(t, new Rect(0, 0, 200, 170), new Vector2(0, 0));
            PanelItem.transform.Find("InputField").GetComponent<InputField>().text = PlayerPrefs.GetString(string.Format("robotName{0}", i));
            PanelItem.transform.Find("InputField").GetComponent<InputField>().onValueChanged.AddListener(delegate{
                ValueChangeCheck(PanelItem);
            });   
            PanelItem.GetComponent<Button>().onClick.AddListener(() => PanelOnClick(PanelItem));
        }
        Destroy(GameObject.Find("testPanelItem"));
        GameObject.Find("AddRobotPanel").GetComponent<Button>().onClick.AddListener(AddRobot);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

    }

    // Update is called once per frame
    void Update () {
       
    }
    public int ValueChangeCheck(GameObject x)
    {
        string s=x.transform.Find("InputField").GetComponent<InputField>().text;
        int i= int.Parse(x.name.Remove(0, 9));
        PlayerPrefs.SetString(string.Format("robotName{0}", i), s);
        PlayerPrefs.Save();
		Input.ResetInputAxes ();
        return 0;
    }
    public void PanelOnClick(GameObject x)
    {
        if (Setup.IdActiveRobot == int.Parse(x.name.Remove(0, 9))) return;
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        Setup.IdActiveRobot= int.Parse(x.name.Remove(0,9));
        PlayerPrefs.SetInt("IdActiveRobot", Setup.IdActiveRobot);
        PlayerPrefs.Save();
        foreach (Transform child in GameObject.Find("Figures").transform)
        {
            Destroy(child.gameObject);
        }
       
        String s = PlayerPrefs.GetString(string.Format("robot{0}", Setup.IdActiveRobot));
        routine=StartCoroutine(Setup.LoadRobot(s));
    }
    public void AddRobot()
    {
        Setup.IdActiveRobot = Setup.RobotsCount;
        Setup.RobotsCount++;
        PlayerPrefs.SetInt("IdActiveRobot", Setup.IdActiveRobot);
        PlayerPrefs.SetInt("RobotsCount", Setup.RobotsCount);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Edit");
    }
}