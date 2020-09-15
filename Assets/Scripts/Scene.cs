using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UI;
using static RuntimePreviewGenerator;

public class Scene : MonoBehaviour {
	RaycastHit[] hits;
	Camera cam;
	int x;
	GameObject nObject,pMarker,Marker;
	Material Shablon;
	Boolean stop=false;
	float ddd=100;
	float Angle=0f;
	void Start () {
		Setup.Start ();
		Setup.LoadBlocks ();
		cam = Camera.main;
		Marker = Resources.Load("Models/Edit/Marker", typeof(GameObject)) as GameObject;
		Shablon=Resources.Load("Materials/Shablon", typeof(Material)) as Material;



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
	Boolean HaveFigureRay()
	{
		foreach (RaycastHit r in hits) {
			if (r.transform.tag=="Figure") {
				return true;
			}
		}
		return false;
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
		Vector3 eee = new Vector3(fround(p.x), fround(p.y), fround(p.z));
		print (eee.y);
		return eee;
	}
	public static float fround(float x)
	{
		return Mathf.Round(x * 10) / 10;
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
	void DeleteShablon ()
	{
		GameObject[] s1 = GameObject.FindGameObjectsWithTag ("Shablon");
		if(s1.Length>0)Destroy(s1[0]);
		//DeleteShablon2 ();
	}
	// Update is called once per frame
	void FixedUpdate () {
		hits = Physics.RaycastAll (cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))).OrderBy(h=>h.distance).ToArray();
		//поворот джоинта
		/*
		Transform Tower = GameObject.Find ("Joint").transform;
		if (!stop) {
			
			//Transform Player = GameObject.Find ("Player").transform;
			Rigidbody z = Tower.GetComponent<Rigidbody> ();
			Vector3 target = hits[0].point - Tower.position;
			target = Quaternion.Inverse (Tower.parent.rotation) * target;
			target.y = 0;
			Quaternion qTo = Quaternion.LookRotation (target);
			qTo = Quaternion.Slerp (Tower.rotation, qTo, 5f * Time.deltaTime);
			z.MoveRotation (qTo);
			Angle = Tower.localEulerAngles.y;

		} else {
			GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
			GetComponent<Rigidbody> ().velocity = Vector3.zero;
			Angle = Tower.localEulerAngles.y;
		}
		*/
		/// 
		/// 
		if (!stop) {
			Transform Tower2 = GameObject.Find ("Joint2").transform;


			Vector3 target2 = GameObject.Find ("Player").transform.position - Tower2.position;
			target2 = Quaternion.Inverse (Tower2.parent.rotation) * target2;
			target2.y = 0;
			Quaternion qTo2 = Quaternion.LookRotation (target2);
			Tower2.localRotation =
			Quaternion.RotateTowards (Tower2.localRotation, qTo2, 150f * Time.deltaTime);



			Transform Tower = GameObject.Find ("Joint").transform;

			Tower.rotation = Tower2.rotation;
			Angle = Tower.localEulerAngles.y;
		}
		///
		if (Input.GetKeyDown (KeyCode.P)) {
			foreach(RaycastHit child in hits){
				print (child.transform.name);
			}
		}
		if (hits.Length > 0) {
			int index = -1;
			if (HaveFigureRay ()) {
				index = IndexFigurePlanRay ();
				if (index >= 0) {
					//print ("есть фигура c плоскостью index="+index);
				} else {
					//print ("есть фигура без плоскости");
					DeleteShablon ();
					//Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
					//RefreshFigureShablonColor (Setup.MainColor);
					return;
				}
			} else {
				if (HavePlanPRay ()) {
					index = IndexPlanPRay ();
					//print ("есть пол index="+index);
				} else {
					//print ("нет фигуры и пола");
					DeleteShablon ();
					//Setup.pokras (GameObject.Find("FigureShablon").transform, Setup.MainColor);
					//RefreshFigureShablonColor (Setup.MainColor);
					return;
				}

			}
			// вычисляем позицию для фигуры
			RaycastHit Ray = hits [index];
			//print (Ray.transform.position);
			Vector3 pos = posFigureOnPlane (Ray.transform);
			if (GameObject.FindGameObjectsWithTag ("Shablon").Length > 0) {
				GameObject ii=GameObject.FindGameObjectsWithTag ("Shablon")[0];
				pos = new Vector3(fround(pos.x), fround(pos.y), fround(pos.z));
				//print (pos== ii.transform.position);
				if 	(
					(ii.transform.position==pos)


				) 
				{
					return;
				}
			}
			DeleteShablon();
			nObject = madeShablon("Shablon",pos, Ray.transform.rotation, true);
			nObject.AddComponent<Shablon>();
			nObject.GetComponent<MeshCollider> ().isTrigger = true;
			nObject.AddComponent<Rigidbody> ();
			nObject.GetComponent<Rigidbody> ().isKinematic = true;
			nObject.GetComponent<Rigidbody> ().useGravity = false;

			// создаем подсветку примыкающей плоскости
			pMarker = Instantiate (Marker,Ray.transform.position,nObject.transform.rotation);
			pMarker.transform.parent = nObject.transform;
			pMarker.transform.GetComponent<MeshRenderer> ().material =  Shablon;
			pMarker.transform.localPosition = new Vector3 (0, 0.02f, 0);
			pMarker.name = "Marker";
		}

	}
	GameObject madeShablon(String tag,Vector3 pos, Quaternion rot,bool WithDelta=false)
	{
		//Setup.Figures[Setup.IdCube].GO.transform.localScale=new Vector3(0.5f,0.5f,0.5f);
		Setup.Figures[Setup.IdCube].GO.transform.localScale=new Vector3(0.49f,0.49f,0.49f);
		GameObject newFigure = GameObject.Instantiate(Setup.Figures[Setup.IdCube].GO,new Vector3(fround(pos.x), fround(pos.y), fround(pos.z)),rot);
		newFigure.transform.localScale=new Vector3(0.49f,0.49f,0.49f);
		newFigure.transform.position = new Vector3 (fround (pos.x), fround (pos.y), fround (pos.z));
		newFigure.name=Setup.Figures[Setup.IdCube].GO.name;
		String ns = newFigure.name;
		if ((ns != "Wheel") && (ns != "Wheel1") && WithDelta) {
			//newFigure.transform.Rotate (0, delta, 0);
			//newFigure.transform.rotation*= Quaternion.Euler(0, delta, 0);
		}
		//SettingsMesh (newFigure.transform);
		foreach (Transform child in newFigure.transform)
		{
			if (child.tag == "Plane") {
				Destroy (child.gameObject);
			}
		}
		Setup.Tagged (newFigure.transform,tag);
		//Сlarify(newFigure);
		return newFigure;
	}
	public void SettingsMesh(Transform x)
	{
		if (!x.gameObject.GetComponent<MeshCollider> ()) {
			x.gameObject.AddComponent<MeshCollider>();
		}
		//x.gameObject.AddComponent<shablon>();
		x.gameObject.GetComponent<MeshCollider>().convex = true;
		x.gameObject.GetComponent<MeshCollider>().isTrigger = true;
		foreach (Transform child in x) {
			if(child.tag!="Plane")
				SettingsMesh (child);
		}
	}
	void OnCollisionEnter(Collision collider) {
		stop = true;
		//print (transform.localEulerAngles.y+" "+Angle);
		transform.localEulerAngles=new Vector3(0,Angle,0);
	}

	void OnCollisionStay(Collision collider) {
		transform.localEulerAngles = new Vector3 (0, Angle, 0);
	}
	void OnCollisionExit(Collision collider) {
		stop = false;
		//print (transform.localEulerAngles.y+" "+Angle);
	}
}
