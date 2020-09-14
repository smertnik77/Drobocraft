using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class Test2 : MonoBehaviour {
	public Texture2D pricel;
	public AudioClip Beam;
	GameObject[] Rotors=new GameObject[0];
	List<GameObject> Rot;
	List<SuperUnits> SU;
	GameObject[] Lasers=new GameObject[0];
	GameObject[] Units=new GameObject[0];
	GameObject[] Wheels;
	GameObject[] WheelColliders;
	RaycastHit[] hits;
	RaycastHit hit;
	GameObject robot2;
	int TowerRotateSpeed=150;
	int GunRotateSpeed=150;
	int MinGunAngle = -90;
	int MaxGunAngle = 40;
	int LaserIndex=0;
	int CountI=0;
	Dictionary<int,List<GameObject>> r1,r2;
	Camera cam;
	// Use this for initialization
	void Start () {
		cam = transform.GetComponent<Camera> ();
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Setup.Start();
		Setup.Pause = false;
		GameObject myrobot=CreateRobot ("MyRobot");
		r1 = GetInfo (myrobot);
		CreateUnits (myrobot);
		CreateLasers (myrobot);
		CreateEmptyRotors (myrobot);

		robot2=CreateRobot ("Robot2", -15, -15);
		r2 = GetInfo (robot2);
		//CreateRotors (robot2);

		DestroyPlanes (robot2);

		myrobot.AddComponent<PlayerOrbitCamera> ();
		DestroyPlanes (myrobot);
		CreateLasers (myrobot);
		/*
		myrobot.AddComponent<RobotTrigger> ();
		CreateLasers (myrobot);
		CreateRotors (myrobot);
		DestroyPlanes (myrobot);
		CreateWheels (myrobot);
		*/
		//print ("Lasers=" + Lasers.Length+" Rotors="+Rotors.Length+" Wheels="+Wheels.Length);
		//print (Application.persistentDataPath);
		//foreach (KeyValuePair<int,List<GameObject>> x in r2) {
		//	print (x.Key+" "+x.Value.Count);
		//}
	}
	public void OnGUI ()
	{
		if (!Setup.Pause) {
			GUI.DrawTexture (new Rect (Screen.width / 2 - 5, Screen.height / 2 - 5, 10, 10), pricel);
		}
	}
	// Update is called once per frame
	void FixedUpdate () {
		if(Input.GetKey(KeyCode.Escape)){
			var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Unit");
			foreach (GameObject child in objects) {
				child.GetComponent<Rigidbody> ().isKinematic = Setup.Pause;
			}
			print ("esc");
		}
		if (Setup.Pause) {
			return;
		}
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

		if(SU.Count>0)
			foreach (SuperUnits dd in SU) {
				Transform Tower = dd.Rotor.transform.Find ("Tow");
				Vector3 p = hit.point;
				Vector3 LMP = LasersMeanPosition3 (dd.Rotor);
				if (LMP == Vector3.zero)
						LMP = Tower.position;
				Vector3 target = p - LMP;//Tower.position;
				target = Quaternion.Inverse (Tower.parent.rotation) * target;
				target.y = 0;
				Quaternion q = Quaternion.LookRotation (target);
				//Tower.localRotation = Quaternion.RotateTowards (Tower.localRotation, q, TowerRotateSpeed * Time.deltaTime);
				Tower.localRotation = q;
				Rigidbody z = dd.Unit.GetComponent<Rigidbody> ();
				q = Quaternion.Slerp (dd.Unit.transform.rotation, Tower.rotation, 10f* Time.deltaTime);
				z.MoveRotation (q);
				z.angularVelocity=Vector3.zero;	
			}
		if(Lasers.Length>0)
		foreach (GameObject dd in Lasers) {
			if (dd.name == "Laser1") {
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
			if (Lasers.Length == 0)
				return;
			if (GameObject.Find ("Tm") != null)
				return;
			if (LaserIndex == Lasers.Length)
				LaserIndex = 0;
			Transform dd=Lasers[LaserIndex++].transform;
			if (dd.name == "Laser1") {
				LaserBeam (dd.transform.Find("LaserTower/Gun"));
			}
			if (dd.name == "Laser1-static") {
				LaserBeam (dd.transform);
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
	}
	GameObject CreateRobot(string name,float x=0f,float z=0f)
	{
		GameObject robot=new GameObject(name);
		robot.transform.position = new Vector3 (x, 10f, z);
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
			Vector3 pos = new Vector3(float.Parse(masParam[0])+x, float.Parse(masParam[1]), float.Parse(masParam[2])+z);
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
			Setup.AddFigureWithPlanes( id, pos, rot, color, robot.transform ,"Figure");
		}
		PivotToCenter (robot);
		return robot;
	}

	public void PivotTo(GameObject x,Vector3 position)
	{
		Vector3 offset = x.transform.position - position;
		foreach (Transform child in x.transform)
			child.transform.position += offset;
		x.transform.position = position;
	}
	void PivotToCenter(GameObject x)
	{
		int i = 0;
		Vector3 center = Vector3.zero;
		foreach (Transform child in x.transform) {
			center += child.position;
			i++;
		}
		center /= i;
		PivotTo (x, center);
	}
	void CreateEmptyRotors(GameObject x)
	{
		Rot = new List<GameObject> ();
		SU = new List<SuperUnits> ();
		int CountI = 0;
		foreach (GameObject child in Units) {
			Rigidbody rb = child.GetComponent<HingeJoint> ().connectedBody;
			if (rb.name == "Body") {
				GameObject s = new GameObject ("Rot");

				s.transform.parent = rb.transform;
				s.transform.position = child.transform.position;
				s.transform.rotation = child.transform.rotation;

				GameObject t = new GameObject ("Tow");
				t.transform.parent = s.transform;
				t.transform.position = child.transform.position;
				t.transform.rotation = child.transform.rotation;

				findUnitTest (child,t);
				Rot.Add(s);
				SU.Add (new SuperUnits(child,s));
			}
		}
	}
	void findUnitTest (GameObject Unit,GameObject ss)
	{
		foreach (GameObject child in Units) {
			Rigidbody rb = child.GetComponent<HingeJoint> ().connectedBody;
			if (rb == Unit.GetComponent<Rigidbody>()) {
				GameObject s = new GameObject ("Rot");
				s.transform.parent = ss.transform;
				s.transform.position = child.transform.position;
				s.transform.rotation = child.transform.rotation;

				GameObject t = new GameObject ("Tow");
				t.transform.parent = s.transform;
				t.transform.position = child.transform.position;
				t.transform.rotation = child.transform.rotation;

				foreach (Transform l in child.transform) {
					if (l.name == "Laser1-static") {
						GameObject las = new GameObject ("Las");
						las.transform.parent = t.transform;
						las.transform.position = l.position;
					}

				}
				findUnitTest (child,t);
				Rot.Add(s);
				SU.Add (new SuperUnits(child,s));
			}
		}
	}
	void CreateUnits(GameObject x)
	{
		List<GameObject> ListRotors = FindRecurse (x, "Rotor1");
		Rotors = new GameObject[ListRotors.Count];
		Rotors = ListRotors.ToArray ();
		foreach (GameObject rotor in Rotors) {
			Vector3 fn = Vector3.zero;
			if (rotor.transform.GetChild (2).name == "Join") {
				GameObject g = Setup.figureAtPlane (rotor.transform.GetChild (2));
				fn = figureNumber (g.transform);
			}
			print ("cubes=" + fn.x+" rotors=" + fn.y+" stators=" + fn.z);
			if ((fn.x > 0) && (fn.y >= 1)) {
				
				Transform Tower=rotor.transform.Find ("Tower");
				GameObject Unit = new GameObject("Unit");
				Unit.transform.position =  Tower.position;
				Unit.AddComponent<HingeJoint>();
				Unit.AddComponent<Triger> ();
				HingeJoint cj = Unit.GetComponent<HingeJoint> ();
				cj.enableCollision = true;
				cj.axis = Vector3.up;
				cj.anchor = Vector3.zero;
				Unit.transform.rotation = Tower.rotation;
				//cj.anchor = Vector3.zero;
				//Unit.transform.position =  Tower.position;
				Tower.parent=Unit.transform;
				Unit.transform.parent = x.transform;
				Unit.GetComponent<Rigidbody> ().angularDrag = 0f;
				Unit.GetComponent<Rigidbody> ().useGravity = false;
				//print ("Unit.transform.position="+Unit.transform.position+"Tower.position"+Tower.position);

				foreach(GameObject child in GameObject.FindGameObjectsWithTag("checkFigure")){
					child.transform.parent = Unit.transform;
				}
			}
			foreach(GameObject child in GameObject.FindGameObjectsWithTag("checkFigure")){
				child.tag = "Figure";
			}

		}

		List<GameObject> figs = new List<GameObject> ();
		foreach (Transform child in x.transform) {
			if (child.name != "Unit") {
				figs.Add (child.gameObject);
			}
		}
		
		GameObject BodyUnit = new GameObject("Body");
		BodyUnit.transform.parent = x.transform;
		foreach (GameObject child in figs) {
			child.transform.parent = BodyUnit.transform;
		}
		PivotToCenter(BodyUnit);

		foreach(Transform child in x.transform){
			if (child.name == "Unit") {
				//child.gameObject.AddComponent<ConfigurableJoint>();
				//print (">>>>>>>>unit position "+child.position);
			} else {
				child.gameObject.AddComponent<Rigidbody> ();
				child.gameObject.GetComponent<Rigidbody> ().mass = 1500f;
			}
		}

		int i = 0;
		foreach (Transform child in x.transform) {
			if (child.name == "Unit") {
				foreach (Transform child2 in x.transform) {
					if (child != child2) {
						foreach (Transform figure in child2) {
							if(figure.name == "Rotor1"){
								//print ("+Rotor1 pos="+figure.position+" unit pos="+child.position);
								if (figure.position == child.position) {
									//print ("+");
									HingeJoint cj = child.GetComponent<HingeJoint> ();
									//ConfigurableJoint cj = child.GetComponent<ConfigurableJoint> ();
									cj.connectedBody = child2.GetComponent<Rigidbody> ();
									//cj.enableCollision = true;
									//cj.axis = figure.TransformDirection (Vector3.up);
									//cj.anchor = Vector3.zero;
									//cj.secondaryAxis = figure.TransformDirection(Vector3.up);
									//child.GetComponent<Rigidbody> ().centerOfMass = Vector3.zero;
									/*
									cj.xMotion = ConfigurableJointMotion.Locked;
									cj.yMotion = ConfigurableJointMotion.Locked;
									cj.zMotion = ConfigurableJointMotion.Locked;
									cj.angularYMotion = ConfigurableJointMotion.Locked;
									cj.angularZMotion = ConfigurableJointMotion.Locked;
									*/
									//child.transform.rotation = figure.transform.rotation;
		
								}
							}
						}
					}
				}
				i++;
			}
		}


		Units = new GameObject[i];
		i = 0;
		foreach (Transform child in x.transform) {
			if (child.name == "Unit") {
				Units [i] = child.gameObject;
				i++;
			}
		}

	}

	void CreateRotors(GameObject x)
	{
		List<GameObject> ListRotors = FindRecurse (x, "Rotor1");
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
		ListRotors = FindRecurse (x, "Rotor1");
		Rotors = new GameObject[ListRotors.Count];
		Rotors = ListRotors.ToArray ();
		foreach (GameObject dd in Rotors) {
			Vector3 fn = Vector3.zero;
			if (dd.transform.GetChild (2).name == "Join") {
				GameObject g = Setup.figureAtPlane (dd.transform.GetChild (2));
				fn = figureNumber (g.transform);
			}
			print ("cubes=" + fn.x+" rotors=" + fn.y+" stators=" + fn.z);
			if ((fn.x > 0) && (fn.y >= 1)) {
				Transform Tower=dd.transform.Find ("Tower");

				dd.tag="Figure";
				foreach(GameObject child in GameObject.FindGameObjectsWithTag("checkFigure")){
					child.transform.parent = Tower;
					/*
					copy = new gameobject();
					copy = original;
					copy. renderer. enabled = false;

					*/
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
			if (x.name == "Rotor1")
				count.y = 1;
			if (x.name != "Rotor1")
				count.x = 1;
			x.tag = "checkFigure";
		}
		foreach (Transform child in x) {
			//print ("-> "+child.name);
			if (child.name == "Join") {

				GameObject g = Setup.figureAtPlane(child);
				//print ("_ "+g.name);
				if ((g.name == "Rotor1")&& (g.tag != "checkFigure")) {


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
				if ((g.name!="Rotor1") && (g != null) && (g.tag != "checkFigure"))
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
		foreach (Transform Un in x.transform) {
			foreach(Transform child2 in Un){
				if (child2.tag == "Figure") {
					foreach (Transform child in child2) {
						if (child.tag == "Plane")
							Destroy (child.gameObject);
					}
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
				if (i.name == "Rotor1") {
					GameObject ii = i.Find ("Tower").gameObject;
					myList.AddRange (FindRecurse (ii,childName));
				}
			}
		}
		return myList;
	}

	List<GameObject> FindRecurseLas(GameObject x){
		List<GameObject>  myList = new List<GameObject>();
		foreach (Transform i in x.transform) {
			if (i.name == "Las") {
				myList.Add (i.gameObject);
			}
			if (i.name == "Rot") {
				GameObject ii = i.Find ("Tow").gameObject;
				myList.AddRange (FindRecurseLas (ii));
			}
		}
		return myList;
	}


	void CreateLasers(GameObject x)
	{
		int i = 0;
		foreach (Transform Un in x.transform) 
		foreach (Transform fig in Un) {
			if ((fig.name == "Laser1")||(fig.name=="Laser1-static")) {
				i++;
			}
		}
		Lasers=new GameObject[i];
		i=0;
		foreach (Transform Un in x.transform) 
			foreach (Transform fig in Un) {
				if (fig.name == "Laser1"||(fig.name=="Laser1-static")) {
					Lasers[i++]=fig.gameObject;
				}
			}
	}
	void LaserBeam (Transform ee)
	{
		Vector3 rr = ee.TransformDirection(Vector3.forward);
		Ray ray = new Ray (ee.position, rr);
		RaycastHit nearHit;
		if (Physics.Raycast(ray, out nearHit)) {
			if(nearHit.transform.parent!=null)
			if (nearHit.transform.parent.name == "MyRobot") {
				print ("qq");
				return;
			}
			if ((nearHit.collider.tag == "Figure")) {
				Transform j = nearHit.collider.transform;
				while(j.parent.tag=="Figure")j=j.parent;
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

	Vector3 LasersMeanPosition2(GameObject unit){
		Vector3 pos = Vector3.zero;
		int i = 0;
		foreach (GameObject child in Lasers) {
			if (ChildIsInUnit (child, unit)) {
				pos += child.transform.position;
				i++;
			}
		}
		pos /= i;
		return pos;
	}
	Vector3 LasersMeanPosition3(GameObject Rot){
		Vector3 pos = Vector3.zero;
		List<GameObject> list=FindRecurseLas(Rot.transform.Find("Tow").gameObject);
		int i = 0;
		foreach (GameObject child in list) {
			pos += child.transform.position;
			i++;
		}
		pos /= i;
		return pos;
	}
	Boolean ChildIsInUnit (GameObject child,GameObject Unit)
	{
		Transform currentParent = child.transform.parent;
		while ((currentParent != null)&&(currentParent.name=="Unit")) {
			if (currentParent.gameObject.GetInstanceID () == Unit.GetInstanceID ()) {
				return true;
			}
			currentParent = currentParent.GetComponent<HingeJoint> ().connectedBody.transform;
		}
		return false;
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
		foreach (SuperUnits x in SU) {
			x.Unit.transform.rotation = x.Rotor.transform.rotation;
			x.Unit.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
			print("Unit.rot="+x.Unit.transform.rotation+" rotor.rot="+x.Rotor.transform.rotation);

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
public class SuperUnits
{
	public GameObject Unit;
	public GameObject Rotor;
	public SuperUnits(GameObject unit,GameObject rotor){
		Unit = unit;
		Rotor = rotor;
	}
}
