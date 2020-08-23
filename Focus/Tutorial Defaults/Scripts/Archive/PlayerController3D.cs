using UnityEngine;

public class PlayerController3D : CharacterController3D
{
    enum CamType { FirstPerson, ThirdPerson };
    [SerializeField] CamType CameraType = CamType.FirstPerson;

    //Stores the transform the main camera should be moved to 
    //when controlling the player character
    [SerializeField] Transform FirstPersonCameraHolder;
    [SerializeField] Transform ThirdPersonCameraHolder;
    [SerializeField] Transform ThirdPersonCameraTarget;

    [SerializeField] bool CanChangeCamType = false;

    //How sensitive the rotation of the camera is when 
    //controling the player character
    [SerializeField] float LookSensitivity = 100.0f;
    //The rotation of the camera around the x-axis (Up/Down)
    private float xRotation = 0.0f;

    private Vector2 LookDirection = new Vector2();
    private Vector2 MoveDirection = new Vector2();

    //Tracks whether the player character is currently being controlled by the player.
    //Only used if the player can control other objects such as vehicles etc.
    [SerializeField] bool IsControlling = true;

    private int CameraIndex = 0;
    int CameraCount = 2;

    private bool SuccessfullyInitialised = true;
    // Start is called before the first frame update
    void Start()
    {

        //Init the custom input manager and only track controllers connected on start
        CustomInputManager.InitialiseCustomInputManager();
        //Add bindings for a single player to the custom input manager
        CustomInputManager.CreateDefaultSinglePlayerInputManager();
        //Begin checking for controllers, to determine changes to the tracked controller list
        StartCoroutine(CustomInputManager.CheckForControllers());


        //Initialise default rotation types and ensure camera holders and/or targets can be found

        Debug.Log("Initialising " + transform.name + " as a " + CameraType + " character");

        switch (CameraType)
        {
            case CamType.FirstPerson:
                InitFirstPersonCamera();

                if (CanChangeCamType)
                {
                    InitThirdPersonCamera();
                }
                SetFirstPersonCamera();
                break;
            case CamType.ThirdPerson:
                InitThirdPersonCamera();
                if (CanChangeCamType)
                {
                    InitFirstPersonCamera();
                }
                SetThirdPersonCamera();
                break;
            default:
                SuccessfullyInitialised = false;
                Debug.LogError("Invalid Camera Type Selected in Player Controller 3D component.");
                break;
        }


        Cursor.lockState = CursorLockMode.Locked;
    }

    void InitFirstPersonCamera()
    {
        //Try to find the CameraHolder object if there is not one already assigned
        if (FirstPersonCameraHolder == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == "CameraHolder" || transform.GetChild(i).CompareTag("CameraHolder"))
                {
                    FirstPersonCameraHolder = transform.GetChild(i);
                    break;
                }
            }

