using UnityEngine;

namespace AgeOfWar.Core
{
    public class CameraController : MonoBehaviour
    {
        public float MovementSpeed = 2;

        // Update is called once per frame
        void Update()
        {
            bool Left = Input.GetKey(KeyCode.A);
            bool Right = Input.GetKey(KeyCode.D);
            if (Left && !Right)
            {
                transform.position += Vector3.right * -MovementSpeed * Time.deltaTime;
            }
            else if (Right && !Left)
            {
                transform.position += Vector3.right * MovementSpeed * Time.deltaTime;
            }
        }
    }
}