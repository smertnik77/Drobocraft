using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UI;
using static RuntimePreviewGenerator;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Collections.Specialized;
namespace Test
{
	public class Test : MonoBehaviour
	{
		public Texture2D pricel;
		public AudioClip Beam;
		public GameObject Effect;
		GameObject[] Rotors;
		GameObject[] Wheels;
		GameObject[] WheelColliders;
		GameObject[][] Weapons;
		RaycastHit[] hits;
		RaycastHit hit;
		GameObject myrobot;
		GameObject robot2;
		GameObject robot3;
		int TowerRotateSpeed = 150;
		int GunRotateSpeed = 150;
		int MinGunAngle = -90;
		int MaxGunAngle = 40;
		int[] WeaponHitIndex = new int[3];
		Dictionary<int, InfoFigure> Info;
		public static Dictionary<string, InfoRobot> Robo;
		Camera cam;
		int WeaponIndex = -1;
		List<int> Stack;
		Texture2D texture;
		GUIStyle style;
		GUIStyle shadow;
		Coroutine testCoroutine2;
		// Use this for initialization
		void Start()
		{
			texture = new Texture2D(1, 1);
			style = new GUIStyle();
			style.fontSize = 20;
			style.normal.textColor = Color.red;
			style.alignment = TextAnchor.MiddleCenter;
			shadow = new GUIStyle();
			shadow.fontSize = 20;
			shadow.normal.textColor = Color.black;
			shadow.alignment = TextAnchor.MiddleCenter;




			cam = transform.GetComponent<Camera>();
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			Setup.Start();
			Setup.Pause = false;

			Info = new Dictionary<int, InfoFigure>();
			Robo = new Dictionary<string, InfoRobot>();

			robot3 = CreateRobot("Robot 3", -15, -15);
			CreateInfo(robot3);
			CreateRotors(robot3);
			DestroyPlanes(robot3);

			robot2 = CreateRobot("Robot 2", -5, -5);
			CreateInfo(robot2);
			CreateRotors(robot2);
			DestroyPlanes(robot2);

			myrobot = CreateRobot("MyRobot");
			CreateInfo(myrobot);
			CreateWeaponPanel();
			CreateWeapons(myrobot);
			myrobot.AddComponent<PlayerOrbitCamera>();
			//myrobot.AddComponent<RobotTrigger>();
			Rotors = CreateRotors(myrobot);
			DestroyPlanes(myrobot);
			CreateWheels(myrobot);

			
			//print("Weapons=" + Weapons.Length + " Rotors=" + Rotors.Length + " Wheels=" + Wheels.Length);

		}
	
		void CreateWeaponPanel()
		{
			if (Setup.LA[0] == 0)
			{
				string L = PlayerPrefs.GetString(string.Format("LA{0}", Setup.IdActiveRobot));
				if (!PlayerPrefs.HasKey(string.Format("LA{0}", Setup.IdActiveRobot))) L = "-1,-1,-1";
				Setup.LA = L.Split(',').Select(Int32.Parse).ToArray();
			}

			if (Setup.LA[0] > -1)
				WeaponIndex = 0;
			else if (Setup.LA[1] > -1)
				WeaponIndex = 1;
			else if (Setup.LA[2] > -1)
				WeaponIndex = 2;


			GameObject i = GameObject.Find("WeaponPanel").transform.Find("Item").gameObject;
			RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.7f, -0.4f, -1f);
			RuntimePreviewGenerator.Padding = 0.1f;
			RuntimePreviewGenerator.OrthographicMode = true;
			for (int x = 0; x < 3; x++)
			{
				if (Setup.LA[x] > -1)
				{
					GameObject item = Instantiate(i, GameObject.Find("WeaponPanel").transform);
					item.name = "Item" + x.ToString();
					item.transform.Find("Weapon/Text").gameObject.GetComponent<Text>().text = (x + 1).ToString();
					RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
					Texture prev = RuntimePreviewGenerator.GenerateModelPreview(Setup.Figures[Setup.LA[x]].GO.transform, 100, 100, false) as Texture;
					Image Im = item.transform.Find("Weapon").gameObject.GetComponent<Image>();
					Im.sprite = Sprite.Create((Texture2D)prev, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
					Im.color = new Color32(255, 255, 255, 255);
				}
			}
			Destroy(i);
			WeaponPanelUpdate();
		}
		public void OnGUI()
		{
			if (Setup.Pause) return;
			GUI.DrawTexture(new Rect(Screen.width / 2 - 5, Screen.height / 2 - 5, 10, 10), pricel);
			
			Dictionary<string, float> Len = new Dictionary<string, float>();
			foreach(InfoRobot r in Robo.Values) 
			{
                if (r.Owner.name != myrobot.name) 
				{
					Vector3 viewPos = cam.WorldToViewportPoint(r.Owner.transform.position + new Vector3(0, 7, 0));
					if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
                    {
						Len.Add(r.Owner.name, Vector3.Distance(r.Owner.transform.position, myrobot.transform.position));
					}	
				}
			}
			foreach(KeyValuePair<string,float> l in Len.OrderByDescending(i => i.Value))
            {
				Vector3 pos = Robo[l.Key].Owner.transform.position + new Vector3(0, 7, 0);
				Vector2 Point = cam.WorldToScreenPoint(pos) + new Vector3(-60, 20, 0);
				ShowName(Point.x,Point.y, l.Key);
				ShowHealth(Point.x, Point.y, (int)((float)Robo[l.Key].Health / Robo[l.Key].DefoltHealth * 120));
			}
			//cam.Render();
			//GameObject.Find("Text300").transform.LookAt(cam.transform);
		}

