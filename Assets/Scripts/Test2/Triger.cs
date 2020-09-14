using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triger : MonoBehaviour {

	// Use this for initialization
	void Start () {
		print ("start");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnCollisionEnter(Collision col)
	{
		print ("коллизия in");
		GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
		GetComponent<HingeJoint>().connectedBody.angularVelocity=Vector3.zero;
	}
	void OnCollisionStay(Collision col)
	{
		print ("коллизия"+col.rigidbody.name);
		GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
		GetComponent<HingeJoint>().connectedBody.angularVelocity=Vector3.zero;
	}
	void OnCollisionExit(Collision col)
	{
		print ("коллизия out");
	}
}
