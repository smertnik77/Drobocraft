using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shablon : MonoBehaviour {
	Material red;
	// Use this for initialization
	void Start () {
		//print ("shablon запущен");
		red=Resources.Load("Materials/Red", typeof(Material)) as Material;
	}
	// Update is called once per frame
	void Update () {
		//print ("shablon запущен");
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.name!="Player"){
			if (gameObject.tag == "Shablon") {
				if (GameObject.Find ("Marker"))
					GameObject.Find ("Marker").GetComponent<MeshRenderer> ().material.color = Color.red;
			}
			redPokras (transform);
		}
	}
	/*
	void OnCollisionEnter(Collision col)
	{
		print ("коллизия");
		if (col.gameObject.name == "PlaneP")
			return;
		if (col.gameObject.name == "box")
			return;
		if (col.gameObject.name == "Plane")
			return;
		//print ("столкновение с " + col.gameObject.name+" центр="+col.transform.localPosition + " в " + col.contacts.Length + " точках");
		foreach (ContactPoint contact in col.contacts) {
			Debug.DrawRay (contact.point, contact.normal, Color.white);
			if (contact.separation <= 0) {
				print ("столкновение");
				//print ("столкновение с " + col.gameObject.name+" центр="+col.transform.localPosition + " в " + col.contacts.Length + " точках");
				//print ("столкновение с " + col.gameObject.name+ " глубина=" +contact.separation );
				//if(GameObject.Find("Marker")!=null)
				//	GameObject.Find ("Marker").GetComponent<MeshRenderer> ().material.color = Color.red;

				//redPokras (transform.Find ("Marker").parent);
				return;
			}
		}
	}
	*/
	void redPokras(Transform x)
	{
		int l = x.GetComponent<MeshRenderer>().materials.Length;
		Material[] xMat = new Material[l];
		for (int i = 0; i < l; i++)
		{
			//red.color = Color.cyan;
			xMat[i] = red;
		}
		x.GetComponent<MeshRenderer>().materials = xMat;
		foreach (Transform child in x) {
			if ((child.tag != "Plane")&&(child.name != "Marker")) {
				redPokras (child);
			}

		}
	}
}
