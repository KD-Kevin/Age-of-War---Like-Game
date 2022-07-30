using BitStrap;
using UnityEngine;

namespace AgeOfWar.Animation
{
    public class UnitAnimationController : MonoBehaviour
    {
        [SerializeField] protected Animator UnitAnimator;
        [SerializeField] protected UnitAnimationData AnimationData;

        public string CurrentAnimation { get; protected set; }

        public virtual void ChangeAnimation(UnitAnimationKey animationName, float AnimationSpeed = 1)
        {
            CurrentAnimation = animationName.ToString();
            UnitAnimator.speed = 1;
            UnitAnimator.Play(AnimationData.GetAnimationForKey(animationName), 0);
        }

        public UnitAnimationKey TestKey = UnitAnimationKey.Idle;
        [Button]
        public void TestSetAnimation()
        {
            ChangeAnimation(TestKey);
        }

        public float TestMovementSpeed = 0;
        [Button]
        public void TestSetMovementSpeed()
        {
            UnitAnimator.SetFloat("Speed", TestMovementSpeed);
        }

        [Button]
        public void TestJump()
        {
            UnitAnimator.SetTrigger("Jump");
        }
    }
}
