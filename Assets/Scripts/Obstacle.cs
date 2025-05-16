using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void Awake()
    {
        // 콜라이더 확인 및 트리거 설정
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            Debug.Log($"장애물 {gameObject.name}: 콜라이더 크기={collider.size}, 오프셋={collider.offset}, 트리거={collider.isTrigger}, 레이어={gameObject.layer}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}에 BoxCollider2D가 없어 추가하고 트리거로 설정합니다.");
            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.5f, 2f); // 기본 크기
        }

        // Rigidbody2D 확인
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log($"장애물 {gameObject.name}: Rigidbody2D Kinematic 설정");
        }
    }
}