            if (FirstPersonCameraHolder == null)
            {
                SuccessfullyInitialised = false;
                Debug.LogError("Camera Holder property in PlayerController3D has not been assigned and could not be found. Unable to process character movements.");
            }
            else
            {
                Debug.LogWarning("Camera Holder property in PlayerController3D has not been assigned. Succesfully found via manual search");
            }
        }
    }

    void SetFirstPersonCamera()
    {
        //Set the main camera's parent to be the camera holder if the player character is being controlled
        if (IsControlling && FirstPersonCameraHolder != null && CameraType == CamType.FirstPerson)
        {
            Camera.main.transform.position = FirstPersonCameraHolder.position;
            Camera.main.transform.rotation = FirstPersonCameraHolder.rotation;
            Camera.main.transform.SetParent(FirstPersonCameraHolder);
        }

        //By default, rotate the entire player transform when in first person
        RotatePlayerObject = true;
    }

    void SetThirdPersonCamera()
    {
        //Set the main camera's parent to be the camera holder if the player character is being controlled
        if (IsControlling && ThirdPersonCameraHolder != null && CameraType == CamType.ThirdPerson)
        {
            Camera.main.transform.position = ThirdPersonCameraHolder.position;
            Camera.main.transform.rotation = ThirdPersonCameraHolder.rotation;
            Camera.main.transform.SetParent(ThirdPersonCameraHolder);
        }
        //By default, do not rotate the entire player transform when in third person
        RotatePlayerObject = false;
    }

    void InitThirdPersonCamera()
    {
        //Try to find the camera target object if not already assigned
        if (ThirdPersonCameraTarget == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == "CameraTarget" || transform.GetChild(i).CompareTag("CameraTarget"))
                {
                    ThirdPersonCameraTarget = transform.GetChild(i);
                    break;
                }
            }

            if (ThirdPersonCameraTarget == null)
            {
                SuccessfullyInitialised = false;
                Debug.LogError("Camera Target property in PlayerController3D has not been assigned and could not be found. Unable to process character movements.");
            }
            else
            {
                Debug.LogWarning("Camera Target property in PlayerController3D has not been assigned. Succesfully found via manual search");
            }
        }

        //using the camera target, try to find the camera holder if not already assigned
        if (ThirdPersonCameraHolder == null)
        {
            for (int i = 0; i < ThirdPersonCameraTarget.childCount; i++)
            {
                if (ThirdPersonCameraTarget.GetChild(i).name == "CameraHolder" || ThirdPersonCameraTarget.GetChild(i).CompareTag("CameraHolder"))
                {
                    ThirdPersonCameraHolder = ThirdPersonCameraTarget.GetChild(i);
                    break;
                }
            }

            if (ThirdPersonCameraHolder == null)
            {
                SuccessfullyInitialised = false;
                Debug.LogError("Camera Holder property in PlayerController3D has not been assigned and could not be found. Unable to process character movements.");
            }
            else
            {
                Debug.LogWarning("Camera Holder property in PlayerController3D has not been assigned. Succesfully found via manual search");
            }
        }
    }

    void ChangeCamera()
    {
        //Increment the camera index
        //NB most cases will only require switching between 1st and 3rd person
        //this method of switching is used if multiple third person camera positions were to be used
        //(ie convert camera holder to list of transforms and set camera count dynamically)
        CameraIndex++;

        if (CameraIndex >= CameraCount)
        {
            CameraIndex = 0;
        }

        switch (CameraIndex)
        {
            case 0:
                CameraType = CamType.FirstPerson;
                SetFirstPersonCamera();
                break;
            case 1:
                CameraType = CamType.ThirdPerson;
                SetThirdPersonCamera();
                break;
            default:
                Debug.LogError("Invalid camera index when changing camera in PlayerController3D");
                break;
        }
    }

    //This function handles input depending on the input device being used and updates the relevant parameters
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if (CustomInputManager.GetButtonDown("ActionButton1"))
        {
            Jump = true;
        }

        if (CustomInputManager.GetButtonDown("ActionButton2") && CanChangeCamType)
        {
            ChangeCamera();
        }


        MoveDirection = new Vector2(CustomInputManager.GetAxisRaw("LeftStickHorizontal"), CustomInputManager.GetAxisRaw("LeftStickVertical"));
        LookDirection = new Vector2(CustomInputManager.GetAxisRaw("RightStickHorizontal"), CustomInputManager.GetAxisRaw("RightStickVertical")) * LookSensitivity * Time.deltaTime;


    }


    // Update is called once per frame
    void Update()
    {
        //Update the player character if it was successfully initialised and is being controlled
        if (SuccessfullyInitialised && IsControlling)
        {
            HandleInput();

            switch (CameraType)
            {
                case CamType.FirstPerson:
                    UpdateFirstPersonLookAt();
                    break;
                case CamType.ThirdPerson:
                    if (MoveDirection == Vector2.zero)
                    {
                        RotatePlayerObject = false;
                    }
                    else
                    {
                        RotatePlayerObject = true;
                    }
                    UpdateThirdPersonLookAt();
                    break;
                default:
                    Debug.LogError("Invalid Camera Type selected when updating the LookAt direction. Ensure this parameter is not being unsafely changed at runtime.");
                    break;
            }

            MoveCharacter(MoveDirection, LookDirection);
        }
    }

    //This function handles the rotations for the first person camera
    void UpdateFirstPersonLookAt()
    {
        //Update the camera's rotation based on the player input
        xRotation -= LookDirection.y;
        //Clamp the x rotation of the camera to prevent turning upside down
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        FirstPersonCameraHolder.localRotation = Quaternion.Euler(xRotation, FirstPersonCameraHolder.localRotation.eulerAngles.y, 0.0f);
    }

    //This function handles the rotations for the third person camera
    void UpdateThirdPersonLookAt()
    {
        ////Update the camera's rotation based on the player input
        xRotation -= LookDirection.y;
        //Clamp the x rotation of the camera to prevent turning upside down
        xRotation = Mathf.Clamp(xRotation, -35, 60);

        //Rotate the camera based on input if not moving, otherwise, lerp back behind the character
        if (!RotatePlayerObject)
        {
            ThirdPersonCameraTarget.Rotate(new Vector3(0, LookDirection.x, 0));
        }
        else
        {
            ThirdPersonCameraTarget.localRotation = Quaternion.Euler(ThirdPersonCameraTarget.localRotation.eulerAngles.x, Mathf.LerpAngle(ThirdPersonCameraTarget.localRotation.eulerAngles.y, 0, Time.deltaTime), ThirdPersonCameraTarget.localRotation.eulerAngles.z);
        }

        //Update the x-rotation (up/down) of the camera
        ThirdPersonCameraTarget.localRotation = Quaternion.Euler(xRotation, ThirdPersonCameraTarget.localRotation.eulerAngles.y, 0);
        ThirdPersonCameraHolder.LookAt(ThirdPersonCameraTarget);
    }

}
