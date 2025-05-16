using UnityEngine;

public class Platform : MonoBehaviour
{
    public GameObject[] obstacles; // 장애물 오브젝트들
    private bool stepped = false; // 플레이어 캐릭터가 밟았었는가

    private void OnEnable()
    {
        stepped = false;

        for (int i = 0; i < obstacles.Length; i++)
        {
            if (Random.Range(0, 3) == 0)
            {
                obstacles[i].SetActive(true);

                // 콜라이더 점검
                BoxCollider2D collider = obstacles[i].GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                    Debug.Log($"장애물 {obstacles[i].name} 활성화: 콜라이더 크기={collider.size}, 오프셋={collider.offset}, 트리거={collider.isTrigger}, 태그={obstacles[i].tag}, 레이어={obstacles[i].layer}");
                }
                else
                {
                    collider = obstacles[i].AddComponent<BoxCollider2D>();
                    collider.isTrigger = true;
                    collider.size = new Vector2(1.5f, 2f);
                    Debug.Log($"장애물 {obstacles[i].name}에 트리거 콜라이더 추가됨");
                }
            }
            else
            {
                obstacles[i].SetActive(false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어가 플랫폼을 밟았을 때 점수 추가
        if (collision.gameObject.CompareTag("Player") && !stepped)
        {
            stepped = true;
            GameManager.instance.AddScore(1);
        }
    }
}