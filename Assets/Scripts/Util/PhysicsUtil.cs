using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil
{
    public static bool ThreeRaycasts(Vector3 origin,Vector3 SidesOrigin, Vector3 dir, float spacing, Transform transform,
     out List<RaycastHit> hits, float distance, LayerMask layer, bool debugDraw = false)
    {
        bool centerHitFound = Physics.Raycast(origin, Vector3.down, out RaycastHit centerHit, distance, layer);
        bool leftHitFound = Physics.Raycast(SidesOrigin - transform.right * spacing, Vector3.down, out RaycastHit lefthit, distance, layer);
        bool RightHitFound = Physics.Raycast(SidesOrigin + transform.right * spacing, Vector3.down, out RaycastHit Righthit, distance, layer);

        hits = new List<RaycastHit>() { centerHit, lefthit, Righthit };

        bool hitfound = centerHitFound || leftHitFound || RightHitFound;

        if (hitfound && debugDraw)
        {
            Debug.DrawLine(origin, centerHit.point, Color.red);
            Debug.DrawLine(SidesOrigin - transform.right * spacing, lefthit.point, Color.red);
            Debug.DrawLine(SidesOrigin + transform.right * spacing, Righthit.point, Color.red);
        }

        return hitfound;

    }
}