		void ShowName(float x,float y,string s)
        {
			GUI.Label(new Rect(x - 1, Screen.height - y - 1, 120, 20), s, shadow);
			GUI.Label(new Rect(x + 1, Screen.height - y + 1, 120, 20), s, shadow);
			GUI.Label(new Rect(x - 1, Screen.height - y + 1, 120, 20), s, shadow);
			GUI.Label(new Rect(x + 1, Screen.height - y - 1, 120, 20), s, shadow);
			GUI.Label(new Rect(x, Screen.height - y, 120, 20), s, style);
		}
		void ShowHealth(float x,float y,int hp)
        {
			Rect pos = new Rect(x, Screen.height +24- y, hp, 6);
			Rect pos2 = new Rect(x-1, Screen.height + 24 - y-1, 120+2, 6+2);
			DrawQuad(pos2, Color.black);
			DrawQuad(pos, Color.red);
		}
		void DrawQuad(Rect position, Color color)
		{
			if (position.width == 0) return;
			texture.SetPixel(0, 0, color);
			texture.Apply();
			GUI.skin.box.normal.background = texture;
			GUI.Box(position, GUIContent.none);
		}
		// Update is called once per frame
		void Update()
		{
			if (Setup.Pause)
			{
				myrobot.GetComponent<Rigidbody>().Sleep();
				return;
			}
			if (Input.GetKey(KeyCode.Mouse1))
			{
				#if UNITY_EDITOR
				Setup.Figure1.color=new Color32(58, 90, 186, 255);
				UnityEditor.EditorApplication.isPlaying = false;
				#else
				Application.Quit();
				#endif
			}
			hits = Physics.RaycastAll(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))).OrderBy(h => h.distance).ToArray();
			foreach (RaycastHit i in hits)
			{
				if (i.transform.name == "MyRobot")
					continue;
				hit = i;
				break;
			}
			foreach (GameObject dd in Rotors)
			{
				Transform Tower = dd.transform.Find("Tower");
				Vector3 p = hit.point;
				Vector3 LMP = WeaponsMeanPosition(Tower.gameObject);
				if (LMP == Vector3.zero)
					LMP = Tower.position;
				Vector3 target = p - LMP;//Tower.position;
				target = Quaternion.Inverse(Tower.parent.rotation) * target;
				target.y = 0;
				Quaternion q = Quaternion.LookRotation(target);
				Tower.localRotation = Quaternion.RotateTowards(Tower.localRotation, q, TowerRotateSpeed * Time.deltaTime);
			}
			if (Setup.IntToName(Setup.LA[WeaponIndex]) == "T0 Laser")
			{
				foreach (GameObject dd in Weapons[WeaponIndex])
				{
					Transform Tower = dd.transform.Find("LaserTower");
					Transform Gun = Tower.Find("Gun");
					Vector3 p = hit.point;
					Vector3 target = p - Tower.position;
					target = Quaternion.Inverse(Tower.parent.rotation) * target;
					target.y = 0;

					Quaternion q = Quaternion.LookRotation(target);
					Tower.localRotation = Quaternion.RotateTowards(Tower.localRotation, q, TowerRotateSpeed * Time.deltaTime);
					target = Gun.position - p;
					Vector3 sss = Gun.position - p;
					sss = Quaternion.Inverse(Tower.parent.rotation) * sss;
					target = Quaternion.Inverse(Tower.parent.rotation) * target;
					target.y = 0;
					float angle = Mathf.Atan2(sss.y, target.magnitude) * Mathf.Rad2Deg;
					if (angle < MinGunAngle)
						angle = MinGunAngle;
					if (angle > MaxGunAngle)
						angle = MaxGunAngle;
					q = Quaternion.Euler(angle, 0, 0);
					Gun.localRotation = Quaternion.RotateTowards(Gun.localRotation, q, GunRotateSpeed * Time.deltaTime);
				}
			}
			
