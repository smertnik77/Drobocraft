using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UI;
using static RuntimePreviewGenerator;
public class Test : MonoBehaviour {
	public Texture2D pricel;
	public AudioClip Beam;
	GameObject[] Rotors;
	GameObject[] Lasers;
	GameObject[] Wheels;
	GameObject[] WheelColliders;
	GameObject[][] Weapons;
	RaycastHit[] hits;
	RaycastHit hit;
	GameObject robot2;
	int TowerRotateSpeed=150;
	int GunRotateSpeed=150;
	int MinGunAngle = -90;
	int MaxGunAngle = 40;
	int LaserIndex=0;
	int[] WeaponHitIndex = new int[3];
	Dictionary<int,List<GameObject>> r1,r2;
	Camera cam;
	int WeaponIndex = -1;
	// Use this for initialization
	void Start () {
		cam = transform.GetComponent<Camera> ();
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Setup.Start();
		Setup.Pause = false;
		GameObject myrobot=CreateRobot ("MyRobot");
		r1 = GetInfo (myrobot);
		CreateWeaponPanel ();
		CreateWeapons (myrobot);

		robot2=CreateRobot ("Robot2", -5, -5);
		r2 = GetInfo (robot2);
		CreateRotors (robot2);

		DestroyPlanes (robot2);
		myrobot.AddComponent<PlayerOrbitCamera> ();
		myrobot.AddComponent<RobotTrigger> ();
		CreateLasers (myrobot);
		CreateRotors (myrobot);
		DestroyPlanes (myrobot);
		CreateWheels (myrobot);
		print ("Lasers=" + Lasers.Length+" Rotors="+Rotors.Length+" Wheels="+Wheels.Length);
		//print (Application.persistentDataPath);
		//foreach (KeyValuePair<int,List<GameObject>> x in r2) {
		//	print (x.Key+" "+x.Value.Count);
		//}
	}
	void CreateWeaponPanel()
	{
		if (Setup.LA [0] == 0) {
			string L = PlayerPrefs.GetString(string.Format("LA{0}", Setup.IdActiveRobot));
			if (!PlayerPrefs.HasKey(string.Format("LA{0}", Setup.IdActiveRobot)))L="-1,-1,-1";
			Setup.LA = L.Split(',').Select(Int32.Parse).ToArray();
		}

		if (Setup.LA [0] > -1)
			WeaponIndex = 0;
		else if(Setup.LA [1] > -1)
			WeaponIndex = 1;
		else if(Setup.LA [2] > -1)
			WeaponIndex = 2;


		GameObject i = GameObject.Find ("WeaponPanel").transform.Find ("Item").gameObject;
		RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.7f, -0.4f, -1f);
		RuntimePreviewGenerator.Padding = 0.1f;
		RuntimePreviewGenerator.OrthographicMode = true;
		for (int x = 0; x < 3; x++) {
			if(Setup.LA[x]>-1) {
				GameObject item=Instantiate (i,GameObject.Find ("WeaponPanel").transform);
				item.name="Item"+x.ToString();
				item.transform.Find("Weapon/Text").gameObject.GetComponent<Text>().text=(x+1).ToString();
				RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
				Texture prev = RuntimePreviewGenerator.GenerateModelPreview(Setup.Figures[Setup.LA[x]].GO.transform, 100, 100, false) as Texture;
				Image Im = item.transform.Find ("Weapon").gameObject.GetComponent<Image> ();
				Im.sprite=Sprite.Create((Texture2D) prev ,new Rect(0,0,100,100),new Vector2(0.5f,0.5f));
				Im.color = new Color32 (255, 255, 255, 255);
			}
		}
		Destroy (i);
		fff();
	}
	public void OnGUI ()
	{
		if (!Setup.Pause) {
			GUI.DrawTexture (new Rect (Screen.width / 2 - 5, Screen.height / 2 - 5, 10, 10), pricel);
		}
	}
	// Update is called once per frame
	void FixedUpdate () {
		if (Setup.Pause)
			return;
		if(Input.GetKey(KeyCode.Mouse1)) {
			#if UNITY_EDITOR
			Setup.Figure1.color=new Color32(58, 90, 186, 255);
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
		hits = Physics.RaycastAll (cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))).OrderBy(h=>h.distance).ToArray();
		foreach (RaycastHit i in hits) {
			if (i.transform.name == "MyRobot")
				continue;
			hit = i;
			break;
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			testing ();
		}
		foreach (GameObject dd in Rotors) {
			Transform Tower = dd.transform.Find ("Tower");
			Vector3 p = hit.point;
			Vector3 LMP = LasersMeanPosition (dd);
			if (LMP == Vector3.zero)
				LMP = Tower.position;
			Vector3 target = p - LMP;//Tower.position;
			target = Quaternion.Inverse (Tower.parent.rotation) * target;
			target.y = 0;
			Quaternion q = Quaternion.LookRotation (target);
			Tower.localRotation = Quaternion.RotateTowards (Tower.localRotation, q, TowerRotateSpeed * Time.deltaTime);
		}
		if (Setup.IntToName (Setup.LA [WeaponIndex]) == "T0 Laser") {
			foreach (GameObject dd in Weapons[WeaponIndex]) {
				Transform Tower = dd.transform.Find ("LaserTower");
				Transform Gun = Tower.Find ("Gun");
				Vector3 p = hit.point;
				Vector3 target = p - Tower.position;
				target = Quaternion.Inverse (Tower.parent.rotation) * target;
				target.y = 0;
				Quaternion q = Quaternion.LookRotation (target);
				Tower.localRotation = Quaternion.RotateTowards (Tower.localRotation, q, TowerRotateSpeed * Time.deltaTime);
				target = Gun.position - p;
				Vector3 sss = Gun.position - p;
				sss = Quaternion.Inverse (Tower.parent.rotation) * sss;
				target = Quaternion.Inverse (Tower.parent.rotation) * target;
				target.y = 0;
				float angle = Mathf.Atan2 (sss.y, target.magnitude) * Mathf.Rad2Deg;
				if (angle < MinGunAngle)
					angle = MinGunAngle;
				if (angle > MaxGunAngle)
					angle = MaxGunAngle;
				q = Quaternion.Euler (angle, 0, 0);
				Gun.localRotation = Quaternion.RotateTowards (Gun.localRotation, q, GunRotateSpeed * Time.deltaTime);
			}
		}

		if (Input.GetKey(KeyCode.Mouse0)) {
			if (Weapons[WeaponIndex].Length > 0) {
				if (GameObject.Find ("Tm") == null)
				{
					if (WeaponHitIndex[WeaponIndex] == Weapons[WeaponIndex].Length)
						WeaponHitIndex[WeaponIndex] = 0;
					Transform Item = Weapons[WeaponIndex][WeaponHitIndex[WeaponIndex]++].transform;
					if (Item.name == "T0 Laser") {
						LaserBeam (Item.transform.Find ("LaserTower/Gun"));
					}
					if (Item.name == "T0 Laser Static") {
						LaserBeam (Item.transform);
					}
				}
			}
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			int i = 0;
			if (r2.Count == 0)
				print ("массив пуст");
			foreach (KeyValuePair<int,List<GameObject>> x in r2) {
				string s = "";
				foreach (GameObject child in x.Value) {
					s += child + ", ";
				}
				print (i+") "+x.Key+" "+x.Value.Count+" : "+s);
				i++;
			}
		}
		if ((Setup.LA[0]>-1)&&Input.GetKeyDown (KeyCode.Alpha1)) {
			WeaponIndex = 0;
			fff();
		}
		if ((Setup.LA[1]>-1)&&Input.GetKeyDown (KeyCode.Alpha2)) {
			WeaponIndex = 1;
			fff();
		}
		if ((Setup.LA[2]>-1)&&Input.GetKeyDown (KeyCode.Alpha3)) {
			WeaponIndex = 2;
			fff();
		}
	}
	void fff()
	{
		for (int x = 0; x < 3; x++) {
			if(Setup.LA[x]>-1) {
				GameObject item=GameObject.Find ("WeaponPanel");
				Image Im = item.transform.Find ("Item"+x.ToString()+"/Weapon").gameObject.GetComponent<Image> ();
				Im.color = new Color32 (255, 255, 255, 100);
				if(x==WeaponIndex)Im.color = new Color32 (255, 255, 255, 255);
			}
		}
	}
	GameObject CreateRobot(string name,float x=15f,float z=15f)
	{
		GameObject robot=new GameObject(name);
		robot.transform.position = new Vector3 (15f, 10f, 15f);
		robot.tag = "Robot";
		//если нету записи, то выходим
		if (!PlayerPrefs.HasKey(string.Format("robot{0}", Setup.IdActiveRobot)))
			return null;
		//получаем строку, содержащую все блоки
		string strBlocks = PlayerPrefs.GetString(string.Format("robot{0}", Setup.IdActiveRobot));
		//получаем массив блоков
		string[] masBlocks = strBlocks.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
		//перебираем каждый блок
		foreach (string s in masBlocks)
		{
			//Массив параметров блока
			string[] masParam = s.Split(new char[] { '#' }, System.StringSplitOptions.RemoveEmptyEntries);
			//Позиция
			Vector3 pos = new Vector3(float.Parse(masParam[0]), float.Parse(masParam[1]), float.Parse(masParam[2]));
			//цвет
			Color color = Setup.IntToColor(int.Parse(masParam[3]));
			//номер
			int id = int.Parse(masParam[4]);
			//наклон
			Vector3 rot;
			rot.x = float.Parse(masParam[5]);
			rot.y = float.Parse(masParam[6]);
			rot.z = float.Parse(masParam[7]);
			//Добавляем блок на сцену
			GameObject zzz=Setup.AddFigureWithPlanes( id, pos, rot, color, robot.transform ,"Figure");
		}
		robot.transform.position = new Vector3 (x, 10f, z);
		robot.AddComponent<Rigidbody> ();
		robot.GetComponent<Rigidbody> ().mass=1500f;
		robot.GetComponent<Rigidbody> ().angularDrag = 1;
		int i = 0;
		Vector3 center = Vector3.zero;
		foreach (Transform child in robot.transform) {
			center += child.position;
			i++;
		}
		center /= i;
		//center = center;// + 10*Vector3.up;
		PivotTo (robot, center);
		return robot;
	}

	public void PivotTo(GameObject x,Vector3 position)
	{
		Vector3 offset = x.transform.position - position;
		foreach (Transform child in x.transform)
			child.transform.position += offset;
		x.transform.position = position;
	}

	void CreateRotors(GameObject x)
	{
		List<GameObject> ListRotors = FindRecurse (x, "T0 Rotor");
		foreach (GameObject g in ListRotors) {
			if ((g.transform.GetChild (1).name == "Plane") || (g.transform.GetChild (2).name == "Plane")) {
				g.name = "Rotor1-Off";
				//тут помечаем красным стрелки ротора
			}
			if ((g.transform.GetChild (1).name == "Join") && (g.transform.GetChild (2).name == "Join")) {
				//тут помечаем зелёным стрелки ротора
				Material w=Resources.Load("Materials/RotorOn", typeof(Material)) as Material;
				Material[] m=new Material[4];
				m = g.GetComponent<MeshRenderer> ().materials;
				m [3] = w;
				g.GetComponent<MeshRenderer> ().materials  = m;
				g.transform.Find("Tower").GetComponent<MeshRenderer>().materials=m;
			}
		}
		ListRotors = FindRecurse (x, "T0 Rotor");
		Rotors = new GameObject[ListRotors.Count];
		Rotors = ListRotors.ToArray ();
		foreach (GameObject dd in Rotors) {
			Vector3 fn = Vector3.zero;
			if (dd.transform.GetChild (2).name == "Join") {
				GameObject g = Setup.figureAtPlane (dd.transform.GetChild (2));
				fn = figureNumber (g.transform);
			}
			print ("cubes=" + fn.x+" rotors=" + fn.y+" stators=" + fn.z);
			if ((fn.x > 0) && (fn.y == 1)) {
				Transform Tower=dd.transform.Find ("Tower");
				dd.tag="Figure";
				foreach(GameObject child in GameObject.FindGameObjectsWithTag("checkFigure")){
					child.transform.parent = Tower;
				}
			}
			if ((fn.x > 0) && (fn.y > 1)) {
				Transform Tower=dd.transform.Find ("Tower");
				dd.tag="Figure";
				foreach(GameObject child in GameObject.FindGameObjectsWithTag("checkFigure")){
					child.transform.parent = Tower;
				}
			}
			foreach(GameObject child in GameObject.FindGameObjectsWithTag("checkFigure")){
				child.tag = "Figure";
			}

		}
	}
	Vector3 figureNumber(Transform x)
	{
		Vector3 count = Vector3.zero;
		//print (">>>>> "+x.name);
		if(x.tag!="checkFigure"){
			if (x.name == "T0 Rotor")
				count.y = 1;
			if (x.name != "T0 Rotor")
				count.x = 1;
			x.tag = "checkFigure";
		}
		foreach (Transform child in x) {
			//print ("-> "+child.name);
			if (child.name == "Join") {

				GameObject g = Setup.figureAtPlane(child);
				//print ("_ "+g.name);
				if ((g.name == "T0 Rotor")&& (g.tag != "checkFigure")) {


					if (Vector3.Distance (child.position, g.transform.GetChild(2).position) < 0.02f) {
						//print ("rotor");
						count.y++;
						child.name = "Plane"; //СПОРНАЯ ХУЙНЯ

						//g.tag = "checkFigure";//СПОРНАЯ ХУЙНЯ 2
					}
					if (Vector3.Distance (child.position, g.transform.GetChild(1).position) < 0.02f) {
						//print ("stator");
						g.tag = "checkFigure";
						count.z++;
					}
				}
				//print (g.name);
				if ((g.name!="T0 Rotor") && (g != null) && (g.tag != "checkFigure"))
				{
					//print ("cube");
					g.tag = "checkFigure";
					count.x++;
					foreach (Transform child2 in g.transform)
					{
						if (child2.name == "Join")
						{
							if (Vector3.Distance(child.position, child2.position) < 0.02f)
							{
								count += figureNumber(g.transform);
							}
						}
					}
				}
			}
		}
		return count;
	}
	void DestroyPlanes(GameObject x){
		foreach (Transform dd in x.transform) {
			if (dd.tag == "Figure") {
				foreach (Transform child in dd) {
					if (child.tag == "Plane")
						Destroy (child.gameObject);
				}
				if (dd.name == "T0 Rotor") {
					DestroyPlanes (dd.Find ("Tower").gameObject);
				}
			}
		}
	}

	List<GameObject> FindRecurse(GameObject x , String childName){
		List<GameObject>  myList = new List<GameObject>();
		foreach (Transform i in x.transform) {
			if (i.tag == "Figure") {
				if (i.name == childName) {
					myList.Add (i.gameObject);
				}
				if (i.name == "T0 Rotor") {
					GameObject ii = i.Find ("Tower").gameObject;
					myList.AddRange (FindRecurse (ii,childName));
				}
			}
		}
		return myList;
	}
	void CreateWeapons(GameObject x)
	{
		Weapons = new GameObject[3][];
		for (int i = 0; i < 3; i++) {
			if (Setup.LA [i] > -1) {
				List<GameObject>  myList = new List<GameObject>();
				foreach (Transform dd in x.transform) {
					if (dd.name == Setup.Figures[Setup.LA [i]].Name) {
						myList.Add(dd.gameObject);
					}
				}
				Weapons [i] = myList.ToArray ();
			}
		}
	}
	void CreateLasers(GameObject x)
	{
		int i = 0;
		foreach (Transform dd in x.transform) {
			if ((dd.name == "T0 Laser")||(dd.name=="T0 Laser Static")) {
				i++;
			}
		}
		Lasers=new GameObject[i];
		i=0;
		foreach (Transform dd in x.transform) {
			if (dd.name == "T0 Laser"||(dd.name=="T0 Laser Static")) {
				Lasers[i++]=dd.gameObject;
			}
		}
	}
	void LaserBeam (Transform ee)
	{
		//if (GameObject.Find ("Tm") != null)
		//	return;
		Vector3 rr = ee.TransformDirection(Vector3.forward);
		Ray ray = new Ray (ee.position, rr);
		RaycastHit nearHit;
		if (Physics.Raycast(ray, out nearHit)) {
			if (nearHit.transform.name == "MyRobot") {
				return;
			}
			if ((nearHit.collider.tag == "Figure")) {
				Transform j = nearHit.collider.transform;
				while((j.parent.tag=="Figure")&&(j.parent.name!="Tower"))j=j.parent;
				//print (nearHit.collider.transform.root.transform.name);
				//j.GetComponent<MeshRenderer>().material.color*=0.5f;
				pizdec(j.gameObject);
				Destroy (j.gameObject);
			}
		}
		GameObject lr = new GameObject("LaserRay");
		GameObject Tm = new GameObject("Tm");
		lr.transform.parent = ee;
		Tm.transform.parent = ee;
		lr.transform.localPosition = new Vector3 (0,0,1.4f);
		lr.AddComponent<LaserLine> ();
		LaserLine ll = lr.GetComponent<LaserLine> ();
		ll.width = 0.7f;
		ll.innerFadeMaterial = Setup.innerFadeMaterial;
		ll.outerFadeMaterial = Setup.outerFadeMaterial;
		ll.SetPositions (new Vector3[] { lr.transform.position, nearHit.point });
		ll.Visible = true;
		Destroy(lr, 0.1f);
		Destroy(Tm, 0.2f);
		lr.AddComponent<AudioSource> ();
		AudioSource audio = lr.GetComponent<AudioSource> ();
		audio.clip = Beam;
		audio.Play ();
	}

	void CreateWheels(GameObject robot)
	{
		List<GameObject> WheelList = FindRecurse (robot, "Wheel1");
		if (WheelList.Count == 0) {
			Wheels = new GameObject [0];
			return;
		}
		foreach(GameObject ee in WheelList){
			ee.transform.Find("Arm").Translate (-0.35f, 0f, 0f);
		}
		Wheels = new GameObject[WheelList.Count];
		Wheels = WheelList.ToArray ();

		GameObject WC = new GameObject ("WheelColliders");
		WC.transform.parent=robot.transform;
		WC.transform.localPosition = Vector3.zero;
		WC.transform.localRotation = Quaternion.identity;

		WheelColliders = new GameObject[WheelList.Count];
		int i = 0;
		foreach (GameObject child in Wheels) {
			Transform V = child.transform.Find ("Arm");
			GameObject Z = new GameObject ("Wheel1");
			Z.transform.parent=WC.transform;
			Z.transform.position = V.position;
			Z.transform.rotation = V.rotation;
			Z.AddComponent<WheelCollider> ();
			Z.GetComponent<WheelCollider> ().radius = 1.1f;
			WheelColliders [i++] = Z;
		}
		transform.gameObject.AddComponent<CarContrller2> ().Robot=robot;
		transform.gameObject.GetComponent<CarContrller2> ().Wheels = Wheels;
		transform.gameObject.GetComponent<CarContrller2> ().WheelColliders = WheelColliders;
	}
	Vector3 LasersMeanPosition(GameObject rotor){
		Vector3 pos = Vector3.zero;
		int i = 0;
		foreach (GameObject child in Lasers) {
			if (ChildIsInParent (child, rotor)) {
				pos += child.transform.position;
				i++;
			}
		}
		pos /= i;
		return pos;
	}
	Boolean ChildIsInParent (GameObject child,GameObject eqParent)
	{
		Transform currentParent = child.transform.parent;
		while (currentParent != null) {
			if (currentParent.gameObject.GetInstanceID () == eqParent.GetInstanceID ()) {
				return true;
			}
			currentParent = currentParent.parent;
		}
		return false;
	}
	void testing(){
		foreach (GameObject dd in Rotors) {
			Transform Tower = dd.transform.Find ("Tower");
			Vector3 p = hit.point;
			Vector3 LMP = LasersMeanPosition (dd);
			if (LMP == Vector3.zero)
				LMP = Tower.position;
			Vector3 target = p - LMP;//Tower.position;
			target = Quaternion.Inverse (Tower.parent.rotation) * target;
			target.y = 0;
			Quaternion q = Quaternion.LookRotation (target);
			print("hit.point="+hit.point+" Unit.position="+Tower.position+" target="+target);
		}
	}
	int countLasers(GameObject rotor){

		List<GameObject> L = FindRecurse (rotor.transform.Find("Tower").gameObject, "Laser1-static");
		return L.Count;
	}

	Dictionary<int,List<GameObject>> GetInfo(GameObject x)
	{
		Dictionary<int,List<GameObject>> d= new Dictionary<int,List<GameObject>>();
		if (x.tag == "Robot") {
			foreach (Transform child in x.transform) {
				if (child.tag == "Figure") {
					List<GameObject> l = new List<GameObject> ();
					l = GetjoinInfo (child.gameObject);
					d.Add (child.gameObject.GetInstanceID (), l);
				}
			}
		}
		return d;
	}
	List<GameObject> GetjoinInfo(GameObject x)
	{
		List<GameObject> y = new List<GameObject> ();
		foreach (Transform child in x.transform) {
			if (child.name == "Join") {
				y.Add (Setup.figureAtPlane(child));
			}
		}
		return y;
	}
	void pizdec(GameObject x){
		List<GameObject> w=new List<GameObject>();
		if (r2.ContainsKey (x.GetInstanceID ())) {
			w = r2 [x.GetInstanceID ()];
		}
		Removing (r2, x);
		print ("w.count="+w.Count);
		if (w.Count < 1) {
			return;
		}
		List<GameObject>[] sList = new List<GameObject>[w.Count];
		List<GameObject>[] ob = new List<GameObject>[w.Count];
		int[] figs = new int[w.Count];
		int i = 0;
		foreach (GameObject s in w) {
			ob [i] = new List<GameObject> ();
			sList [i] = new List<GameObject>(zalupa (s,ob[i]));
			print (i+") " + sList [i].Count+" ob="+ob[i].Count);
			figs [i] = ob [i].Count;
			i++;
		}
		int maxValue = figs.Max();
		int maxIndex = figs.ToList().IndexOf(maxValue);
		print("maxValue ="+maxValue+" maxIndex ="+maxIndex);
		if(x.transform.parent.name=="Tower"){
			print ("in rotor");
			print (" r 1) " + x.transform.parent.parent.GetInstanceID ());
			if (!ob [maxIndex].Contains (x.transform.parent.parent.gameObject)) {
				foreach (GameObject child in ob [maxIndex]) {
					if (child.transform.parent == x.transform.parent) {
						child.transform.parent=robot2.transform;
					}
				}
			};


		}
		if (x.name == "Rotor1") {
			Transform Tower = x.transform.Find ("Tower");
			if (ob [maxIndex] [0].transform.parent.gameObject.GetInstanceID () == Tower.gameObject.GetInstanceID ()) {
				List<GameObject> box = new List<GameObject> ();
				foreach (Transform child in Tower) {
					box.Add (child.gameObject);
				}
				foreach (GameObject child in box) {
					child.transform.parent = robot2.transform;
				}
			}

		}
		for(i=0;i<figs.Length;i++) {
			if (i != maxIndex) {
				if (!ob [maxIndex].Contains(ob[i][0])) {
					foreach (GameObject g in ob[i]) {
						print ("remove "+g.name);
						Removing (r2, g);
						Destroy (g);
					}
				}
			}
		}
	}
	List<GameObject> zalupa(GameObject s,List<GameObject> ob){
		ob.Add (s);
		List<GameObject> cList=new List<GameObject>();
		if(r2.ContainsKey(s.GetInstanceID ()))
			cList = new List<GameObject>(r2[s.GetInstanceID ()]);
		foreach (GameObject child in cList) {
			if((!ob.Contains(child)))
			zalupa (child,ob);
		}
		ob = ob.Distinct().ToList();
		return ob;
	}
	void Removing (Dictionary<int,List<GameObject>> d, GameObject x)
	{
		if(d.ContainsKey(x.GetInstanceID ())){	
			List<GameObject> yy = d [x.GetInstanceID ()];
			foreach (GameObject child in yy) {
				List<GameObject> ee = d [child.GetInstanceID ()];
				ee.Remove (x);
			}
			d.Remove (x.GetInstanceID ());
		}
	}

}
