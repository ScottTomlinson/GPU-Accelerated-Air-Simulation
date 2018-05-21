using UnityEngine;
using System.Collections;

public class CamControl : MonoBehaviour {

    public float moveSpeed = 1.0f;
    public float keyMoveSpeed = 1.0f;
    public float rotSpeed = 1.0f;
    public float keyRotSpeed = 1.0f;
    public float zoomSpeed = 2.0f;
    private float zoomAmt = 0.0f;
    public float maxPos = 100.0f;
    public float minPos = 0.0f;
    public float minPitch = 5.0f;
    public float maxPitch = 89.0f;
    public float minZoom = 1.0f;
    public float maxZoom = 20.0f;

    private bool active = true;

    private Vector2 moveDir;
    private Vector2 rotDir;

    private GameObject rotator;
    private GameObject zoomer;

    private Camera cam;
    private Camera altCam;

	// Use this for initialization
	void Start () {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        //altCam = GameObject.Find("AltCamera").GetComponent<Camera>();
        rotator = transform.Find("CameraRotator").gameObject;
        zoomer = rotator.transform.Find("CameraLifter").gameObject;
        moveDir = new Vector2(0f, 0f);
        rotDir = new Vector2(0f, 0f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            //move forward
            moveDir.y = keyMoveSpeed;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            //move backward
            moveDir.y = -keyMoveSpeed;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            //move left
            moveDir.x = -keyMoveSpeed;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            //move right
            moveDir.x = keyMoveSpeed;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            rotDir.y = keyRotSpeed;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            rotDir.y = -keyRotSpeed;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
        {
            rotDir.x = keyRotSpeed;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.E))
        {
            rotDir.x = -keyRotSpeed;
        }

        /*
        if (Input.GetMouseButton(0))
        {
            //linear motion
            moveDir.x = -Input.GetAxis("Mouse X");
            moveDir.y = -Input.GetAxis("Mouse Y");
        }
        */
        if (Input.GetMouseButton(2))
        {
            //rotational motion
            rotDir.x = Input.GetAxis("Mouse X");
            //rotDir.y = -Input.GetAxis("Mouse Y");
        }
        if(Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            zoomAmt = Input.GetAxis("Mouse ScrollWheel");
        }
        else
        {
            zoomAmt = 0f;
        }


    }

    void FixedUpdate()
    {
        if (active)
        {
            if (moveDir.x != 0f || moveDir.y != 0f)
            {
                MoveStuff();
            }
            if (rotDir.x != 0f || rotDir.y != 0f)
            {
                RotateStuff();
            }
            if (zoomAmt != 0f)
            {
                ZoomStuff();
            }
        }
    }
    

    void ZoomStuff()
    {
        /*
        //zoomer.transform.localPosition += zoomer.transform.forward * zoomAmt * zoomSpeed * Time.deltaTime;
        Vector3 newPos = zoomer.transform.localPosition;
        newPos.z += zoomAmt * zoomSpeed * Time.deltaTime;
        newPos.z = Mathf.Clamp(newPos.z, -maxZoom, minZoom);
        //newPos.x = Mathf.Clamp(newPos.x, minZoom, maxZoom);
        zoomer.transform.localPosition = newPos;
        */
        cam.orthographicSize += (zoomAmt * zoomSpeed);
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        altCam.orthographicSize += (zoomAmt * zoomSpeed);
        altCam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    void MoveStuff()
    {
        this.transform.position += transform.forward * Time.deltaTime * moveDir.y * moveSpeed;
        this.transform.position += transform.right * Time.deltaTime * moveDir.x * moveSpeed;
        Vector3 newPos = this.transform.position;
        //newPos = Vector3.Lerp(this.transform.position, newPos, Time.deltaTime * moveSpeed);
        newPos.x = Mathf.Clamp(newPos.x, minPos, maxPos);
        newPos.z = Mathf.Clamp(newPos.z, minPos, maxPos);
        this.transform.position = newPos;
        

    }

    void RotateStuff()
    {
        //euler angles fuck -- lol nope
        Vector3 angle = new Vector3(rotDir.y, 0f, 0f);
        rotator.transform.Rotate(angle * rotSpeed * Time.deltaTime);

        angle = new Vector3(0f, rotDir.x, 0f);
        this.transform.Rotate(angle * rotSpeed * Time.deltaTime * 1.5f);



    }

    void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
        {
            //stop forward/backward motion
            moveDir.y = 0f;
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            //stop lateral motion
            moveDir.x = 0f;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            moveDir.x = 0f;
            moveDir.y = 0f;
        }


        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            //stop pitch motion
            rotDir.y = 0f;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
        {
            //stop yaw motion
            rotDir.x = 0f;
        }
        if (Input.GetMouseButtonUp(2))
        {
            rotDir.x = 0f;
            rotDir.y = 0f;
        }

    }
}
