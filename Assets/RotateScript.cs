using UnityEngine;
using System.Collections;

public class RotateScript : MonoBehaviour {

    public Vector3 rotationPoint;
    public Vector3 rotationAxis;
    public float angle = 1f;
    public bool rotating = false;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (rotating)
        {
            this.gameObject.transform.RotateAround(rotationPoint, rotationAxis, angle);
        }
    }

    public void StopRotating()
    {
        rotating = false;
    }

    public void StartRotating()
    {
        rotating = true;
    }
}
