using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
public class EscapePanel : MonoBehaviour {
    [SerializeField]
    GameObject ModalEscape;
	//public GameObject ModalOptions;

	Button Escape, Cancel, Edit, Test, Main,Options;
    // Use this for initialization
    void Start () {
		ModalEscape = Setup.EscapePanel;// GameObject.Find("EscapePanel");

		ModalEscape.SetActive(true);
		Escape = GameObject.Find("ButtonEscape").GetComponent<Button>();
		Cancel = GameObject.Find("ButtonCancel").GetComponent<Button>();
		Edit = GameObject.Find("ButtonEdit").GetComponent<Button>();
		Test = GameObject.Find("ButtonTest").GetComponent<Button>();
		Main = GameObject.Find("ButtonMain").GetComponent<Button>();
		Options = GameObject.Find("ButtonOptions").GetComponent<Button>();

        Escape.onClick.AddListener(EscapeOnClick);
        Cancel.onClick.AddListener(CancelOnClick);
        Edit.onClick.AddListener(EditOnClick);
        Test.onClick.AddListener(TestOnClick);
        Main.onClick.AddListener(MainOnClick);
		ModalEscape.SetActive(false);
		Options.onClick.AddListener(OptionsOnClick);


		string Name = SceneManager.GetActiveScene ().name;
		if (Name == "Edit")
			Edit.gameObject.SetActive (false);
		if (Name == "Test")
			Test.gameObject.SetActive (false);
		if (Name == "Main")
			Main.gameObject.SetActive (false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ModalEscape.activeSelf)
            {
				CancelOnClick();
            }
            else
            {
				if(!Setup.Pause)EscapeMenuOnClick();
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            MainOnClick();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            EditOnClick();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestOnClick();
        }
		/*if (Input.GetKeyDown(KeyCode.O))
		{
			OptionsOnClick();
		}*/
    }

	void OptionsOnClick()
	{
		ModalEscape.SetActive(false);
		//ModalOptions.SetActive(true);
	}
    public void EscapeMenuOnClick()
    {
        Setup.Pause = true;
        ModalEscape.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
		if (SceneManager.GetActiveScene ().name == "Edit") {
		}
    }

    void MainOnClick()
    {
        if(SceneManager.GetActiveScene().name=="Edit") Setup.SaveBlocks();
        SceneManager.LoadScene("Main");
        
    }
    void EditOnClick()
    {
        SceneManager.LoadScene("Edit");
    }
    void TestOnClick()
    {
        if (SceneManager.GetActiveScene().name == "Edit") Setup.SaveBlocks();
        SceneManager.LoadScene("Test");
    }
    void CancelOnClick()
    {
        ModalEscape.SetActive(false);
        if (SceneManager.GetActiveScene().name == "Edit"|| SceneManager.GetActiveScene().name == "Test")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
		if (SceneManager.GetActiveScene ().name == "Edit") {
		}
        Setup.Pause = false;
    }
    void EscapeOnClick()
    {
        if (SceneManager.GetActiveScene().name == "Edit") Setup.SaveBlocks();
        #if UNITY_EDITOR
            Setup.Figure1.color=new Color32(58, 90, 186, 255);
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
