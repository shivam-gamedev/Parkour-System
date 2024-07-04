using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header("Obstacle Detection")]
    [SerializeField] Vector3 _forwardrayoffset = new Vector3(0, 0.25f, 0);
    [SerializeField] float _forwardRayLength = 0.8f;
    [SerializeField] LayerMask ObstacleLayer;
    [SerializeField] float _heightRayLength = 5.0f;

    [Header("Ledge Detections")]
    [SerializeField] float _ledgeRayLength = 5.0f;
    [SerializeField] float LedgeHeight = 0;
    [SerializeField] float offsetOrigin = 0.5f;
    [SerializeField] float ledgeHeightThreshold = 2f;
    [SerializeField] float RaysSpacing = 0.25f;
    [SerializeField] float SidesOffset = 0.5f;

    [Header("ClimbLedges Detections")]
    [SerializeField] int NumberofRays = 25;
    [SerializeField] float climbLedgeRayLength;
    [SerializeField] float climbLedgeRayStartOffset = 3.5f;
    [SerializeField] Vector3 climbLedgeRayOffset;
    [SerializeField] LayerMask ClimbLedgeLayer;
    [Header("DropLedges Detections")]
    [SerializeField] float _dropLedgeOriginOffset;
    [SerializeField] float _dropLedgeRayLength;

    [Header("For Debug")]
    [SerializeField] float angle = 0;
    public Vector3 surfaceRayOrigin;
    public float DistanceBetween_Rays = 0;

    private void Update()
    {
        ObstacleCheck();

    }
    public ObstacleHitData ObstacleCheck()
    {
        var hitdata = new ObstacleHitData();
        var forwardOrigin = transform.position + _forwardrayoffset;
        hitdata.forward_HitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitdata.forwardhit,
                                                     _forwardRayLength, ObstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * _forwardRayLength,
                        (hitdata.forward_HitFound) ? Color.green : Color.red);

        if (hitdata.forward_HitFound)
        {
            var heightOrigin = hitdata.forwardhit.point + Vector3.up * _heightRayLength;

            hitdata.Height_HitFound = Physics.Raycast(heightOrigin, Vector3.down,
                                        out hitdata.heighthit, _heightRayLength, ObstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * _heightRayLength,
                   (hitdata.Height_HitFound) ? Color.green : Color.red);


            // Calculate the distance between the forward ray hit point and the height ray hit point
            hitdata.Distance = Vector3.Distance(forwardOrigin, heightOrigin);
            DistanceBetween_Rays = Mathf.Round(hitdata.Distance * 10f) / 10f;

        }
        return hitdata;
    }

    public bool ClimbLedgeCheck(Vector3 dir, out RaycastHit ledgehit,LayerMask ClimbLayer)
    {
        ledgehit = new RaycastHit();

        if (dir == Vector3.zero)
            return false;

        var origin = transform.position + Vector3.up * climbLedgeRayStartOffset;
        var offset = climbLedgeRayOffset;

        for (int i = 0; i < NumberofRays; i++)
        {
            Debug.DrawRay(origin + offset * i, dir* climbLedgeRayLength);
            if (Physics.Raycast(origin + offset * i, dir, out RaycastHit hit, climbLedgeRayLength, ClimbLayer))
            {
                ledgehit = hit;
                return true;

            }
        }
        return false;
    }


    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit();

        var origin = transform.position + Vector3.down * 0.1f + transform.forward * _dropLedgeOriginOffset;

        Debug.DrawRay(origin, -transform.forward *_dropLedgeRayLength, Color.red);
        
        if (Physics.Raycast(origin, -transform.forward, out RaycastHit hit, _dropLedgeRayLength, ClimbLedgeLayer))
        {
            ledgeHit = hit;
            return true;
        }

        return false;

    }
    public bool LedgeCheck(Vector3 movedir, out LedgeData ledgedata)
    {

        ledgedata = new LedgeData();
        if (movedir == Vector3.zero)
            return false;


        var origin = transform.position + movedir * offsetOrigin + Vector3.up;
        var Sidesorigin = transform.position + movedir * SidesOffset + Vector3.up;

        if (PhysicsUtil.ThreeRaycasts(origin, Sidesorigin, Vector3.down, RaysSpacing, transform,
            out List<RaycastHit> hits, _ledgeRayLength, ObstacleLayer, true))
        {

            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHeightThreshold).ToList();

            if (validHits.Count > 0)
            {
                surfaceRayOrigin = validHits[0].point;
                surfaceRayOrigin.y = transform.position.y - 0.1f;
                if (Physics.Raycast(surfaceRayOrigin, transform.position - surfaceRayOrigin, out RaycastHit surfacehit, 2, ObstacleLayer))
                {
                    Debug.DrawLine(surfaceRayOrigin, transform.position, Color.cyan);
                    LedgeHeight = transform.position.y - validHits[0].point.y;

                    ledgedata.angle = Vector3.Angle(transform.forward, surfacehit.normal);
                    angle = ledgedata.angle;

                    ledgedata.height = LedgeHeight;
                    ledgedata.surfaceHit = surfacehit;

                    return true;

                }
            }

        }
        return false;

    }


}

public struct ObstacleHitData
{
    public bool forward_HitFound;
    public bool Height_HitFound;
    public float Distance;
    public RaycastHit forwardhit;
    public RaycastHit heighthit;
}

public struct LedgeData
{
    public float height;
    public float angle;
    public RaycastHit surfaceHit;
}
