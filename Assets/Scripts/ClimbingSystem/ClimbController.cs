using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class NeighbourAnimationData
{
    public string animationName;
    public Vector2 direction;
    public Vector3 anim_offset = new Vector3();

    public float matchStartTime, matchTargetTime;

}
public class ClimbController : MonoBehaviour
{
    EnvironmentScanner envScanner;
    PlayerMovement playercontroller;
    ClimbPoint currentPoint;

    ParkourController parkourcontroller;
    public List<NeighbourAnimationData> neighbourAnimations;
    public float forwardOffset = 0.1f;
    public float upOffset = 0.1f;
    public float rightOffset = 0.25f;
    [SerializeField] LayerMask LedgeLayer;
    [SerializeField] LayerMask SwingLayer;
    [SerializeField] ParkourAction Swing;

    [Header("Debug Values")]
    [SerializeField] float tolerance = 5f;
    [SerializeField] float _MinSwingDistance = 5f;
    [SerializeField] float _MaxSwingDistance = 5f;
    private void Awake()
    {
        envScanner = GetComponent<EnvironmentScanner>();
        playercontroller = GetComponent<PlayerMovement>();
        parkourcontroller = GetComponent<ParkourController>();
    }
    private void Update()
    {
        if (!playercontroller.isHanging)
        {
            if (Input.GetButton("Jump") && !playercontroller._inAction)
            {
                if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit, LedgeLayer))
                {
                    if (ledgeHit.distance < 2)
                    {
                        Debug.Log("In this fucn");
                        currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                        playercontroller.SetControl(false);
                        StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.41f, 0.54f));
                    }
                }
                if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit RodHit, SwingLayer))
                {
                    if (RodHit.distance  >_MaxSwingDistance || RodHit.distance < _MinSwingDistance)
                    { 
                        return;
                    }
                    else
                    {
                        Debug.Log(RodHit.distance);
                        playercontroller.SetControl(false);
                        StartCoroutine(JumpToSwing("Swing", RodHit.transform, 0.245f, 0.313f, RodHit));
                    }
                }
            }
            if (Input.GetButton("Drop") && !playercontroller._inAction)
            {
                if (envScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {

                    Debug.Log(ledgeHit.transform.name);
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    playercontroller.SetControl(false);
                    StartCoroutine(JumpToLedge("DropTohang", currentPoint.transform, 0.30f, 0.45f, handOffsetValue: new Vector3(-0.25f, 0.3f, -0.19f)));
                }
            }

        }
        else
        {
            if (Input.GetButton("Drop") && !playercontroller._inAction)
            {

                StartCoroutine(JumpFromHang("JumpFromHang"));
                return;
            }

            float h = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float v = Mathf.Round(Input.GetAxisRaw("Vertical"));

            var Inputdir = new Vector2(h, v);

            if (playercontroller._inAction || Inputdir == Vector2.zero) return;

            //Mount From Hanging

            if (currentPoint.MountPoint && Inputdir.y == 1)
            {
                StartCoroutine(MountFromHanging());
                //  parkourcontroller.MountFromHanging();
                return;
            }



            var neighbour = currentPoint.GetNeibour(Inputdir);
            if (neighbour == null)
            {
                if (Inputdir.y == -1 && Input.GetButton("Jump"))

                    StartCoroutine(JumpFromHang("HangToStanding"));
                return;
            }



            //*********************************************************************************************************
            if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                currentPoint = neighbour.Point;

                NeighbourAnimationData animationData = GetAnimationData(neighbour.direction);
                if (animationData != null)
                {
                    StartCoroutine(JumpToLedge(animationData.animationName, currentPoint.transform, animationData.matchStartTime, animationData.matchTargetTime
                    , handOffsetValue: animationData.anim_offset));
                }
                else
                {
                    Debug.Log("Drop to ground");
                }

            }

            else if (neighbour.connectionType == ConnectionType.Move && Input.GetButton("Jump"))
            {
                currentPoint = neighbour.Point;

                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handOffsetValue: new Vector3(-0.3f, 0, -0.15f)));
                if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffsetValue: new Vector3(-0.3f, 0, -0.15f)));
            }

        }
    }
    NeighbourAnimationData GetAnimationData(Vector2 direction)
    {
        foreach (NeighbourAnimationData animationData in neighbourAnimations)
        {
            if (animationData.direction == direction)
            {
                return animationData;
            }
        }
        return null;
    }
    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime,
    AvatarTarget hand = AvatarTarget.RightHand, Vector3? handOffsetValue = null)
    {
        var matchParams = new MatchTargetParams()
        {
            pos = GetHandPos(ledge, hand, handOffsetValue),
            bodypart = hand,
            startime = matchStartTime,
            targettime = matchTargetTime,
            posweight = Vector3.one,
        };

        var targetRot = Quaternion.LookRotation(ledge.forward);

        yield return playercontroller.DoAction(anim, matchParams, targetRot, true);
        playercontroller.isHanging = true;

    }
    IEnumerator JumpToSwing(string anim, Transform ledge, float matchStartTime, float matchTargetTime, RaycastHit rodhit,
    AvatarTarget hand = AvatarTarget.RightHand, Vector3? handOffsetValue = null)
    {
        var matchParams = new MatchTargetParams()
        {
            pos = ledge.transform.position + new Vector3(0, 0, -0.14f),
            bodypart = hand,
            startime = matchStartTime,
            targettime = matchTargetTime,
            posweight = Vector3.one,
        };

        Vector3 playerForwardDirection = transform.forward;
        Vector3 ledgeForwardDirection = ledge.forward;

        // Calculate the angle between the two vectors in degrees.
        float angle = Vector3.Angle(playerForwardDirection, ledgeForwardDirection);

        // Check if the angle is close to 180 degrees (opposite direction).
        // You can adjust the tolerance value as needed.
        bool isFacingOppositeDirection = Mathf.Abs(angle - 180f) < tolerance;
        Quaternion targetRot;
        if (isFacingOppositeDirection)
        {
            // If the player is facing the opposite direction, rotate 'myObject' to face 'ledge.forward'.
           targetRot = Quaternion.LookRotation(-ledgeForwardDirection);
           
        }
        else
        {

         targetRot = Quaternion.LookRotation(ledge.forward);
        }
       
        yield return playercontroller.DoAction(anim, matchParams, targetRot, true);

        playercontroller.SetControl(true);

    }


    Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handoffset)
    {
        var Offval = (handoffset != null) ? handoffset.Value : new Vector3(-0.3f, 0, -0.14f);

        var hDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * Offval.z + Vector3.up * Offval.y - hDir * Offval.x;
    }

    IEnumerator JumpFromHang(string animname)
    {
        playercontroller.isHanging = false;
        yield return playercontroller.DoAction(animname);
        playercontroller.ResetTargetRotation();
        playercontroller.SetControl(true);

    }

    IEnumerator MountFromHanging()
    {
        playercontroller.isHanging = false;
        yield return playercontroller.DoAction("ClimbFromHang");


        playercontroller.EnableCharacterController(true);
        yield return new WaitForSeconds(.5f);
        playercontroller.ResetTargetRotation();
        playercontroller.SetControl(true);
    }

    ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        var Points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null;
        float nearestPointDistance = Mathf.Infinity;

        foreach (var point in Points)
        {
            float distance = Vector3.Distance(point.transform.position, hitPoint);

            if (distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }
        return nearestPoint;
    }
}
