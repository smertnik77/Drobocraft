using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public class PlayerOrbitCamera : MonoBehaviour 
{  
	public GameObject cam;
	public float rotSpeed = 3f;
	private float _rotY;
	private float _rotX;
	private Vector3 _offset;
	void Start()
	{    
		cam = GameObject.Find ("Camera");
		cam.transform.position = transform.position + 30*Vector3.up;
		cam.transform.LookAt(transform);
		_rotX = cam.transform.eulerAngles.x; 
		_rotY = cam.transform.eulerAngles.y;
		_offset =transform.position+new Vector3(0,10,0) - cam.transform.position;


		//cam.transform.rotation = Quaternion.Euler(0, -90, 0);
	}
	void LateUpdate()
	{
        if (Setup.Pause) return;
        _rotX += Input.GetAxis("Mouse Y") * rotSpeed ;
		_rotY += Input.GetAxis("Mouse X") * rotSpeed ;
		if (_rotX < 1)
			_rotX = 1;
		if (_rotX > 179)
			_rotX = 179;
		
		if(Input.GetAxis("Mouse ScrollWheel")>0)
		{
			cam.GetComponent<Camera> ().fieldOfView-=5;
			if(cam.GetComponent<Camera> ().fieldOfView<=5)cam.GetComponent<Camera> ().fieldOfView=10;

		}
		if(Input.GetAxis("Mouse ScrollWheel")<0)
		{
			cam.GetComponent<Camera> ().fieldOfView+=5;
			if(cam.GetComponent<Camera> ().fieldOfView>=105)cam.GetComponent<Camera> ().fieldOfView=100;
		}
		Quaternion rotation = Quaternion.Euler(-_rotX,-90+ _rotY, 0);
		cam.transform.position = transform.position+new Vector3(0,10,0) - (rotation * _offset); 
		//cam.transform.LookAt(transform);
		cam.transform.LookAt(transform.position+new Vector3(0,10,0));
	}
}
