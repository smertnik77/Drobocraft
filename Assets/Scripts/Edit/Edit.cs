using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UI;
using static RuntimePreviewGenerator;
public class Edit : MonoBehaviour
{
	public Texture2D pricel;
	private Material Shablon;
	private Material red;
	private AudioClip On,Off,Turn,Click;
	private GameObject PlanePol;
	private GameObject Marker;
    GameObject pMarker;
    private GameObject Line;
    GameObject nObject,lineObject;
	float lineX=15f;
	Camera cam;
	RaycastHit[] hits;
	int delta=90;
	Boolean simMode=false;
	Texture[] prev=new Texture[Setup.FiguresCount];
	Dictionary<int, Color> ColorArr = new Dictionary<int, Color>();
	private bool rotating=false;
	GameObject ColorPanel;
	GameObject FigurePanel;
	GameObject InfoPanel;
	GameObject WeaponPanel;
	int Cpu=0;
	Button btnLeft;
	Button btnRight;
	Button btnDelete;
	int ButtonIndex=1;
    void Start()
	{
        Setup.Start();
		ColorPanel=GameObject.Find("ColorPanel");
		InfoPanel=GameObject.Find("InfoPanel");

		Shablon=Resources.Load("Materials/Shablon", typeof(Material)) as Material;
		red=Resources.Load("Materials/Red", typeof(Material)) as Material;
		On = Resources.Load("Audio/On", typeof(AudioClip)) as AudioClip;
		Off = Resources.Load("Audio/Off", typeof(AudioClip)) as AudioClip;
		Turn = Resources.Load("Audio/Turn", typeof(AudioClip)) as AudioClip;
		Click = Resources.Load("Audio/Click", typeof(AudioClip)) as AudioClip;
		PlanePol = Resources.Load("Models/Edit/PlanePol", typeof(GameObject)) as GameObject;
		Marker = Resources.Load("Models/Edit/Marker", typeof(GameObject)) as GameObject;
		Line = Resources.Load("Models/Edit/Line", typeof(GameObject)) as GameObject;
        Setup.Pause = false;
       
		cam = Camera.main;

		for (int z = 0; z < 31; z++)
			for (int x = 0; x < 31; x++) { 
				GameObject NO = Instantiate (PlanePol);
				NO.transform.position = new Vector3 (x,0,z);
				NO.name = "PlaneP";
				NO.transform.parent = GameObject.Find("Pole").transform;
			}
        Setup.LoadBlocks(ref Cpu);
		CpuUpdate ();
		transform.LookAt (GameObject.Find("Figures").transform);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
        
		madeFigureShablon ();
		CreateFigurePanel ();
		CreateColorPanel ();
		CreateWeaponPanel ();
    }
	void PlayAudio(AudioClip x)
	{
		AudioSource audio = GetComponent<AudioSource>();
		audio.clip = x;
		audio.Play();
	}
	void CreateWeaponPanel()
	{
		Button btnOk = GameObject.Find ("OkButton").GetComponent<Button>();
		btnRight = GameObject.Find ("RightButton").GetComponent<Button>();
		btnLeft = GameObject.Find ("LeftButton").GetComponent<Button>();
		btnDelete = GameObject.Find ("DeleteButton").GetComponent<Button>();
		WeaponPanel= GameObject.Find ("WeaponPanel");

		btnOk.onClick.AddListener(OkButtonOnClick);
		btnRight.onClick.AddListener(RightButtonOnClick);
		btnLeft.onClick.AddListener(LeftButtonOnClick);
		btnDelete.onClick.AddListener(DeleteButtonOnClick);

		Button btn1 = GameObject.Find ("GridItem1").transform.Find ("WeaponItem").GetComponent<Button>();
		btn1.onClick.AddListener(()=>GridItemOnClick(btn1,1));
		Button btn2 = GameObject.Find ("GridItem2").transform.Find ("WeaponItem").GetComponent<Button>();
		btn2.onClick.AddListener(()=>GridItemOnClick(btn2,2));
		Button btn3 = GameObject.Find ("GridItem3").transform.Find ("WeaponItem").GetComponent<Button>();
		btn3.onClick.AddListener(()=>GridItemOnClick(btn3,3));

		for (int x = 0; x < 3; x++) {
			PutOnWeaponPanel (x+1, Setup.LA[x]);
		}

		EditButtonsUpdate();
		WeaponPanel.SetActive (false);
	}
	void EditButtonsUpdate()
	{
		btnLeft.gameObject.SetActive (ButtonIndex != 1);
		btnRight.gameObject.SetActive (ButtonIndex != 3);
		btnDelete.gameObject.SetActive (Setup.LA[ButtonIndex-1]>-1);
	}
	void PutOnWeaponPanel(int index,int idFigure){
		Boolean www = false;
		if (!WeaponPanel.activeSelf) {
			WeaponPanel.SetActive (true);
			www = true;
		}
		GameObject g = GameObject.Find ("GridItem"+index);
		Image I = g.transform.Find ("Image").GetComponent<Image> ();
		Button btn = g.transform.Find ("WeaponItem").GetComponent<Button>();
		if(idFigure<0){
			I.sprite = null;
			I.color = new Color32 (255,255,255,0);
			btn.transform.Find ("Text2").GetComponent<Text> ().text = "";
		}
		else {
			RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
			Texture prev = RuntimePreviewGenerator.GenerateModelPreview(Setup.Figures[idFigure].GO.transform, 100, 100, false) as Texture;
			I.sprite=Sprite.Create((Texture2D) prev ,new Rect(0,0,100,100),new Vector2(0.5f,0.5f));
			I.color = new Color32 (255, 255, 255, 255);
			btn.transform.Find ("Text2").GetComponent<Text> ().text = Setup.Figures [idFigure].Name;
		}
		Setup.LA [index-1] = idFigure;
		if(www)WeaponPanel.SetActive (false);
	}