			if((Input.GetKey(KeyCode.F))&&(testCoroutine2 == null))
			{
				if (testCoroutine2 != null)
				{
					StopCoroutine(testCoroutine2);
					testCoroutine2 = null;
				}
				if (testCoroutine2 == null) testCoroutine2 = StartCoroutine(
					MoveCoroutine(robot3, 10)
				);
			}		
			if (Input.GetKey(KeyCode.Mouse0))
			{
				if (Weapons[WeaponIndex].Length > 0)
				{
					if (GameObject.Find("Tm") == null)
					{
						if (WeaponHitIndex[WeaponIndex] == Weapons[WeaponIndex].Length)
							WeaponHitIndex[WeaponIndex] = 0;
						Transform Item = Weapons[WeaponIndex][WeaponHitIndex[WeaponIndex]++].transform;
						if (Item.name == "T0 Laser")
						{
							LaserBeam(Item.transform.Find("LaserTower/Gun"));
						}
						if (Item.name == "T0 Laser Static")
						{
							LaserBeam(Item.transform);
						}
					}
				}
			}
			if ((Setup.LA[0] > -1) && Input.GetKeyDown(KeyCode.Alpha1))
			{
				WeaponIndex = 0;
				WeaponPanelUpdate();
			}
			if ((Setup.LA[1] > -1) && Input.GetKeyDown(KeyCode.Alpha2))
			{
				WeaponIndex = 1;
				WeaponPanelUpdate();
			}
			if ((Setup.LA[2] > -1) && Input.GetKeyDown(KeyCode.Alpha3))
			{
				WeaponIndex = 2;
				WeaponPanelUpdate();
			}
		}

		IEnumerator HealthTimer(float time,string name)
		{
			//print("хуй в жопе1");
			yield return new WaitForSeconds(time);
            //print("хуй в жопе2");
            //testCoroutine2 = null;
            if (time == 10)
            {
				
					if (testCoroutine2 == null) testCoroutine2 = StartCoroutine(
						MoveCoroutine(Robo[name].Owner, 10)
					);
				

			}
			if (Robo[name].Health<Robo[name].DefoltHealth)
			{
				Healing(name, 300);
				if (Robo[name].Health < Robo[name].DefoltHealth)
					Robo[name].HealthCoroutine = StartCoroutine(HealthTimer(0.1f, name));
			}
		}

		public IEnumerator MoveCoroutine(GameObject x, float time)
		{
			//x.GetComponent<Rigidbody>().useGravity = false;
		
			Quaternion wantedRotation = Quaternion.Euler(0, x.transform.eulerAngles.y, 0);
			for (float t = 0; t <= 1 * time; t += Time.deltaTime)
			{
				//x.transform.position = Vector3.Lerp(startPos, endPos, t / time);
				x.transform.rotation = Quaternion.RotateTowards(x.transform.rotation, wantedRotation, t / time);
				
				yield return null;
			}
			//x.transform.position = endPos;
			x.transform.rotation = wantedRotation;
			//x.GetComponent<Rigidbody>().useGravity = true;
			testCoroutine2 = null;
		}
		//Quaternion wantedRotation = Quaternion.Euler(0,0,90);
		//transform.rotation = Quaternion.RotateTowards(currentRotation, wantedRotation, Time.deltaTime* rotateSpeed);
		void WeaponPanelUpdate()
		{
			for (int x = 0; x < 3; x++)
			{
				if (Setup.LA[x] > -1)
				{
					GameObject item = GameObject.Find("WeaponPanel");
					Image Im = item.transform.Find("Item" + x.ToString() + "/Weapon").gameObject.GetComponent<Image>();
					Im.color = new Color32(255, 255, 255, 100);
					if (x == WeaponIndex) Im.color = new Color32(255, 255, 255, 255);
				}
			}
		}
		GameObject CreateRobot(string name, float x = 15f, float z = 15f)
		{
			GameObject robot = new GameObject(name);
			robot.transform.position = new Vector3(15f, 10f, 15f);
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
				GameObject zzz = Setup.AddFigureWithPlanes(id, pos, rot, color, robot.transform, "Figure");
			}
			robot.transform.position = new Vector3(x, 10f, z);
			robot.transform.rotation = Quaternion.Euler(0, -90, 0);////
			robot.AddComponent<Rigidbody>();
			robot.GetComponent<Rigidbody>().mass = 1500f;
			robot.GetComponent<Rigidbody>().angularDrag = 1;
			int i = 0;
			Vector3 center = Vector3.zero;
			foreach (Transform child in robot.transform)
			{
				center += child.position;
				i++;
			}
			center /= i;
			PivotTo(robot, center);
			//robot.AddComponent<MeshRenderer>();
			return robot;
		}

		public void PivotTo(GameObject x, Vector3 position)
		{
			Vector3 offset = x.transform.position - position;
			foreach (Transform child in x.transform)
				child.transform.position += offset;
			x.transform.position = position;
		}

		GameObject[] CreateRotors(GameObject x)
		{
			List<GameObject> ListRotors = FindRecurse(x, "T0 Rotor");
			foreach (GameObject g in ListRotors)
			{
				if ((g.transform.GetChild(1).name == "Plane") || (g.transform.GetChild(2).name == "Plane"))
				{
					g.name = "T0 Rotor Off";
					//тут помечаем красным стрелки ротора
					Material w = Resources.Load("Materials/RotorOff", typeof(Material)) as Material;
					Material[] m = new Material[4];
					m = g.GetComponent<MeshRenderer>().materials;
					m[3] = w;
					g.GetComponent<MeshRenderer>().materials = m;
					g.transform.Find("Tower").GetComponent<MeshRenderer>().materials = m;
				}
				if ((g.transform.GetChild(1).name == "Join") && (g.transform.GetChild(2).name == "Join"))
				{
					//тут помечаем зелёным стрелки ротора
					Material w = Resources.Load("Materials/RotorOn", typeof(Material)) as Material;
					Material[] m = new Material[4];
					m = g.GetComponent<MeshRenderer>().materials;
					m[3] = w;
					g.GetComponent<MeshRenderer>().materials = m;
					g.transform.Find("Tower").GetComponent<MeshRenderer>().materials = m;
				}
			}
			ListRotors = FindRecurse(x, "T0 Rotor");
			foreach (GameObject dd in ListRotors.ToArray())
			{
				Vector3 fn = Vector3.zero;
				if (dd.transform.GetChild(2).name == "Join")
				{
					GameObject g = Setup.figureAtPlane(dd.transform.GetChild(2));
					fn = figureNumber(g.transform);
				}
				//print("cubes=" + fn.x + " rotors=" + fn.y + " stators=" + fn.z);
				if ((fn.x > 0) && (fn.y == 1))
				{
					Transform Tower = dd.transform.Find("Tower");
					dd.tag = "Figure";
					foreach (GameObject child in GameObject.FindGameObjectsWithTag("checkFigure"))
					{
						child.transform.parent = Tower;
					}
				}
				if ((fn.x > 0) && (fn.y > 1))
				{
					Transform Tower = dd.transform.Find("Tower");
					dd.tag = "Figure";
					foreach (GameObject child in GameObject.FindGameObjectsWithTag("checkFigure"))
					{
						child.transform.parent = Tower;
					}
				}
				foreach (GameObject child in GameObject.FindGameObjectsWithTag("checkFigure"))
				{
					child.tag = "Figure";
				}

			}
			return ListRotors.ToArray();
		}
		Vector3 figureNumber(Transform x)
		{
			Vector3 count = Vector3.zero;
			//print (">>>>> "+x.name);
			if (x.tag != "checkFigure")
			{
				if (x.name == "T0 Rotor")
					count.y = 1;
				if (x.name != "T0 Rotor")
					count.x = 1;
				x.tag = "checkFigure";
			}
			foreach (Transform child in x)
			{
				//print ("-> "+child.name);
				if (child.name == "Join")
				{

					GameObject g = Setup.figureAtPlane(child);
					//print ("_ "+g.name);
					if ((g.name == "T0 Rotor") && (g.tag != "checkFigure"))
					{


						if (Vector3.Distance(child.position, g.transform.GetChild(2).position) < 0.02f)
						{
							//print ("rotor");
							count.y++;
							child.name = "Plane"; //СПОРНАЯ ХУЙНЯ

							//g.tag = "checkFigure";//СПОРНАЯ ХУЙНЯ 2
						}
						if (Vector3.Distance(child.position, g.transform.GetChild(1).position) < 0.02f)
						{
							//print ("stator");
							g.tag = "checkFigure";
							count.z++;
						}
					}
					//print (g.name);
					if ((g.name != "T0 Rotor") && (g != null) && (g.tag != "checkFigure"))
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
		void DestroyPlanes(GameObject x)
		{
			foreach (Transform dd in x.transform)
			{
				if (dd.tag == "Figure")
				{
					foreach (Transform child in dd)
					{
						if (child.tag == "Plane")
							Destroy(child.gameObject);
					}
					if (dd.name == "T0 Rotor")
					{
						DestroyPlanes(dd.Find("Tower").gameObject);
					}
				}
			}
		}

		List<GameObject> FindRecurse(GameObject x, String childName)
		{
			List<GameObject> myList = new List<GameObject>();
			foreach (Transform i in x.transform)
			{
				if (i.tag == "Figure")
				{
					if (i.name == childName)
					{
						myList.Add(i.gameObject);
					}
					if (i.name == "T0 Rotor")
					{
						GameObject ii = i.Find("Tower").gameObject;
						myList.AddRange(FindRecurse(ii, childName));
					}
				}
			}
			return myList;
		}
		void CreateWeapons(GameObject x)
		{
			Weapons = new GameObject[3][];
			for (int i = 0; i < 3; i++)
			{
				if (Setup.LA[i] > -1)
				{
					List<GameObject> myList = new List<GameObject>();
					foreach (Transform dd in x.transform)
					{
						if (dd.name == Setup.Figures[Setup.LA[i]].Name)
						{
							myList.Add(dd.gameObject);
						}
					}
					Weapons[i] = myList.ToArray();
				}
			}
		}
		void LaserBeam(Transform ee)
		{
			Vector3 rr = ee.TransformDirection(Vector3.forward);
			Ray ray = new Ray(ee.position, rr);
			RaycastHit nearHit;
			if (Physics.Raycast(ray, out nearHit))
			{
				string RootName = nearHit.transform.root.name;
				if (RootName == "MyRobot")
				{
					return;
				}
				Instantiate(Effect, nearHit.point, Quaternion.identity);
				if ((nearHit.collider.tag == "Figure"))
				{
					Transform j = nearHit.collider.transform;
					while ((j.parent.tag == "Figure") && (j.parent.name != "Tower")) j = j.parent;
					Damaging(j.gameObject, 700, RootName);
					
					if (Robo.ContainsKey(RootName))
					{
						if (Robo[RootName].Health <= 0)
                        {
							nearHit.transform.root.gameObject.SetActive(false);
							Robo[RootName].HealthCoroutine = null;
							return;
						}
							
						//print(RootName + "=" + Robo[RootName].Health);

						if(Robo[RootName].HealthCoroutine!=null)
                        {
							StopCoroutine(Robo[RootName].HealthCoroutine);
							Robo[RootName].HealthCoroutine = null;
						}
						if (Robo[RootName].HealthCoroutine == null) 
							Robo[RootName].HealthCoroutine = StartCoroutine(HealthTimer(10, RootName));
					}
				}
			}
			GameObject lr = new GameObject("LaserRay");
			GameObject Tm = new GameObject("Tm");
			lr.transform.parent = ee;
			Tm.transform.parent = ee;
			lr.transform.localPosition = new Vector3(0, 0, 1.4f);
			lr.AddComponent<LaserLine>();
			LaserLine ll = lr.GetComponent<LaserLine>();
			ll.width = 0.7f;
			ll.innerFadeMaterial = Setup.innerFadeMaterial;
			ll.outerFadeMaterial = Setup.outerFadeMaterial;
			ll.SetPositions(new Vector3[] { lr.transform.position, nearHit.point });
			ll.Visible = true;
			Destroy(lr, 0.1f);
			Destroy(Tm, 0.2f);
			lr.AddComponent<AudioSource>();
			AudioSource audio = lr.GetComponent<AudioSource>();
			audio.clip = Beam;
			audio.Play();
		}

		void CreateWheels(GameObject robot)
		{
			List<GameObject> WheelList = FindRecurse(robot, "Wheel1");
			if (WheelList.Count == 0)
			{
				Wheels = new GameObject[0];
				return;
			}
			foreach (GameObject ee in WheelList)
			{
				ee.transform.Find("Arm").Translate(-0.35f, 0f, 0f);
			}
			Wheels = new GameObject[WheelList.Count];
			Wheels = WheelList.ToArray();

			GameObject WC = new GameObject("WheelColliders");
			WC.transform.parent = robot.transform;
			WC.transform.localPosition = Vector3.zero;
			WC.transform.localRotation = Quaternion.identity;

			WheelColliders = new GameObject[WheelList.Count];
			int i = 0;
			foreach (GameObject child in Wheels)
			{
				Transform V = child.transform.Find("Arm");
				GameObject Z = new GameObject("Wheel1");
				Z.transform.parent = WC.transform;
				Z.transform.position = V.position;
				Z.transform.rotation = V.rotation;
				Z.AddComponent<WheelCollider>();
				Z.GetComponent<WheelCollider>().radius = 1.1f;
				WheelColliders[i++] = Z;
			}
			transform.gameObject.AddComponent<CarContrller2>().Robot = robot;
			transform.gameObject.GetComponent<CarContrller2>().Wheels = Wheels;
			transform.gameObject.GetComponent<CarContrller2>().WheelColliders = WheelColliders;
		}
		Vector3 WeaponsMeanPosition(GameObject rotor)
		{
			Vector3 pos = Vector3.zero;
			int i = 0;
			GameObject[] ar = Weapons[WeaponIndex];
			if (ar != null)
			{
				foreach (GameObject child in ar)
				{
					if (ChildIsInParent(child, rotor))
					{
						pos += child.transform.position;
						i++;
					}
				}
			}
			if (i == 0) return pos;
			pos /= i;
			return pos;
		}
		Boolean ChildIsInParent(GameObject child, GameObject eqParent)
		{
			Transform currentParent = child.transform.parent;
			while (currentParent != null)
			{
				//print(currentParent.gameObject.GetInstanceID()+" <> "+ eqParent.GetInstanceID());
				if (currentParent.gameObject.GetInstanceID() == eqParent.GetInstanceID())
				{
					return true;
				}
				currentParent = currentParent.parent;
			}
			return false;
		}

		void CreateInfo(GameObject x)
		{
			if (x.tag == "Robot")
			{
				Robo.Add(x.name, new InfoRobot(x));
				foreach (Transform child in x.transform)
				{
					if (child.tag == "Figure")
					{
						List<int> IdList = new List<int>();
						IdList = GetJoinInfoId(child.gameObject);
						InfoFigure i = new InfoFigure(child.gameObject, IdList);
						Info.Add(child.gameObject.GetInstanceID(), i);
						Robo[x.name].DefoltHealth += i.DefoltHealth;
						Robo[x.name].Health += i.Health;
					}
				}
			}
		}

		List<int> GetJoinInfoId(GameObject x)
		{
			List<int> IdList = new List<int>();
			foreach (Transform child in x.transform)
			{
				if (child.name == "Join")
				{
					IdList.Add(Setup.figureAtPlane(child).GetInstanceID());
				}
			}
			return IdList;
		}

		void Healing(string name,int hp) 
		{
			var keys = Info.Where(item =>(item.Value.RobotName == name)&&(item.Value.Health>0)&&(item.Value.Health<item.Value.DefoltHealth)).Select(item => item.Key).ToList();
			if (keys.Count > 0)
			{
				foreach (int x in keys)
				{
					var item = Info[x];
					int shift = item.DefoltHealth - item.Health;
					if (hp <= shift)
					{
						item.Health += hp;
						Robo[name].Health += hp;
						HealthColoring(item);
						//print("info[" + x + "].Health+=" + hp);
						hp = 0;
						break;
					}
					else
					{
						item.Health += shift;
						Robo[name].Health += shift;
						HealthColoring(item);
						//print("info[" + x + "].Health+=" + shift);
						hp -= shift;
					}
				}
			}
			//print("осталось hp:"+hp);
			if (hp == 0) return;
			keys = Info.Where(item => (item.Value.RobotName == name) && (item.Value.Health == 0))
				.OrderByDescending(item => item.Value.RealFrendsCount).Select(item => item.Key).ToList();
			if (keys.Count > 0)
			{
				
				foreach (int x in keys)
				{
					var item = Info[x];
					if(GetRealFrends(item).Count>0)
					{
						int shift = item.DefoltHealth;
						if (hp <= shift)
						{
							item.Health += hp;
							Robo[name].Health += hp;
							item.Owner.SetActive(true);
							HealthColoring(item);
                            //print("Создан info[" + x + "].Health+=" + hp);
                            if (item.Owner.name == "Wheel1")
                            {
								if (testCoroutine2 == null) testCoroutine2 = StartCoroutine(
									MoveCoroutine(Robo[name].Owner, 10)
								);
							}
                            if (item.Owner.name== "T0 Rotor") 
							{
							}
							hp = 0;
							foreach (int q in item.Frends)
							{
								Info[q].RealFrendsCount++;
							}
							break;
						}
						else
						{
							item.Health += shift;
							Robo[name].Health += shift;
							item.Owner.SetActive(true);
							HealthColoring(item);
							//print("Создан info[" + x + "].Health+=" + shift);
							if (item.Owner.name == "Wheel1")
							{
								if (testCoroutine2 == null) testCoroutine2 = StartCoroutine(
									MoveCoroutine(Robo[name].Owner, 10)
								);
							}
							if (item.Owner.name == "T0 Rotor")
							{
							}
							hp -= shift;
							foreach (int q in item.Frends)
							{
								Info[q].RealFrendsCount++;
							}
						}
					}
				}
			}
			//print("осталось hp:" + hp);
		}


		void HealthColoring(InfoFigure w)
        {
			float ratio = (float)w.Health / w.DefoltHealth;
			Coloring(w.Owner, Setup.Figures[w.Id].GO, w.BaseColor, ratio);
		}

		void Coloring(GameObject x,GameObject target,Color col,float ratio) 
		{
			var l = x.GetComponent<MeshRenderer>().materials.Length;
			var ca = new Color[l];
			var xMat = new Material[l];
			var yMat = new Material[l];
			xMat = x.GetComponent<MeshRenderer>().materials;
			yMat = target.GetComponent<MeshRenderer>().sharedMaterials;
			for (int i = 0; i < l; i++)
			{
				ca[i] = yMat[i].color;
				if (i == 0) ca[i] = col;
				xMat[i].color = new Color(ca[i].r * ratio, ca[i].g * ratio, ca[i].b * ratio);
			}
			x.GetComponent<MeshRenderer>().materials = xMat;
			if (x.name == "Tower") return;
			foreach (Transform child in x.transform)
			{
				Coloring(child.gameObject, target.transform.Find(child.name).gameObject, col, ratio);
			}
		}


		void Damaging(GameObject x, int damage, string name)
		{
			if (!Info.ContainsKey(x.GetInstanceID())) return;
			InfoFigure w = Info[x.GetInstanceID()];
			if (!x.activeSelf)
			{
				//print("Выход по пизде");
				return;
			}
			if (damage < w.Health)
			{
				w.Health -= damage;
				Robo[name].Health -= damage;
				
				//dark figure
				HealthColoring(w);
			}
			else
			{
				damage -= w.Health;
				x.SetActive(false);
				if (w.Frends.Count == 0)
				{
					Robo[name].Health -= w.Health;
					w.Health = 0;
					foreach (int q in w.Frends)
					{
						Info[q].RealFrendsCount--;
					}
					return;
				}
				List<int> RealFrends = GetRealFrends(w);
				if (RealFrends.Count > 0)
				{
					damage /= RealFrends.Count;
					//проверка на огрызки и удаление их
					CheckIsolatedSegmentS(w, name);
					//new
					Robo[name].Health -= w.Health;
					w.Health = 0;
					foreach (int q in w.Frends)
					{
						Info[q].RealFrendsCount--;
					}
					//new

					foreach (int z in RealFrends)
					{
						if (Info[z].Health > 0)
							Damaging(Info[z].Owner, damage, name);
					}
				}
				else
                {
					Robo[name].Health -= w.Health;
					w.Health = 0;
					foreach (int q in w.Frends)
					{
						Info[q].RealFrendsCount--;
					}
				}
			}
			//print("2)Health in "+w.Owner.name+" = "+w.Health+ " Robo.Health=" + Robo[name].Health);
		}

		List<int> GetRealFrends(InfoFigure x)
		{
			List<int> r = new List<int>();
			foreach (int z in x.Frends)
			{
				if (Info[z].Owner.activeSelf) r.Add(z);
			}
			return r;
		}


		void CheckIsolatedSegmentS(InfoFigure x, string name)
		{
			List<int> RealFrends = GetRealFrends(x);
			List<int>[] Segment = new List<int>[RealFrends.Count];
			int[] SegmentHealth = new int[RealFrends.Count];
			for (int i = 0; i < RealFrends.Count; i++)
			{
				Stack = new List<int>();
				GetSegment(RealFrends[i]);
				Segment[i] = Stack;
				SegmentHealth[i] = SumHealth(Segment[i]);
			}
			foreach (List<int> s in Segment)
			{
				//print("s.Count=" + s.Count + " SumHealth(s)=" + SumHealth(s));
			}
			int maxValue = SegmentHealth.Max();
			int maxIndex = SegmentHealth.ToList().IndexOf(maxValue);
			//print("maxValue =" + maxValue + " maxIndex =" + maxIndex);

			if (x.Owner.transform.parent.name == "Tower")
			{
				//print("in rotor");
				if (!Segment[maxIndex].Contains(x.Owner.transform.parent.parent.gameObject.GetInstanceID()))
				{
					//print("рукав больше тела");
					foreach (int z in Segment[maxIndex])
					{
						if (Info[z].Owner.transform.parent == x.Owner.transform.parent)
						{
							Info[z].Owner.transform.parent = Info[z].Owner.transform.root;
						}
					}
				};

			}

			if (x.Owner.name == "T0 Rotor")
			{
				//print("is rotor");
				Transform Tower = x.Owner.transform.Find("Tower");
				if (Info[Segment[maxIndex][0]].Owner.transform.parent.gameObject.GetInstanceID() == Tower.gameObject.GetInstanceID())//Robot2
				{
					//print("рукав больше тела2");

					List<GameObject> box = new List<GameObject>();
					foreach (Transform child in Tower)
					{
						box.Add(child.gameObject);
					}
					foreach (GameObject child in box)
					{
						child.transform.parent = child.transform.root;
					}

				}
			}
			for (int i = 0; i < SegmentHealth.Length; i++)
			{
				if (i != maxIndex)
				{
					if (!Segment[maxIndex].Contains(Segment[i][0]))
					{
						Segment[i].Reverse();
						foreach (int z in Segment[i])
						{
							Info[z].Owner.SetActive(false);
							Robo[name].Health -= Info[z].Health;
							Info[z].Health = 0;
							foreach (int q in Info[z].Frends)
							{
								Info[q].RealFrendsCount--;
							}
						}
					}
				}
			}
		}

		void GetSegment(int x)
		{
			Stack.Add(x);
			List<int> RealFrends = GetRealFrends(Info[x]);
			foreach (int z in RealFrends)
			{
				if ((!Stack.Contains(z)))
					GetSegment(z);
			}
			Stack = Stack.Distinct().ToList();
		}

		int SumHealth(List<int> x)
		{
			int sum = 0;
			foreach (int z in x)
			{
				sum += Info[z].Health;
			}
			return sum;
		}

		public class InfoFigure
		{
			private GameObject _owner;
			private int _health;
			private int _defoltHealth;
			private int _id;
			private Color _baseColor;
			private List<int> _frends;
			private int _realFrendsCount;
			public GameObject Owner
			{
				get
				{
					return _owner;
				}
			}
			public int DefoltHealth
			{
				get
				{
					return _defoltHealth;
				}
			}
			public int Health
			{
				get
				{
					return _health;
				}
				set
				{
					_health = value;
				}
			}
			public int Id
			{
				get
				{
					return _id;
				}
			}
			public Color BaseColor
			{
				get
				{
					return _baseColor;
				}
			}
			public List<int> Frends
			{
				get
				{
					return _frends;
				}
			}
			public string RobotName
            {
                get
                {
					return _owner.transform.root.name;
                }
            }
			public int RealFrendsCount
            {
				get
				{
					return _realFrendsCount;
				}
                set 
				{
					_realFrendsCount = value;
				}
			}
			public InfoFigure(GameObject owner, List<int> frends)
			{
				_owner = owner;
				_id = Setup.NameToInt(owner.name);
				_baseColor = owner.GetComponent<MeshRenderer>().material.color;
				_defoltHealth = Setup.Figures[_id].Health;
				_health = _defoltHealth;
				_frends = frends;
				_realFrendsCount = _frends.Count;
			}
		}

		public class InfoRobot
		{
			private int _health;
			private int _defoltHealth;
			private int _mass;
			private Coroutine _healthCoroutine;
			private GameObject _owner;
			public GameObject Owner
            {
				get
                {
					return _owner;
				}
            }
			public int Health
			{
				get
				{
					return _health;
				}
				set
				{
					_health = value;
				}
			}
			public int DefoltHealth
			{
				get
				{
					return _defoltHealth;
				}
				set
				{
					_defoltHealth = value;
				}
			}
			public int Mass
			{
				get
				{
					return _mass;
				}
				set
				{
					_mass = value;
				}
			}
			public Coroutine HealthCoroutine
			{
				get
				{
					return _healthCoroutine;
				}
				set
				{
					_healthCoroutine = value;
				}
			}
			public InfoRobot(GameObject owner)
			{
				_owner = owner;
			}
		}
	}
}
