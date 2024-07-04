using System.Collections.Generic;
using UnityEngine;

public class AnimationOffsetManager : MonoBehaviour
{
    public Transform ledge;
    public List<AnimationOffsetData> animationOffsets;

    private Dictionary<AnimationClip, Dictionary<AvatarTarget, Vector3>> animationOffsetDictionary;

    private void Awake()
    {
        animationOffsetDictionary = new Dictionary<AnimationClip, Dictionary<AvatarTarget, Vector3>>();

        // Populate the dictionary with animation offsets
        foreach (AnimationOffsetData offsetData in animationOffsets)
        {
            if (offsetData.animationClip != null)
            {
                if (!animationOffsetDictionary.ContainsKey(offsetData.animationClip))
                {
                    animationOffsetDictionary.Add(offsetData.animationClip, new Dictionary<AvatarTarget, Vector3>());
                }

                var offsetDictionary = animationOffsetDictionary[offsetData.animationClip];
                if (!offsetDictionary.ContainsKey(offsetData.hand))
                {
                    offsetDictionary.Add(offsetData.hand, offsetData.offset);
                }
            }
        }
    }

    public Vector3 GetHandPos(Transform ledge,AnimationClip animationClip, AvatarTarget hand)
    {
        Vector3 offset = new Vector3(-0.3f, 0f, -0.14f);

        if (animationOffsetDictionary.ContainsKey(animationClip))
        {
            var offsetDictionary = animationOffsetDictionary[animationClip];
            if (offsetDictionary.ContainsKey(hand))
            {
                offset = offsetDictionary[hand];
            }
        }

        Vector3 hDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offset.z + Vector3.up * offset.y - hDir * offset.x;
    }
}

[System.Serializable]
public class AnimationOffsetData
{
    public AnimationClip animationClip;
    public AvatarTarget hand;
    public Vector3 offset;
}
