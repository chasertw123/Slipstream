using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : MonoBehaviour
{
    
    [Header("Sense Settings")]
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float movementAcceleration = 4.0f;

    [Header("Camera Bobbing Settings")] 
    [SerializeField] private float bobbingSpeed = 0.18f;
    [SerializeField] private float crouchModifier = 0.5f;
    [SerializeField] private float runModifier = 1.3f;
    [SerializeField] private Vector2 bobbingAmount = new Vector2(0.1f, 0.2f);
    
    [Header("Speed Settings")]
    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float runSpeed = 12.0f;
    [SerializeField] private float airSpeed = 3.0f;
    [SerializeField] private float crouchSpeed = 3.0f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpMultipler = 1.0f;
    [SerializeField] private AnimationCurve jumpFallOff = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Slope Settings")] 
    [SerializeField] private float slopeForce = 6.0f;
    [SerializeField] private float slopeForceRayLength = 1.5f;

    [Header("Third Person Settings")] 
    [SerializeField] private float cameraTransitionDelta = 0.75f;
    [SerializeField] private Vector3 thirdPersonCameraOffset = new Vector3(-1.0f, 1.25f, 0.5f);
    
    // Player Movement Components
    private CharacterController controller;
    private Camera cam;
    
    // Third Person Components
    private Animator animator;
    
    // Camera Clamping
    private float xAxisClamp = 0.0f;
    
    // Head Bobbing
    private float yOffset;
    private float timer;

    // Player Movement
    private float movementSpeed = 0.0f;
    private Vector3 moveDir;
    
    // State Bools
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool isThirdPerson = false;
    private bool isCameraTransitioning = false;
    
    void Awake()
    {
        // Components
        controller = GetComponent<CharacterController>();
        
        // Children Components
        cam = GetComponentInChildren<Camera>();
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        yOffset = cam.transform.localPosition.y;
        
        animator.gameObject.SetActive(isThirdPerson);
    }

    void Update()
    {
        UpdateCameraPerspective();
        
        CameraRotation();
        CameraBobbing();
        
        Movement();
    }

    #region Camera Functions

    private void CameraRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        xAxisClamp += mouseY;
        
        if (xAxisClamp > 90.0f)
        {
            xAxisClamp = 90.0f;
            mouseY = 0.0f;
            ClampCameraXAxisRotationToValue(270.0f);
        }
        
        else if (xAxisClamp < -90.0f)
        {
            xAxisClamp = -90.0f;
            mouseY = 0.0f;
            ClampCameraXAxisRotationToValue(90.0f);
        }
        
        cam.transform.Rotate(Vector3.left * mouseY);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void ClampCameraXAxisRotationToValue(float value)
    {
        Vector3 eulerRotation = cam.transform.eulerAngles;
        eulerRotation.x = value;
        
        cam.transform.eulerAngles = eulerRotation;
    }

    private void CameraBobbing()
    {

        if (!controller.isGrounded || isCameraTransitioning)
        {

            return;
        }

        float horizontal = Mathf.Abs(Input.GetAxis("Horizontal"));
        float vertical = Mathf.Abs(Input.GetAxis("Vertical"));
        float waveSlice = 0.0f;
        
        if (horizontal == 0 && vertical == 0)
        {
            timer = timer - bobbingSpeed;
            
            if (timer < 0)
                timer = 0.0f;
        }

        else
        {
            waveSlice = Mathf.Sin(timer);
            timer = timer + bobbingSpeed;
            
            if (timer > Mathf.PI * 2)
                timer = timer - (Mathf.PI * 2);   
        }
        
        bool isRunning = Input.GetButton("Shift");
        Vector3 cameraPosition = cam.transform.localPosition;
        
        if (waveSlice != 0) 
        {
            float translateChangeX = waveSlice * (bobbingAmount.x * (isCrouching ? crouchModifier : (isRunning ? runModifier : 1)));
            translateChangeX *= Mathf.Clamp (horizontal + vertical, 0.0f, 1.0f);
            
            if (vertical != 0)
                cameraPosition.x = translateChangeX;

            if (isCrouching)
                cameraPosition.y = controller.height / 2 - 0.1f - ((Mathf.Abs(translateChangeX) / bobbingAmount.x) * bobbingAmount.y);
            
            else
                cameraPosition.y = yOffset + (isThirdPerson ? thirdPersonCameraOffset.y : 0) - ((Mathf.Abs(translateChangeX) / bobbingAmount.x) * bobbingAmount.y);
        }

        cam.transform.localPosition = cameraPosition;
    }

    private void UpdateCameraPerspective()
    {
        if (Input.GetButtonDown("Change Camera") && !isCameraTransitioning)
            StartCoroutine(ChangeCameraPerspective());
    }

    private IEnumerator ChangeCameraPerspective()
    {
        isCameraTransitioning = true;

        Vector3 start = cam.transform.localPosition;
        Vector3 target = start + (!isThirdPerson ? thirdPersonCameraOffset : -thirdPersonCameraOffset);
        
        animator.gameObject.SetActive(!isThirdPerson);

        float time = 0.0f;
        while (time < cameraTransitionDelta)
        {
            time += Time.deltaTime;
            cam.transform.localPosition = Vector3.Lerp(start, target, time);

            yield return null;
        }

        isThirdPerson = !isThirdPerson;
        isCameraTransitioning = false;
    }

    #endregion

    #region Movement Functins

    private void Movement()
    {
        float horizInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");

        Vector3 forwardMovement = transform.forward * vertInput;
        Vector3 rightMovement = transform.right * horizInput;
        
        if (Input.GetButtonDown("Crouch") && !isCrouching)
            StartCoroutine(Crouch());
        
        if (Input.GetButtonDown("Jump") && !isJumping)
            StartCoroutine(Jump());

        bool isRunning = Input.GetButton("Shift");
        float currentMaxSpeed = isCrouching ? crouchSpeed : isRunning ? runSpeed : walkSpeed;
        
        movementSpeed = Mathf.Lerp(movementSpeed, currentMaxSpeed, Time.deltaTime * movementAcceleration); // Set Movement Speed
        moveDir = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f);

        if (animator.gameObject.activeInHierarchy)
        {
            float velY = ((vertInput * movementSpeed) / currentMaxSpeed) * (!isRunning ? 0.5f : 1f);
            float velX = ((horizInput * movementSpeed) / currentMaxSpeed) * (!isRunning ? 0.5f : 1f);

            animator.SetFloat("VelY", velY);
            animator.SetFloat("VelX", velX);
        }
        
        if (!controller.SimpleMove(moveDir * movementSpeed))
            controller.Move(moveDir * airSpeed * Time.deltaTime); 

        if ((vertInput != 0.0f || horizInput != 0.0f) && OnSlope())
            controller.Move(Vector3.down * controller.height / 2 * slopeForce * Time.deltaTime);
    }
    
    private bool OnSlope()
    {
        if (isJumping)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, controller.height / 2 * slopeForceRayLength))
            if (hit.normal != Vector3.up)
                return true;

        return false;
    }
    
    private IEnumerator Jump()
    {
        controller.slopeLimit = 90.0f;
        isJumping = true;

        float airTime = 0.0f;
        do
        {
            float jumpForce = jumpFallOff.Evaluate(airTime);
            controller.Move(Vector3.up * jumpForce * jumpMultipler * Time.deltaTime);

            airTime += Time.deltaTime;
            yield return null;
        } while (!controller.isGrounded && controller.collisionFlags != CollisionFlags.Above);

        controller.slopeLimit = 45.0f;
        isJumping = false;
    }
    
    private IEnumerator Crouch()
    {
        Vector3 originalCameraPos = cam.transform.localPosition;
        bool preGrounded = controller.isGrounded;

        isCrouching = true;
       
        controller.height /= 2;
        cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, controller.height / 2 - 0.1f, cam.transform.localPosition.z);

        if (preGrounded)
            do
            {
                controller.Move(Vector3.down * controller.height / 2 * Time.deltaTime);
            } while (!controller.isGrounded || controller.collisionFlags != CollisionFlags.Below);
        
        RaycastHit hit;
        while (Input.GetButton("Crouch") || Physics.Raycast(transform.position + new Vector3(-controller.radius, controller.height * 0.5f, 0), Vector3.up, out hit, 0.15f))
            yield return null;

        cam.transform.localPosition = originalCameraPos;
        controller.height *= 2;
        
        isCrouching = false;
    }
    
    #endregion

}
