using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;


public class LabelOnRobot : MonoBehaviour
{
	Boolean Enabled;
	GameObject robot;
	Camera cam;
	Texture2D texture;
	GUIStyle style;
	GUIStyle shadow;
	GameObject Label;
	// Start is called before the first frame update
	void Start()
    {
		robot = transform.parent.gameObject;
		cam = GameObject.Find("Camera").GetComponent<Camera>();
		Enabled = true;

		texture = new Texture2D(1, 1);
		style = new GUIStyle();
		style.fontSize = 20;
		style.normal.textColor = Color.red;
		style.alignment = TextAnchor.MiddleCenter;
		shadow = new GUIStyle();
		shadow.fontSize = 20;
		shadow.normal.textColor = new Color(0f, 0f, 0f,0.5f);
		shadow.alignment = TextAnchor.MiddleCenter;

	}

    // Update is called once per frame
    void Update()
    {
        
	}

	public void OnGUI()
	{
		if (Setup.Pause) return;
		if (Enabled&&robot.activeSelf)
		{
			//Vector3 pos = new Vector3(robot.transform.position.x, robot.transform.position.y + 7, robot.transform.position.z);
			transform.position = robot.transform.position+new Vector3(0,7,0);
			Vector3 pos = transform.position;
			Vector2 screenPoint = cam.WorldToScreenPoint(pos)+new Vector3(-60,-10,0);
			string s = robot.name;
			GUI.Label(new Rect(screenPoint.x - 1, Screen.height - screenPoint.y - 1, 120, 20), s, shadow);
			GUI.Label(new Rect(screenPoint.x + 1, Screen.height - screenPoint.y + 1, 120, 20), s, shadow);
			GUI.Label(new Rect(screenPoint.x - 1, Screen.height - screenPoint.y + 1, 120, 20), s, shadow);
			GUI.Label(new Rect(screenPoint.x + 1, Screen.height - screenPoint.y - 1, 120, 20), s, shadow);
			GUI.Label(new Rect(screenPoint.x, Screen.height - screenPoint.y, 120, 20), s, style);
			Progress(new Rect(screenPoint.x, Screen.height + 24 - screenPoint.y, 120, 6), Test.Test.Robo[s].Health, Test.Test.Robo[s].DefoltHealth);
		}
	}
	void Progress(Rect pos, int Health, int DefaultHealth)
	{
		int x = (int)((float)Health / DefaultHealth * 120);
		Rect pos2 = pos;
		pos2.x -= 1;
		pos2.y -= 1;
		pos2.width += 2;
		pos2.height += 2;
		DrawQuad(pos2, new Color(0f, 0f, 0f, 0.5f));
		pos.width = x;
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

	void OnBecameVisible()
	{
		Enabled = true;
		//print("on");
	}

	void OnBecameInvisible()
	{
		Enabled = false;
		//print("off");
	}
	void Awake()
	{
		Enabled = true;
		//print("on");
	}
}