using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Custom Action/New Vault action")]
public class VaultAction : ParkourAction
{
    
    public override bool CheckIfPossible(ObstacleHitData hitdata, Transform player)
    {
       if(! base.CheckIfPossible(hitdata, player))
        return false;

        var hitpoint = hitdata.forwardhit.transform.InverseTransformPoint(hitdata.forwardhit.point);

        if(hitpoint.z <0 && hitpoint.x <0 || hitpoint.z >0 && hitpoint.x >0)
        {
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;
        }
        else
        {
            Mirror = false;
              matchBodyPart = AvatarTarget.LeftHand;
        }

        return true;
    } 
}
