using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour {
	public float speed;
	public GameObject cam;
	public GameObject rot;
	// Use this for initialization
	void Start () {
		cam.transform.LookAt(transform);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			cam.transform.Translate (0, 0, -2);
			//print (cam.transform.localPosition.z);
		}
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			cam.transform.Translate (0, 0, 2);
			//print (cam.transform.localPosition.z);
		}
		if (Input.GetMouseButton(0)||Input.GetMouseButton(1))
		{
			if ((Input.GetAxis ("Mouse Y") < 0)&&(rot.transform.rotation.eulerAngles.x<80)) {
				rot.transform.Rotate(2, 0, 0);

			}
			if ((Input.GetAxis ("Mouse Y") > 0)&&(rot.transform.rotation.eulerAngles.x>2)) {
				rot.transform.Rotate(-2, 0, 0);

			}
			transform.Rotate (0,6*Input.GetAxis ("Mouse X"),0);
		}
		transform.Rotate (0,speed*Time.deltaTime,0);


	}
}