	void GridItemOnClick(Button btn, int x)
	{
		PlayAudio(Click);
		GameObject EditButtons = GameObject.Find ("EditButtons");
		EditButtons.transform.position = new Vector2(btn.transform.position.x,EditButtons.transform.position.y);
		ButtonIndex = x;
		EditButtonsUpdate();
	}
	void DeleteButtonOnClick()
	{
		int value = Setup.LA [ButtonIndex - 1];

		PutOnWeaponPanel (ButtonIndex, -1);

		btnDelete.gameObject.SetActive (false);
		PlayAudio(Click);
		if (value != -1) {
			print (value);
			foreach (Transform child in GameObject.Find ("Figures").transform) {
				if(Setup.Figures [value].Name==child.name){
					Destroy (child.gameObject);
				}
			}
		}
	}
	void RightButtonOnClick()
	{
		PlayAudio(Click);
		GameObject EditButtons = GameObject.Find ("EditButtons");
		EditButtons.transform.position = new Vector2(EditButtons.transform.position.x+196,EditButtons.transform.position.y);
		ButtonIndex++;
		GameObject.Find ("GridItem"+(ButtonIndex).ToString()).transform.Find ("WeaponItem").GetComponent<Button> ().Select ();

		int value1 = Setup.LA [ButtonIndex-1];
		int value2 = Setup.LA [ButtonIndex-2];

		PutOnWeaponPanel (ButtonIndex, value2);
		PutOnWeaponPanel (ButtonIndex-1, value1);
		EditButtonsUpdate();

	}
	void LeftButtonOnClick()
	{
		PlayAudio(Click);
		GameObject EditButtons = GameObject.Find ("EditButtons");
		EditButtons.transform.position = new Vector2(EditButtons.transform.position.x-196,EditButtons.transform.position.y);
		ButtonIndex--;
		GameObject.Find ("GridItem"+(ButtonIndex).ToString()).transform.Find ("WeaponItem").GetComponent<Button> ().Select ();

		int value1 = Setup.LA [ButtonIndex-1];
		int value2 = Setup.LA [ButtonIndex];

		PutOnWeaponPanel (ButtonIndex, value2);
		PutOnWeaponPanel (ButtonIndex+1, value1);
		EditButtonsUpdate();

	}
	void OkButtonOnClick()
	{
		TriggerPanel (WeaponPanel);
	}

