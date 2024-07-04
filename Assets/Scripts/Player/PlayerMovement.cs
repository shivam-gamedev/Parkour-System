using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public float _airMoveSpeed;





    [Header("Jump & Gravity")]
    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;


    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool isGrounded = true;
    public bool isGrounded_Anim = false;

    [Tooltip("Useful for rough ground")]
    public Vector3 GroundedOffset;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;
    [SerializeField] float ParkourRotationSpeed = 10;

    public bool isOnLedge { get; set; }
    public bool isHanging { get; set; }


    // player
    private float _speed;
    [SerializeField] private float _animationBlend;
     private float _targetRotation = 0.0f;
    private float _rotationVelocity;
     float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    [SerializeField] bool _hasControl = true;
     Vector3 desiredTargetDir;
     Vector3 TargetDir;
    



    // animation IDs
    private int _animIDSpeed;
    // private int _animIDGrounded;
    // private int _animIDJump;
    // private int _animIDFreeFall;
    // private int _animIDMotionSpeed;

    public InputManager _input;
    private CharacterController _controller;
    private Transform _mainCamera;
    private Animator _animator;

    private ParkourController _parkourController;

    private EnvironmentScanner environmentScanner;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 forwardvelocity;
     private float targetSpeed;

    //Parkour Properties
    public LedgeData LedgeData { get; set; }
    public bool _inAction;
    [SerializeField] private bool canHaveControlls = true;
   

    private void Awake()
    {
        // get a reference to our main camera
        _mainCamera = Camera.main.transform;

        AssignAnimationIDs();
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputManager>();
        _animator = GetComponent<Animator>();
        _parkourController = GetComponent<ParkourController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }

    private void Update()
    {
        if (!_hasControl)
            return;
        if (isHanging)
            return;
        GroundedCheck();
        JumpAndGravity();
        Move();

            if(Input.GetKey(KeyCode.R) )
                 _animator.Play("SnakeDance");

            else if(Input.GetKey(KeyCode.T) )
                  _animator.Play("Dancing");

            else if(Input.GetKey(KeyCode.Y) )
                 _animator.Play("hiphop");

            else if(Input.GetKey(KeyCode.U) )
                 _animator.Play("TutDance");

            else if(Input.GetKey(KeyCode.I) )
                 _animator.Play("BellyDance");

            else if(Input.GetKey(KeyCode.O) )
                 _animator.Play("lockdance");
            else if(Input.GetKey(KeyCode.P) )
                 _animator.Play("traindance");
    }


    public bool _HasControl 
    {
        get =>_hasControl;
        set => _hasControl = value;
    }
    public IEnumerator DoAction(string Animname, MatchTargetParams matchParams = null, Quaternion targetRotation = new Quaternion(), bool rotate = false,
     float postDelay = 0, bool mirror = false)
    {

        _inAction = true;

        _animator.SetBool("mirrorAction", mirror);
        _animator.CrossFadeInFixedTime(Animname, 0.2f);
        yield return null;


        var animState = _animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(Animname))
        {
            Debug.Log("Animname :" + Animname);
            Debug.LogError("The Parkour animation name is Wrong");
        }


        float rotationStartTime = (matchParams != null) ? matchParams.startime : 0;

        Debug.Log("TargetRotation :" + targetRotation);
        float timer = 0f;

        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizeTime = timer / animState.length;


            //Rotate the player to the obstacle
            if (rotate && normalizeTime > rotationStartTime)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, ParkourRotationSpeed * Time.deltaTime);



            if (matchParams != null && !_animator.IsInTransition(0))
                MatchTarget(matchParams);

            if (_animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }

        yield return new WaitForSeconds(postDelay);


        _inAction = false;
    }

    void MatchTarget(MatchTargetParams mp)
    {
        if (_animator.isMatchingTarget) return;
        _animator.MatchTarget(mp.pos, transform.rotation, mp.bodypart, new MatchTargetWeightMask(mp.posweight, 0), mp.startime, mp.targettime);
        Debug.Log("Matched");
    }

    public void SetControl(bool hasControl)
    {
        if (canHaveControlls)
        {
            _hasControl = hasControl;
            _controller.enabled = hasControl;
            
            if (!hasControl)
            {

                //_animator.SetFloat(_animIDSpeed, 0);
                //_animationBlend = 0;
                TargetDir = transform.rotation.eulerAngles;
            }
        }
        

    }

    public void EnableCharacterController(bool enabled)
    {
        _controller.enabled = enabled;

    }

    public void ResetTargetRotation()
    {
        TargetDir = transform.rotation.eulerAngles;
    }
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        // _animIDGrounded = Animator.StringToHash("Grounded");
        // _animIDJump = Animator.StringToHash("Jump");
        // _animIDFreeFall = Animator.StringToHash("FreeFall");
        // _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {

        isGrounded = Physics.CheckSphere(transform.TransformPoint(GroundedOffset), GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }

        _animator.SetBool("isGrounded", isGrounded);

    }

    void LedgeMovement()
    {
        float signedangle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredTargetDir, Vector3.up);
        float angle = Mathf.Abs(signedangle);


        if (Vector3.Angle(desiredTargetDir, transform.forward) >= 80)
        {
            velocity = new Vector3(0, 0, 0);
            return;
        }
        if (angle < 60)
        {
            velocity = new Vector3(0, 0, 0);
            //TargetDir = Vector3.zero;
            // Debug.Log("woho in this fucn");
            //_input.move = Vector2.zero;
        }

        else if (angle < 90)
        {
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedangle);

            velocity = velocity.magnitude * dir;
            // TargetDir = dir;

        }
    }
    private void Move()
    {
        if (isGrounded)
        {
            targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero)
                targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;


            _speed = targetSpeed;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

                Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

            if (_input.move != Vector2.zero)
            {
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            desiredTargetDir = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            TargetDir = desiredTargetDir;
            velocity = TargetDir.normalized * (_speed * Time.deltaTime);


            isOnLedge = environmentScanner.LedgeCheck(desiredTargetDir, out LedgeData ledgedata);

            if (isOnLedge)
            {
                Debug.Log("Is On Ledge :" + isOnLedge);
                LedgeData = ledgedata;
                Debug.Log("LedgeHeight :" + LedgeData.height);
                LedgeMovement();
            }

        }

        else
        {
            
           forwardvelocity = transform.forward * _airMoveSpeed * Time.deltaTime;
            velocity = forwardvelocity +new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
           
        }
       
        if (_controller.enabled)
            _controller.Move(velocity);

        _animator.SetFloat(_animIDSpeed, _animationBlend);
    }

    private void JumpAndGravity()
    {
        if (isGrounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // // Jump
            if (_input.jump && !_inAction && !isHanging)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                //  _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity); 
                _parkourController.CheckJump();

                _input.jump = false;
            }



            // // jump timeout
            // if (_jumpTimeoutDelta >= 0.0f)
            // {
            //     _jumpTimeoutDelta -= Time.deltaTime;
            // }
        }
        else
        {
            //     // reset the jump timeout timer
            //     _jumpTimeoutDelta = JumpTimeout;

            //     // fall timeout
            //     if (_fallTimeoutDelta >= 0.0f)
            //     {
            //         _fallTimeoutDelta -= Time.deltaTime;
            //     }


            //     // if we are not grounded, do not jump

        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)


    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(transform.TransformPoint(GroundedOffset), GroundedRadius);
    }



}

public class MatchTargetParams
{
    public Vector3 pos;
    public AvatarTarget bodypart;
    public Vector3 posweight;
    public float startime;
    public float targettime;
}
