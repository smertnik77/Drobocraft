using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public float speed;
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
		if (Setup.Pause) return;
        if (Input.GetKey(KeyCode.W))
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
			transform.Translate(Vector3.back * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
			transform.Translate(Vector3.left * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Translate(Vector3.right * speed * Time.deltaTime);
       if(Input.GetKey(KeyCode.Space))
        {
			transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
		if(Input.GetKey(KeyCode.LeftControl))
		{
			transform.Translate(Vector3.down * speed * Time.deltaTime);
		}
    }
}
