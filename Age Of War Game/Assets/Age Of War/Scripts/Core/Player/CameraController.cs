using UnityEngine;

namespace AgeOfWar.Core
{
    public class CameraController : MonoBehaviour
    {
        public float MovementSpeed = 2;
        public Vector2 Bounds = new Vector2(-20,20);

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

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, Bounds.x, Bounds.y),
                transform.position.y,
                transform.position.z
                );
        }
    }
}