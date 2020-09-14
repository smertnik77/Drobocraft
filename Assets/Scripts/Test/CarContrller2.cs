using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarContrller2 : MonoBehaviour {
	public GameObject Robot;
	public GameObject[] Wheels;
	public GameObject[] WheelColliders;
	Vector3 center;
	public float maxMotorTorque=2000; // максимальный крутящий момент
	public float maxSteeringAngle=30; // максимальный угол поворота колес
	private AudioClip CarEngine;
	new public AudioSource audio;
	// Use this for initialization
	void Start () {
		cent ();
		Robot.AddComponent<AudioSource> ();
		CarEngine = Resources.Load("Audio/CarEngine", typeof(AudioClip)) as AudioClip;
		audio = Robot.GetComponent<AudioSource> ();
		audio.clip = CarEngine;
		audio.volume = 0.1f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Setup.Pause) return;
		float motor = maxMotorTorque * Input.GetAxis("Vertical");
		float[] steering=new float[Wheels.Length];
		//float input = Input.GetAxis ("Horizontal");
		steering=CalculateSteering (Input.GetAxis("Horizontal"));
		for (int x = 0; x < Wheels.Length; x++) {
			WheelColliders[x].transform.GetComponent<WheelCollider>().motorTorque = motor;
			WheelColliders[x].transform.GetComponent<WheelCollider>().steerAngle = steering[x];
			Wheels[x].transform.Find("Arm").localEulerAngles=new Vector3(steering[x],0,0);
			Wheels[x].transform.Find("Arm").Find("Wheel").Rotate(0, WheelColliders[x].transform.GetComponent<WheelCollider>().rpm / 60 * 360 * Time.deltaTime, 0);
		}
		audio.pitch = Robot.transform.GetComponent<Rigidbody>().velocity.magnitude * 3.4f/100;
		audio.Play ();
		//if (input == 0)
		//	Robot.transform.GetComponent<Rigidbody> ().velocity *= 0.95f;
	}
	void cent()
	{
		center = Vector3.zero;
		foreach (GameObject child in Wheels) {
			center += child.transform.localPosition;
		}
		center /= Wheels.Length;
	}
	public float[] CalculateSteering (float input)
	{
		float[] yy=new float[Wheels.Length];
		for (int x = 0; x < Wheels.Length; x++) {
			float carLenth = Mathf.Abs(center.z - WheelColliders [x].transform.localPosition.z);
			float carWidth = Mathf.Abs(center.x - WheelColliders [x].transform.localPosition.x);

			float carLenth2 = center.z - WheelColliders [x].transform.localPosition.z;
			float carWidth2 = center.x - WheelColliders [x].transform.localPosition.x;


			float turnRadius = Mathf.Abs (carLenth*Mathf.Tan(Mathf.Deg2Rad*(90-Mathf.Abs(maxSteeringAngle*input))));
			if (turnRadius < 8f)
				turnRadius = 8f;

			float radiusBig = turnRadius + carWidth;
			float radiusSmall = turnRadius - carWidth;

			float Angle=0f;
			if (carWidth2> 0) {

				//print(x+" turnRadius="+turnRadius+" carWidth"+carWidth);
				//левое колесо
				if (input > 0)//вправо
					Angle = Mathf.Rad2Deg * Mathf.Atan (carLenth / radiusBig);
				else  //влево
					Angle = Mathf.Rad2Deg * Mathf.Atan (carLenth / radiusSmall);
				Angle = Mathf.Sign (input) * Angle;
				if (carLenth2 < 0)
					Angle = -Angle;
				Angle = -Angle;
			}

			if (carWidth2 < 0) {
				//правое колесо
				if (input > 0) //вправо
					Angle = -Mathf.Rad2Deg * Mathf.Atan (carLenth /  radiusSmall);
				else //влево
					Angle = Mathf.Rad2Deg * Mathf.Atan (carLenth / radiusBig);
				//Angle = Mathf.Sign (input) * Angle;
				if (carLenth2 < 0)
					Angle = -Angle;

			}

			//print (Angle);
			yy [x] = Angle;
		}
		return yy;
	}
}