	void CreateFigurePanel()
	{
		RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 1);
		RuntimePreviewGenerator.OrthographicMode = true;
		RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.7f, -0.4f, -1f);
		RuntimePreviewGenerator.Padding = 0.1f;
		for (int i = 0; i < Setup.FiguresCount; i++) {
			Setup.Figures[i].GO.transform.GetComponent<MeshRenderer>().sharedMaterials[0].color = new Color32(58, 90, 186, 255);
			prev[i] = RuntimePreviewGenerator.GenerateModelPreview(Setup.Figures[i].GO.transform, 200, 200, false) as Texture;
			if (Setup.Figures[i].GO.name == "Wheel1") {
				GameObject papa2=new GameObject("papa2");
				GameObject Child = GameObject.Instantiate(Setup.Figures[i].GO);
				Child.transform.parent = papa2.transform;
				Child.transform.name="Child";
				Child.transform.Rotate (0,180,90);
				prev[i] = RuntimePreviewGenerator.GenerateModelPreview(papa2.transform, 200, 200, false) as Texture;
				Destroy (papa2);
			}
			GameObject button=Instantiate (GameObject.Find ("FigureItem"),GameObject.Find ("Content").transform);
			button.name=i.ToString();
			button.transform.Find("Panel1").Find("Image").GetComponent<Image>().sprite = Sprite.Create((Texture2D) prev [i] ,new Rect(0,0,200,200),new Vector2(0.5f,0.5f));
			button.GetComponent<Button> ().onClick.AddListener (()=>buttonClick(button,i));
			button.transform.Find("Panel2").Find("Text").GetComponent<Text>().text=Setup.Figures[i].Cpu.ToString()+" CPU";
			button.transform.Find("Panel3").Find("Text").GetComponent<Text>().text=
				Setup.Figures[i].Name+"\r\n"+Setup.Figures[i].Faces+" Faces";
		}
		Destroy (GameObject.Find ("FigureItem"));
		FigurePanel= GameObject.Find ("FigurePanel");
		GameObject c= GameObject.Find ("Content");
		int s = c.transform.childCount/9;
		if((c.transform.childCount%9)>0)s++;
		c.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0,s*310+10);
		FigurePanel.SetActive (false);
	}
    int buttonClick(GameObject btn, int x)
    {
        Setup.IdCube = Convert.ToInt32(btn.name);
		FigurePanel.SetActive(false);
		InfoPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
		PlayAudio(Click);
		Setup.Pause = false;
		madeFigureShablon ();
        return 0;
    }
	void CreateColorPanel()
	{
		Transform panel = GameObject.Find ("ColorGrid").transform;
		for (int i = 0; i < 32; i++) {

			GameObject button=Instantiate (GameObject.Find ("ColorItem"),panel);
			button.name=i.ToString();
			ColorBlock x=button.GetComponent<Button> ().colors;
			Color color=Setup.Palette[i];
			x.normalColor = color;
			color.a = 0.5f;
			x.highlightedColor = color;
			x.pressedColor = color;
			button.GetComponent<Button> ().colors = x;
			button.GetComponent<Button> ().onClick.AddListener (()=>buttonColorClick(button,i));
		}
		Destroy (GameObject.Find ("ColorItem"));
		ColorPanel.SetActive(false);
	}
	int buttonColorClick(GameObject btn, int x)
	{
		Setup.MainColor = Setup.Palette[Convert.ToInt32(btn.name)];
		ColorPanel.SetActive(false);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		PlayAudio(Click);
		Setup.Pause = false;
		madeFigureShablon ();
		return 0;
	}
	void end(){
		if (SceneManager.GetActiveScene().name == "Edit") Setup.SaveBlocks();
		#if UNITY_EDITOR
		Setup.Figure1.color=new Color32(58, 90, 186, 255);
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
	void madeFigureShablon()
	{
		Destroy(GameObject.Find("FigureShablon"));
		GameObject eee=GameObject.Instantiate(Setup.Figures[Setup.IdCube].GO,GameObject.Find("FigureParent").transform);
		eee.transform.localPosition = Vector3.zero;// (0, 1.1f, -1);
		//eee.transform.localEulerAngles = new Vector3 (-45, 0, 0);
		eee.transform.localScale = new Vector3 (0.4f,0.4f, 0.4f);
		//eee.transform.GetComponent<MeshCollider> ().enabled = false;
		DisableMesh (eee.transform);
		eee.name = "FigureShablon";
		foreach (Transform child in eee.transform) {
			if (child.tag == "Plane")
				Destroy (child.gameObject);
		}
		Setup.Layered(eee,8);	
		//Setup.pokras (eee.transform, Setup.MainColor );
		RefreshFigureShablonColor (Setup.MainColor);
		eee.transform.Rotate (0,delta-90,0);
	}

	void RefreshFigureShablonColor(Color c)
	{
		//c=new Color(c.r/4, c.g/4,c.b/4);
		//c=c/4;
		Setup.pokras (GameObject.Find("FigureShablon").transform, c);
	}

	private IEnumerator Rotated( Vector3 angles)
	{
		Transform or = GameObject.Find ("FigureShablon").transform;
		rotating = true ;
		Quaternion from = or.localRotation;
		Quaternion to =Quaternion.Euler(or.localEulerAngles+angles);
		float t = 0;
		while (t < 1f) {
			t += Time.deltaTime*10;
			or.localRotation = Quaternion.Lerp (from, to, t);
			yield return null;
		}
		rotating = false;
	}

	public void DisableMesh(Transform x)
	{
		x.transform.GetComponent<MeshCollider> ().enabled = false;
		foreach (Transform child in x) {
			if(child.tag!="Plane")
				DisableMesh (child);
		}
	}

	void CpuUpdate()
	{
		GameObject.Find ("Cpu").GetComponent<Text> ().text = Cpu.ToString();
		GameObject.Find ("CpuSlider").GetComponent<Slider> ().value = Cpu;
	}
	void TriggerPanel(GameObject panel)
	{
		panel.SetActive(!panel.activeSelf);
		Cursor.visible = panel.activeSelf;
		if (!panel.activeSelf)
			Cursor.lockState = CursorLockMode.Locked;
		else Cursor.lockState = CursorLockMode.None;
		Setup.Pause=panel.activeSelf;
		Input.ResetInputAxes ();
	}

    void Update()
	{
		hits = Physics.RaycastAll (cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))).OrderBy(h=>h.distance).ToArray();
		if (Input.GetKeyDown (KeyCode.L) && (!Setup.Pause || (Setup.Pause && WeaponPanel.activeSelf)))
		{
			TriggerPanel (WeaponPanel);
			if (WeaponPanel.activeSelf) {
				EditButtonsUpdate ();
				GameObject.Find ("GridItem" + (ButtonIndex).ToString ()).transform.Find ("WeaponItem").GetComponent<Button> ().Select ();
			}
		}
		if (Input.GetKeyDown(KeyCode.C)&&(!Setup.Pause||(Setup.Pause&&ColorPanel.activeSelf)))
        {
			TriggerPanel (ColorPanel);
        }
		if (Input.GetKeyDown(KeyCode.Tab)&&(!Setup.Pause||(Setup.Pause&&FigurePanel.activeSelf)))
		{
			TriggerPanel (FigurePanel);
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (FigurePanel.activeSelf)
			{
				TriggerPanel (FigurePanel);
				return;
			}
			if (ColorPanel.activeSelf)
			{
				TriggerPanel (ColorPanel);
				return;
			}
			if (WeaponPanel.activeSelf)
			{
				TriggerPanel (WeaponPanel);
				return;
			}
		}

		if (Setup.Pause) return;
        if (Input.GetKeyDown (KeyCode.Comma)) {
			foreach (Transform ch in GameObject.Find("Figures").transform) {
				ch.position=new Vector3 (ch.position.x,ch.position.y-1,ch.position.z);
			}
		}
		if (Input.GetKeyDown (KeyCode.Period)) {
			foreach (Transform ch in GameObject.Find("Figures").transform) {
				ch.position=new Vector3 (ch.position.x,ch.position.y+1,ch.position.z);
			}
		}

		if((Input.GetAxis("Mouse ScrollWheel")<0)&&!rotating)
		{
			if(Setup.IntToName(Setup.IdCube)=="Wheel1")return;
			delta -= 90;
			if (delta == -90)delta = 270;
			PlayAudio(Turn);
			StartCoroutine( Rotated( new Vector3(0, -90, 0) ) ) ;
        }
		if((Input.GetAxis("Mouse ScrollWheel")>0)&&!rotating)
		{
			if(Setup.IntToName(Setup.IdCube)=="Wheel1")return;
			delta += 90;
			if (delta == 360)delta = 0;
			PlayAudio(Turn);
			StartCoroutine( Rotated( new Vector3(0, 90, 0) ) ) ;
        }
		if(Input.GetKeyDown(KeyCode.PageUp)){
			Setup.IdCube--;
			if (Setup.IdCube < 0) Setup.IdCube = Setup.FiguresCount-1;
			madeFigureShablon ();
		}
		if(Input.GetKeyDown(KeyCode.PageDown)){
            Setup.IdCube++;
			if (Setup.IdCube == Setup.FiguresCount) Setup.IdCube = 0;
			madeFigureShablon ();
		}
		if(Input.GetKeyDown(KeyCode.F)){
			foreach (Transform child in GameObject.Find ("Figures").transform) {
				if (Color.Equals (child.GetComponent<MeshRenderer> ().material.color, red.color))
					continue;
				Setup.pokras (child, Setup.MainColor);
			}
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			testing ();
		}
        if (Input.GetKeyDown(KeyCode.V)){
			simMode=simMode ? false : true;
			if (simMode) {
				lineObject=Instantiate(Line);
				lineObject.name="Line";
				lineObject.transform.eulerAngles = new Vector3 (0,270,0);
				lineObject.transform.position = new Vector3 (lineX, 0.505f, 15f);
				madeShablon2();
				return;
			} else {
				Destroy (GameObject.Find ("Line"));
				DeleteShablon2 ();
				return;
			}

		}
		if(simMode){
			Transform t = lineObject.transform;
			if (Input.GetKeyDown (KeyCode.LeftBracket)) {
				if (lineX > 0) {
					lineX -= 0.5f;
					t.position = new Vector3 (lineX, t.position.y, t.position.z);
					DeleteShablon2 ();
					madeShablon2();
					return;
				}
			}
			if (Input.GetKeyDown (KeyCode.RightBracket)) {
				if (lineX < 30) {
					lineX += 0.5f;
					t.position = new Vector3 (lineX, t.position.y, t.position.z);
					DeleteShablon2 ();
					madeShablon2();
					return;
				}
			}
		}
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (hits.Length > 0)
            {
                String s;
                foreach (RaycastHit x in hits)
                {
                    s = x.transform.name;
                    if ((x.transform.tag == "Plane")||(x.transform.name == "Plane")) print(s+" "+x.transform.position+ x.transform.parent.name);
                    else print(s + " " + x.transform.position);
                }
				print (IndexFigurePlanRay ());
            }
        }
		if(Input.GetKeyDown(KeyCode.Delete)) {
			foreach (Transform child in GameObject.Find ("Figures").transform)
				Destroy (child.gameObject);
			Cpu=0;
			CpuUpdate ();
		}
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			if (hits.Length > 0) {
				if (isFigure(hits [0].transform.tag))	return;
				if (GameObject.FindGameObjectsWithTag ("Shablon").Length == 0)	return;
				if (GameObject.Find ("Marker").GetComponent<MeshRenderer> ().material.color == Color.red)
					return;
				PlayAudio(On);
				Transform tr = GameObject.FindGameObjectsWithTag ("Shablon")[0].transform;
				DeleteShablon();
				Transform xx= Setup.madeFigure (Setup.IdCube, tr.position, tr.eulerAngles, Setup.MainColor);
				Cpu+=Setup.Figures[Setup.IdCube].Cpu;
				CpuUpdate ();
				redFigure (xx);
				if (simMode)
				{
					if (Math.Abs(lineX-tr.position.x)<0.2) {
						return;
					}
					if (GameObject.FindGameObjectsWithTag ("Shablon2") [0]) {
						if (GameObject.FindGameObjectsWithTag ("Shablon2") [0].GetComponent<MeshRenderer> ().material.color == red.color)
							return;
					}
					//добавить проверку на занятую точку
					if (haveFigure(new Vector3(2 * (lineX - tr.position.x) + tr.position.x, tr.position.y, tr.position.z)))
					{
						return;
					}
					Vector3 vv = new Vector3 (2*(lineX-tr.position.x)+tr.position.x,tr.position.y,tr.position.z);
					Transform yy = Setup.madeFigure (Setup.IdCube, vv, tr.eulerAngles, Setup.MainColor);
					Cpu+=Setup.Figures[Setup.IdCube].Cpu;
					CpuUpdate ();
					yy.eulerAngles = SimRotate(tr.eulerAngles);
					if (yy.name == "Wheel1") {
						yy.transform.Rotate (0, 180, 0);
					}
					redFigure (yy);
				}
			}
			if(Setup.Figures[Setup.IdCube].Type=="Weapon"){
				if (!Setup.LA.Contains (Setup.IdCube)) {
					print ("если не содержит то добавить");
					if (Setup.LA [0] == -1)PutOnWeaponPanel (1, Setup.IdCube);
					else if (Setup.LA [1] == -1)PutOnWeaponPanel (2, Setup.IdCube);
					else if (Setup.LA [2] == -1)PutOnWeaponPanel (3, Setup.IdCube);

					TriggerPanel (WeaponPanel);
					EditButtonsUpdate ();
					GameObject.Find ("GridItem" + (ButtonIndex).ToString ()).transform.Find ("WeaponItem").GetComponent<Button> ().Select ();
				}
			}
			return;
		}
		if (Input.GetKeyDown (KeyCode.Mouse1)) {
			if (hits.Length > 0) {
				foreach (RaycastHit w in hits) {
					if (isFigure( w.transform.tag)) {
						Transform t = w.transform;
						while (isFigure(t.parent.tag))t = t.parent.transform;
						Cpu-=Setup.Figures[Setup.NameToInt(t.name)].Cpu;
						CpuUpdate();
						Destroy(t.gameObject);
						figToRed (t);
						madeFreePlanes (t);
						if (simMode&&Math.Abs(lineX - t.position.x) > 0.2f) {
							Vector3 vv = new Vector3 (2*(lineX-t.position.x)+t.position.x,t.position.y,t.position.z);
							GameObject g = Setup.figureAtPos (vv);
							//надо добавить проверку что есть объект
							if (g != null) {
								Cpu-=Setup.Figures[Setup.NameToInt(g.name)].Cpu;
								CpuUpdate ();
								Destroy(g);
								figToRed (g.transform);
								madeFreePlanes (g.transform);
							}
						}
						PlayAudio(Off);
						if(Setup.Figures[Setup.NameToInt(t.name)].Type=="Weapon"){
							foreach (Transform child in GameObject.Find ("Figures").transform) {
								if(child.GetInstanceID()!=t.GetInstanceID()) {
									if(t.name==child.name){
										return;
									}
								}
							}

							if (Setup.LA.Contains (Setup.IdCube)) {
								PutOnWeaponPanel (Array.IndexOf( Setup.LA, Setup.NameToInt(t.name) )+1, -1);
							}
						}
						return;

					}
				}
			}
        }
		if (Input.GetKeyDown (KeyCode.Mouse4)) {
			if (SceneManager.GetActiveScene().name == "Edit") Setup.SaveBlocks();
			#if UNITY_EDITOR
			Setup.Figure1.color=new Color32(58, 90, 186, 255);
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
		//луч никуда не попал
		if (hits.Length == 0) {
			DeleteShablon();
            return;
		}
		if (hits.Length > 0) {
			int index = -1;
			if (HaveFigureRay()) {
				index = IndexFigurePlanRay ();
				if (index>=0) {
					//print ("есть фигура c плоскостью index="+index);
				} else {
					//print ("есть фигура без плоскости");
					DeleteShablon();
					//Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
					RefreshFigureShablonColor (Setup.MainColor);
					return;
				}
			} else {
				if (HavePlanPRay()) {
					index = IndexPlanPRay ();
					//print ("есть пол index="+index);
				} else {
					//print ("нет фигуры и пола");
					DeleteShablon();
					//Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
					RefreshFigureShablonColor (Setup.MainColor);
					return;
				}

			}
			// вычисляем позицию для фигуры
			RaycastHit Ray=hits[index];
			//print (Ray.transform.position);
			Vector3 pos=posFigureOnPlane(Ray.transform);
            // если позиця уже занята то выходим
			/*if(haveFigurePlane (pos, Ray.transform.rotation)) {
				DeleteShablon();
				RefreshFigureShablonColor (Setup.MainColor);
				return;
			}*/

			if (haveFigure(pos)){
				DeleteShablon();
				RefreshFigureShablonColor (Setup.MainColor);
				//print (4);
				return;
			}
			// колеса только слева и справа
			if (
					(Setup.IntToName (Setup.IdCube) == "Wheel")||
					(Setup.IntToName (Setup.IdCube) == "Wheel1")
				) {
				string s = Setup.naklon (Ray.transform.eulerAngles); ///

				if (s != "left" && s != "right") {
					DeleteShablon();
					RefreshFigureShablonColor (Setup.MainColor);
					return;	
				}
			}
			//ПРОВЕРКА
			if (GameObject.FindGameObjectsWithTag ("Shablon").Length > 0) {
				GameObject ii=GameObject.FindGameObjectsWithTag ("Shablon")[0];
				if 	(
						(ii.transform.position==pos)&&
					((ii.name=="Wheel1")||(ii.transform.rotation==Ray.transform.rotation*Quaternion.Euler(0,delta,0)))&&
						(ii.name== Setup.IntToName(Setup.IdCube))&&
						(GameObject.Find("FigureShablon").transform.GetComponent<MeshRenderer>().materials[0].color==
							Setup.MainColor)
					) 
				{
					return;
				}
			}
			// удаляем шаблон нового кубика
			DeleteShablon();
			// создаем шаблон нового кубика
			//Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
			RefreshFigureShablonColor (Setup.MainColor);




            nObject = madeShablon("Shablon",pos, Ray.transform.rotation, true);
			nObject.AddComponent<shablon>();
			//nObject.GetComponent<MeshCollider> ().isTrigger = true;
			nObject.AddComponent<Rigidbody> ();
			nObject.GetComponent<Rigidbody> ().isKinematic = true;
			nObject.GetComponent<Rigidbody> ().useGravity = false;

            // создаем подсветку примыкающей плоскости
            pMarker = Instantiate (Marker,Ray.transform.position,nObject.transform.rotation);
			pMarker.transform.parent = nObject.transform;
			pMarker.transform.GetComponent<MeshRenderer> ().material =  Shablon;
			pMarker.transform.localPosition = new Vector3 (0, 0.02f, 0);
			pMarker.name = "Marker";
			if (simMode)
            {
				madeShablon2 ();
            }
        }
	}
	void madeShablon2()
	{
		if (nObject == null)
			return;
		float nX = nObject.transform.position.x;
		if( (2 * (lineX - nX)+ nX)<0 )
			return;
		if( (2 * (lineX - nX)+ nX)>30 )
			return;
		if (Math.Abs(lineX - nX) < 0.2)
		{
			return;
		}
		if (haveFigure(new Vector3(2 * (lineX - nX)
			+ nX, nObject.transform.position.y, nObject.transform.position.z)))
		{
			return;
		}
		Vector3 vv = new Vector3(2 * (lineX - nX) + 
			nX, nObject.transform.position.y, nObject.transform.position.z);
		GameObject Shablon2 = madeShablon("Shablon2",vv, nObject.transform.rotation);
		Shablon2.transform.eulerAngles = SimRotate(nObject.transform.eulerAngles);

		if (Shablon2.name == "Wheel1") {
			Shablon2.transform.Rotate (0,180, 0);
		}
		Shablon2.AddComponent<shablon>();
		Shablon2.AddComponent<Rigidbody> ();
		Shablon2.GetComponent<Rigidbody> ().isKinematic = true;
		Shablon2.GetComponent<Rigidbody> ().useGravity = false;
	}
	Boolean HaveFigureRay()
	{
		foreach (RaycastHit r in hits) {
			if (r.transform.tag=="Figure") {
				return true;
			}
		}
		return false;
	}
	int IndexFigure()
	{
		int i = 0;
		foreach (RaycastHit r in hits) {
			if (r.transform.tag=="Figure") {
				return i; 
			}
			i++;
		}	
		return -1;
	}
	int IndexFigurePlanRay()
	{
		int i = 0;
		foreach (RaycastHit r in hits) {
			if (r.transform.tag=="Figure") {
				if (i == 0)
					return -1;
				if (hits [i - 1].transform.tag == "Plane")
					return i - 1;
				else
					return -1;
			}
			i++;
		}
		return -1;
	}

	Boolean HavePlanPRay()
	{
		foreach (RaycastHit r in hits) {
			if (r.transform.name=="PlaneP") {
				return true;
			}
		}
		return false;
	}

	int IndexPlanPRay()
	{
		int i = 0;
		foreach (RaycastHit r in hits) {
			if (r.transform.name=="PlaneP") {
				return i;
			}
			i++;
		}
		return -1;
	}

	public void OnGUI ()
	{
        if(!FigurePanel.activeSelf&&!Setup.Pause)
        GUI.DrawTexture(new Rect(Screen.width / 2-5, Screen.height / 2-5, 10, 10), pricel);
    }
	void DeleteShablon ()
	{
		GameObject[] s1 = GameObject.FindGameObjectsWithTag ("Shablon");
		if(s1.Length>0)Destroy(s1[0]);
		GameObject[] s2 = GameObject.FindGameObjectsWithTag ("Shablon2");
		if(s2.Length>0)Destroy(s2[0]);
	}
	void DeleteShablon2()
	{
		GameObject[] s2 = GameObject.FindGameObjectsWithTag ("Shablon2");
		if(s2.Length>0)Destroy(s2[0]);
	}
	GameObject madeShablon(String tag,Vector3 pos, Quaternion rot,bool WithDelta=false)
    {
		GameObject newFigure = GameObject.Instantiate(Setup.Figures[Setup.IdCube].GO,pos,rot);
		newFigure.transform.localScale=new Vector3(0.499f,0.499f,0.499f);
		newFigure.name=Setup.Figures[Setup.IdCube].Name;
		String ns = newFigure.name;
		if ((ns != "Wheel1") && WithDelta) {
			newFigure.transform.rotation*= Quaternion.Euler(0, delta, 0);
		}
		SettingsMesh (newFigure.transform);
		foreach (Transform child in newFigure.transform)
        {
			if (child.tag == "Plane") {
				Destroy (child.gameObject);
			}
        }
		Setup.Tagged (newFigure.transform,tag);
        Сlarify(newFigure);
        return newFigure;
    }

	public void SettingsMesh(Transform x)
	{
		if (!x.gameObject.GetComponent<MeshCollider> ()) {
			x.gameObject.AddComponent<MeshCollider>();
		}
		x.gameObject.GetComponent<MeshCollider>().convex = true;
		x.gameObject.GetComponent<MeshCollider>().isTrigger = true;
		foreach (Transform child in x) {
			if(child.tag!="Plane")
				SettingsMesh (child);
		}
	}
    Color Transparent(Color x)
    {
        x.a = 0.5f;
        return x;
    }
    void Сlarify(GameObject g)
    {
        int l = g.transform.GetComponent<MeshRenderer>().materials.Length;
        Color[] ca = new Color[l];
        Material[] xMat = new Material[l];
        xMat[0] = Shablon;
        ca[0] = Transparent(Setup.MainColor);
        for (int x = 1; x < l; x++)
        {
            xMat[x] = Shablon;
            ca[x] = Transparent(g.transform.GetComponent<MeshRenderer>().materials[x].color);
        }
        g.transform.GetComponent<MeshRenderer>().materials = xMat;
        for (int x=0;x<l; x++)
        {
            g.transform.GetComponent<MeshRenderer>().materials[x].color = ca[x]; 
        } 
		foreach (Transform child in g.transform) {
			if (child.tag != "Plane")
				Сlarify (child.gameObject);
		}
    }
    Vector3 SimRotate(Vector3 f)
    {
        string s = Setup.naklon(f);
		int sa = Setup.Figures [Setup.IdCube].Sim;
        if (s == "top" || s == "down")
        {
            if (sa == 180)
            {
                if (delta == 0 || delta == 180)
                {
					f += new Vector3 (0, 180, 0);
                }    
            }
            if (sa == -90)
            {
                if (delta == 0 || delta == 180)
                {
					f += new Vector3 (0, 0, 0);
                }
                else
                {
					f += new Vector3 (0, 180, 0);
                }
            }
            if (sa == 90)
            {
				if (s == "top") {
					if (delta == 0 || delta == 180) {
						f += new Vector3 (0, 90, 0);
					} else {
						f += new Vector3 (0, -90, 0);
					}
				}
				if (s == "down") {
					if (delta == 0 || delta == 180) {
						f += new Vector3 (0, -90, 0);
					} else {
						f += new Vector3 (0, 90, 0);
					}
				}
            }
        }

        if (s == "forward" || s == "back")
        {
            if (sa == 180)
            {
                if (delta == 0 || delta == 180)
                {
					f += new Vector3 (0,180, 180);
                }
            }
            if (sa == -90)
            {
                if (delta == 0 || delta == 180)
                {
					f += new Vector3 (0,0, 0);
                }
                else
                {
					f += new Vector3 (180, 0, 180);
                }
            }
            if (sa == 90)
            {
				if (delta == 0)f += new Vector3 (-90, 90, -90);
				if (delta == 90)f += new Vector3 (90, -90, 90);
				if (delta == 180)f += new Vector3 (90, -90, -90);
				if (delta == 270)f += new Vector3 (-90, -90, -90);
            }
        }
        if (s == "left" || s == "right")
        {
            if (sa == 180)
				f += new Vector3 (0, 0, 180);
            if (sa == -90)
            {
				if(delta==90 || delta==270)
					f += new Vector3 (180, 180, 180);
				else
					f += new Vector3 (180, 0, 180);
            }
            if (sa == 90)
            {
				if(delta==0)f += new Vector3 (-90, 180, 0);
				if(delta==90)f += new Vector3 (90, 90,90);
				if(delta==180)f += new Vector3 (90, 180, 0);
				if(delta==270)f += new Vector3 (90, -90, 90);
            }
        }
		return f;
    }
	Boolean haveFigure(Vector3 x) {
		for(int i=0;i<GameObject.Find("Figures").transform.childCount;i++) {
			Transform c = GameObject.Find ("Figures").transform.GetChild (i);
			if (Vector3.Distance (x, c.position) < 0.02f) {
				//print (c.position+" "+x);
				return true;
			}
		}
		return false;
	}

	Boolean haveFigurePlane(Vector3 pos,Quaternion rot) {
		if (Setup.Figures[Setup.IdCube].GO.name != "Wheel1") {
			rot*= Quaternion.Euler(0, delta, 0);
		}
		if(GameObject.Find("Empty")!=null)Destroy(GameObject.Find("Empty"));
		GameObject empty = GameObject.Instantiate(Setup.Figures[Setup.IdCube].GO,pos,rot);
		empty.tag = "Untagged";

		empty.transform.localScale=new Vector3(0.49f,0.49f,0.49f);
		/*
		if (empty.name == "Rotor1") {
			empty.transform.localScale = new Vector3 (0.49f, 0.2f, 0.49f);
			empty.transform.localPosition-=new Vector3(0,0.02f,0);
		}
		*/
		empty.name = "Empty";
		empty.GetComponent<MeshRenderer> ().enabled = false;
		empty.GetComponent<MeshCollider> ().enabled = false;
		foreach (Transform child in empty.transform) {
			if (child.tag == "Plane") {
				child.transform.localScale = new Vector3 (0.190f, 0.0001f, 0.190f);
				child.name = "P";
				child.tag = "Untagged";
				child.GetComponent<MeshCollider> ().enabled = false;
			} else
				Destroy (child.gameObject);
		}
		foreach (Transform child in empty.transform) {
			foreach(GameObject plane in GameObject.FindGameObjectsWithTag("Plane") ) {
				if (Vector3.Distance (child.position, plane.transform.position) < 0.0099f) {
					//plane.GetComponent<MeshRenderer> ().enabled = true;
					Vector3 r = child.position - plane.transform.position;
					print (r.x+" "+r.y+" "+r.z);
					//Destroy(GameObject.Find("Empty"));
					return true;
				}
			}
		}

		//Destroy(GameObject.Find("Empty"));
		return false;
	}
	Boolean redFigure(Transform x) {
		if (GameObject.Find ("Figures").transform.childCount == 1)
			return false;
		//какие рядом фигуры?
		int figs=0;
		int reds = 0;
		foreach (Transform child in x) {
			if (child.tag == "Plane") {
				GameObject g = Setup.figureAtPlane(child);
                if (g != null)
                {
					if (Color.Equals (red.color, g.transform.GetComponent<MeshRenderer> ().material.color)) {
						reds++;
					} else
						figs++;
                }
			}
		}
		//print ("figs="+figs+" reds="+reds);
		//если рядом обычных нет то красим в красный и выходим
		if (figs == 0) {
			redPokras (x);
			return false;
		}
		//если рядом обычные и красных нет то выходим
		if (reds == 0) {
			return true;
		}
		//если рядом обычные и красные
		redToFig(x);
		return true;
	}
	Boolean redToFig(Transform x) {
		foreach (Transform child in x) {
            if (child.tag == "Plane") {
				GameObject g = Setup.figureAtPlane(child);
                if (g != null) {
                    if (Color.Equals(red.color, g.transform.GetComponent<MeshRenderer>().material.color)) {
						antiRedPokras (g.transform);////красим кубик из красного
                        redToFig(g.transform);
                    }
                }
			}
		}
		return true;
	}
	Boolean figToRed(Transform x) {
		if (Color.Equals(red.color,x.GetComponent<MeshRenderer>().material.color)){
			return true;
		}
		int joinFigureCount = joinFiguresCount (x);
		if(joinFigureCount==1)return true;
		int[] rr=new int[joinFigureCount];
		Transform[] rf = new Transform[joinFigureCount];
		int i = 0;
		x.tag="checkFigure";
		foreach (Transform child in x) {
            if (child.name == "Join")
            {
                rr[i] = 0;
				GameObject g = Setup.figureAtPlane(child);
                if ((g != null) && (g.tag != "checkFigure"))
                {
                    rf[i] = g.transform;
                    g.tag = "checkFigure";
                    rr[i]++;
                    foreach (Transform child2 in g.transform)
                    {
                        if (child2.name == "Join")
                        {
                            if (Vector3.Distance(child.position, child2.position) < 0.02f)
                            {
                                child.name = "Plane";//
                                child2.name = "Plane";//
                                rr[i] += figToRed2(g.transform);
                            }
                        }
                    }
                    i++;
                }
			}
		}
		//Красим все группы кубиков в красный кроме самой большой группы
		if(joinFigureCount>1){
			int max=rr.Max();
			for(int y=0;y<joinFigureCount;y++){
				if (max != rr [y]) {
					if(rr[y]>0)justRedFigures (rf [y]);
				}
			}
		}
		foreach(GameObject child in GameObject.FindGameObjectsWithTag("checkFigure")){
			child.tag = "Figure";
		}
		return true;
	}
	int figToRed2(Transform x){
		int count = 0;
		foreach (Transform child in x) {
			if (child.name == "Join") {
				GameObject g = Setup.figureAtPlane(child);
                if ((g != null) && (g.tag != "checkFigure"))
                {
                    g.tag = "checkFigure";
                    count++;
                    foreach (Transform child2 in g.transform)
                    {
                        if (child2.name == "Join")
                        {
                            if (Vector3.Distance(child.position, child2.position) < 0.02f)
                            {
                                count += figToRed2(g.transform);
                            }
                        }
                    }
                }
			}
		}
		return count; 
	}
    Boolean isFigure(String s){
		if (s == "Figure")
			return true;
		return false;
	}
	int joinFiguresCount (Transform t){
		int x = 0;
		foreach (Transform child in t) {
			if (child.name == "Join") {
				x++;
			}
		}
		return x;
	}
	Vector3 posFigureOnPlane(Transform x){
		Vector3 p = x.position;
		if (x.name == "PlaneP") {
			return new Vector3 (p.x,p.y + 1, p.z);
		}
        String s = Setup.naklon(x.eulerAngles);
        if (s == "top")
        {
            return new Vector3(p.x, p.y + 0.5f, p.z);
        }
        if (s == "down")
        {
            return new Vector3(p.x, p.y - 0.5f, p.z);
        }
        if (s == "left")
        {
            return new Vector3(p.x - 0.5f, p.y , p.z);
        }
        if (s == "right")
        {
            return new Vector3(p.x + 0.5f, p.y, p.z);
        }
        if (s == "back")
        {
            return new Vector3(p.x, p.y, p.z - 0.5f);
        }
        if (s == "forward")
        {
            return new Vector3(p.x, p.y, p.z + 0.5f);
        }
        return new Vector3(p.x, p.y, p.z);
    }
	void madeJoinedPlanes(Transform x){
		foreach (Transform child in x) {
			if (child.tag == "Plane") {
				GameObject g = Setup.figureAtPlane(child);
				if (g != null) {
					foreach (Transform child2 in g.transform) {
						if (child2.tag == "Plane") {
							if (Vector3.Distance (child.position, child2.position) < 0.02f) {
								child.name = "Join";
								child2.name = "Join";
							}
						}
					}
				} else {
					child.name = "Plane";
				}
			}
		}
	}
	void madeFreePlanes(Transform x){
		foreach (Transform child in x) {
			if (child.name == "Join") {
				GameObject g = Setup.figureAtPlane(child);
				if (g != null) {
					foreach (Transform child2 in g.transform) {
						if (child2.name == "Join") {
							if (Vector3.Distance (child.position, child2.position) < 0.02f) {
								child2.name = "Plane";
							}
						}
					}
				}
			}
		}
	}
	Boolean justRedFigures (Transform x) {
		if (Color.Equals(red.color,x.GetComponent<MeshRenderer>().material.color)){
			return true;
		}
		redPokras (x);
		foreach (Transform child in x) {
			if (child.name == "Join") {
				GameObject g = Setup.figureAtPlane(child);
				justRedFigures (g.transform);
			}
		}
		return true;
	}
	void redPokras(Transform x)
	{
		if(x.parent.name=="Figures"){
			ColorArr.Add (x.gameObject.GetInstanceID(),x.GetComponent<MeshRenderer>().material.color);
		}
		int l = x.GetComponent<MeshRenderer>().materials.Length;
		Material[] xMat = new Material[l];
		for (int i = 0; i < l; i++)
		{
			xMat[i] = red;
		}
		x.GetComponent<MeshRenderer>().materials = xMat;
		foreach (Transform child in x) {
			if (child.tag != "Plane") {
				redPokras (child);
			}

		}
	}
	void antiRedPokras(Transform x)
	{
		Color col=ColorArr[x.gameObject.GetInstanceID()];
		ColorArr.Remove (x.gameObject.GetInstanceID());
		Transform t = Setup.Figures [Setup.NameToInt (x.name)].GO.transform;
		x.GetComponent<MeshRenderer> ().sharedMaterials = t.GetComponent<MeshRenderer> ().sharedMaterials;
		x.GetComponent<MeshRenderer> ().material.color = col;
		foreach (Transform child in x) {
			if (child.tag != "Plane") {
				child.GetComponent<MeshRenderer> ().sharedMaterials = 
					t.Find (child.name).GetComponent<MeshRenderer> ().sharedMaterials;
				child.GetComponent<MeshRenderer> ().material.color = col;
				Transform t2 = t.Find (child.name);
				foreach (Transform child2 in child) {
					if (child2.tag != "Plane") {
						child2.GetComponent<MeshRenderer> ().sharedMaterials = 
							t2.Find (child2.name).GetComponent<MeshRenderer> ().sharedMaterials;
						child2.GetComponent<MeshRenderer> ().material.color = col;
					}
				}
			}

		}
	}
	void testing(){
		foreach(RaycastHit child in hits){
			print (child.transform.name);
		}
		if (hits.Length > 0) {
			int index = -1;
			if (HaveFigureRay ()) {
				index = IndexFigurePlanRay ();
				if (index >= 0) {
					print (hits [index + 1].transform.name + " "+ getMeshIndex(hits [index + 1]));
				} else {
					index = IndexFigure ();
					print (hits [index].transform.name+ " "+ getMeshIndex(hits [index]));
				}
			}
		}
	}
	int getMeshIndex(RaycastHit r)
	{
		int[] subMeshesFaceTotals;
		int totalSubMeshes;
		MeshFilter mf = (MeshFilter)r.transform.gameObject.GetComponent(typeof(MeshFilter));
		Mesh mesh = mf.mesh;

		totalSubMeshes = mesh.subMeshCount;
		subMeshesFaceTotals= new int[totalSubMeshes];

		for(int i=0; i<totalSubMeshes; i++)  
		{
			subMeshesFaceTotals[i] = mesh.GetTriangles(i).Length /3;
		}
		int hitSubMeshNumber = 0;
		int maxVal= 0;

		for(int i=0; i<totalSubMeshes; i++)  
		{
			maxVal += subMeshesFaceTotals[i];

			if (r.triangleIndex <= maxVal-1 )
			{      
				hitSubMeshNumber = i+1;
				break;
			}

		}

		Debug.Log("We hit sub mesh number: " + hitSubMeshNumber);
		return hitSubMeshNumber;
	}
}