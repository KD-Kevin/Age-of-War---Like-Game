using UnityEngine;

namespace AgeOfWar.Core
{
    public class CameraController : MonoBehaviour
    {
        public float MovementSpeed = 2;
        public Vector2 Bounds = new Vector2(-20,20);
        public Vector2 ZoomBounds = new Vector2(-30, 20);

        private float ZoomValue = 0;

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

            bool Forward = Input.GetKey(KeyCode.W);
            bool Back = Input.GetKey(KeyCode.S);

            if (Forward && !Back)
            {
                if (ZoomValue < ZoomBounds.y)
                {
                    transform.position += transform.forward * MovementSpeed * Time.deltaTime;
                    ZoomValue += MovementSpeed * Time.deltaTime;
                }
            }
            else if (Back && !Forward)
            {
                if (ZoomValue > ZoomBounds.x)
                {
                    transform.position += transform.forward * -MovementSpeed * Time.deltaTime;
                    ZoomValue -= MovementSpeed * Time.deltaTime;
                }
            }

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, Bounds.x, Bounds.y),
                transform.position.y,
                transform.position.z
                );
        }
    }
}