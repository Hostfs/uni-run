using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int healPercentage = 10; // 최대 체력의 몇 퍼센트를 회복시킬지
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌 시 체력 회복 처리
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 최대 체력의 healPercentage% 만큼 회복
                int healAmount = Mathf.RoundToInt(player.maxHealth * (healPercentage / 100f));
                player.HealPlayer(healAmount);
                
                Debug.Log($"플레이어가 HP 물약을 획득: 회복량={healAmount}");
                
                // 물약 제거
                Destroy(gameObject);
            }
        }
    }
}