using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider other)
	{
		print ("OnTriggerEnter");
	}
	void OnCollisionEnter(Collision col)
	{
		print ("коллизия");
	}
	void OnTriggerStay(Collider other)
	{
		print ("OnTriggerStay");
	}
}