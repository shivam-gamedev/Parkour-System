using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{

    EnvironmentScanner environmentScanner;
    Animator _animator;
    PlayerMovement _playerMovement;

    [SerializeField] List<ParkourAction> parkourActions;

    [SerializeField] ParkourAction Swing;
    [SerializeField] ParkourAction jumpDown, RollLanding, MountfromHang, JumpDown_HighJump;
    [SerializeField] float JumpingHeight_Threshold = 2f;
    public bool Jumped;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
    }


    private void Update()
    {
        if (!_playerMovement.isHanging && _playerMovement.isOnLedge
            && !_playerMovement._inAction && Input.GetKeyDown(KeyCode.Space))
        {

            // if( )de
            // {

            Debug.Log("Jumping of ledge");
            // _playerMovement._input.jump = false;

            if (_playerMovement.LedgeData.height < JumpingHeight_Threshold)
            {
                _animator.SetBool("isHighJump", false);
                StartCoroutine(DoParkourAction(jumpDown));
            }
            else
            {

                _animator.SetBool("isHighJump", true);
                //_playerMovement._airMoveSpeed = 5f;
                StartCoroutine(DoParkourAction(JumpDown_HighJump));

            }

            _playerMovement.isOnLedge = false;
            // }
        }


    }

    [SerializeField] LayerMask SwingLayer;
    public void CheckJump()
    {
        var hitdata = environmentScanner.ObstacleCheck();
        if (hitdata.forward_HitFound)
        {
            foreach (var action in parkourActions)
            {

                if (action.CheckIfPossible(hitdata, transform))
                {
                    StartCoroutine(DoParkourAction(action));
                    break;
                }
            }
        }
    }

    public void DoAdvancedAction(ParkourAction action)
    {
        StartCoroutine(DoParkourAction(action));
    }
    IEnumerator DoParkourAction(ParkourAction action)
    {
        _playerMovement.SetControl(false);

        MatchTargetParams matchparams = null;
        if (action.EnableTargetMatching)
        {
            matchparams = new MatchTargetParams()
            {
                pos = action.MatchPos,
                bodypart = action.MatchBodyPart,
                posweight = action.MatchPosWeight,
                startime = action.MatchStartTime,
                targettime = action.MatchTargetTime

            };
        }
        yield return _playerMovement.DoAction(action.Animname, matchparams, action.TargetRotation, action.RotateToObstacle, action.PostActionDelay, action.Mirror);

        //      Debug.Log(matchparams.pos);
        _playerMovement.SetControl(true);

    }

    public void MountFromHanging()
    {
        _playerMovement.isHanging = false;
        StartCoroutine(DoParkourAction(MountfromHang));
    }

    void MatchTarget(ParkourAction action)
    {
        if (_animator.isMatchingTarget) return;
        _animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime, action.MatchTargetTime);
        Debug.Log("Matched");
    }
    void LedgeMatchTarget(ParkourAction action)
    {
        if (_animator.isMatchingTarget) return;

        _animator.MatchTarget(environmentScanner.surfaceRayOrigin, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime, action.MatchTargetTime);

    }
}
