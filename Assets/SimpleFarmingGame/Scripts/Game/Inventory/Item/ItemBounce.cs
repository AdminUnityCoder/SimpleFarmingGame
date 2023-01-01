using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform m_ItemSpriteTransform;
        private BoxCollider2D m_BoxCollider2D;

        [SerializeField] private float Gravity = -3.5f;
        private float m_Distance;
        private Vector2 m_Direction;
        private Vector3 m_TargetPosition;
        private bool m_IsOnGround;

        private void Awake()
        {
            m_ItemSpriteTransform = transform.GetChild(0);
            m_BoxCollider2D = GetComponent<BoxCollider2D>();
            m_BoxCollider2D.enabled = false; // Close Trigger
        }

        private void Update()
        {
            Bounce();
        }

        public void InitBounceItem(Vector3 targetPosition, Vector2 direction)
        {
            m_BoxCollider2D.enabled = false;
            m_Direction = direction;
            m_TargetPosition = targetPosition;
            m_Distance = Vector3.Distance(m_TargetPosition, transform.position);

            m_ItemSpriteTransform.position += Vector3.up * 1.5f;
        }

        private void Bounce()
        {
            m_IsOnGround = m_ItemSpriteTransform.position.y <= transform.position.y;

            if (Vector3.Distance(transform.position, m_TargetPosition) > 0.1f)
            {
                transform.position += (Vector3)m_Direction * (m_Distance * -Gravity * Time.deltaTime);
            }

            if (!m_IsOnGround)
            {
                m_ItemSpriteTransform.position += Vector3.up * (Gravity * Time.deltaTime);
            }
            else
            {
                m_ItemSpriteTransform.position = transform.position;
                m_BoxCollider2D.enabled = true; // Open trigger
            }
        }
    }
}