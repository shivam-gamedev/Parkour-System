using UnityEngine;

public class MotionMatching : MonoBehaviour
{
    private Animator animator;
    public AnimationClip[] motionClips;
    public float speedThreshold = 0.1f;
    public float timeThreshold = 0.1f;
    private int currentClipIndex;
    private float blendTimer;
    private CharacterController characterController;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        currentClipIndex = 0;
        blendTimer = 0f;
    }

    private void Update()
    {
        // Calculate the character's current movement speed
        float speed = characterController.velocity.magnitude;

        // Check if the character's speed is above the threshold for motion matching
        if (speed > speedThreshold)
        {
            // Increment the blend timer
            blendTimer += Time.deltaTime;

            // Check if it's time to blend to the next motion clip
            if (blendTimer >= timeThreshold)
            {
                // Transition to the next motion clip
                TransitionToNextClip();
            }
        }
        else
        {
            // Reset the blend timer
            blendTimer = 0f;

            // If the character is not moving, play the idle animation
            animator.CrossFade("Idle", 0.2f);
        }
    }

    private void TransitionToNextClip()
    {
        // Increment the current clip index and loop back to 0 if needed
        currentClipIndex = (currentClipIndex + 1) % motionClips.Length;

        // Crossfade to the new motion clip
        animator.CrossFade(motionClips[currentClipIndex].name, 0.2f);

        // Reset the blend timer
        blendTimer = 0f;
    }
}
