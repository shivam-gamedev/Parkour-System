using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New Parkour action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] string _animName;
    [SerializeField] float _minHeight;
    [SerializeField] float _maxHeight;
    
    [Header("ParkourActions Minimum Distance")]
    [SerializeField] float _minDist;
    [SerializeField] float _maxDist;
    [SerializeField] string _obstacleTag;

    [SerializeField] bool _rotateToObstacle;
    [SerializeField] float _postActionDelay;

    [Header("Target Matching")]
    [SerializeField] bool enableTargetMatching;
    [SerializeField] bool ledgeTargetMatching;
    [SerializeField] protected AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchPosWeight = new Vector3(0,1,0);

    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPos {get; set;}
    public bool Mirror{get;set;}

    public virtual bool CheckIfPossible(ObstacleHitData hitdata, Transform player)
    {
        if(!string.IsNullOrEmpty(_obstacleTag) && hitdata.forwardhit.transform.tag != _obstacleTag)
            return false;
      

        float height = hitdata.heighthit.point.y - player.position.y;
         float Distance = Mathf.Round( hitdata.Distance * 10f) / 10f;
        
        //  Debug.Log("height.y of ledge : " + hitdata.heighthit.point.y);
        //  Debug.Log("hitdata of ledge : " + hitdata.heighthit.point);
        //  Debug.Log("playerheight.y of player : " + player.position.y);
        //  Debug.Log("final height of ledge : " + height);
        if (height < _minHeight || height > _maxHeight)
            return false;

        if(Distance < _minDist || Distance >_maxDist)
            return false;

        if (_rotateToObstacle)
        {
            TargetRotation = Quaternion.LookRotation(-hitdata.forwardhit.normal);
        }

        if(enableTargetMatching)
        {
            MatchPos = hitdata.heighthit.point;
        }

        return true;
    }

    public string Animname => _animName;

    public float PostActionDelay =>_postActionDelay;
    public bool RotateToObstacle => _rotateToObstacle;

    public bool EnableTargetMatching => enableTargetMatching;
    public bool LedgeTargetMatching => ledgeTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;

    public Vector3 MatchPosWeight => matchPosWeight;


}
