using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public GameObject platformPrefab; // 생성할 발판의 원본 프리팹
    public int count = 3; // 생성할 발판의 개수
    
    [Header("발판 생성 타이밍")]
    public float timeBetSpawnMin = 1.25f; // 다음 발판 배치까지의 시간 간격 최솟값
    public float timeBetSpawnMax = 2.25f; // 다음 발판 배치까지의 시간 간격 최댓값
    
    [Header("발판 배치 위치")]
    public float yMin = -3.5f; // 배치할 위치의 최소 y값
    public float yMax = 1.5f; // 배치할 위치의 최대 y값
    private float xPos = 20f; // 배치할 위치의 x 값
    
    [Header("HP 물약 설정")]
    public GameObject healthPotionPrefab; // HP 물약 프리팹
    public float potionSpawnChance = 0.3f; // HP 물약 생성 확률 (0-1)

    private GameObject[] platforms; // 미리 생성한 발판들
    private int currentPlatformIndex = 0; // 사용할 현재 순번의 발판
    
    private float lastSpawnTime; // 마지막 발판 배치 시점
    private float timeBetSpawn; // 다음 발판 배치까지의 시간 간격

    private Vector2 poolPosition = new Vector2(0, -25); // 초반에 생성된 오브젝트들을 숨길 위치

    void Start()
    {
        // 발판 풀 초기화
        platforms = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            platforms[i] = Instantiate(platformPrefab, poolPosition, Quaternion.identity);
        }

        // 초기화
        lastSpawnTime = 0f;
        timeBetSpawn = 0f;
    }

    void Update()
    {
        // 게임이 시작되지 않았거나 게임오버 상태면 실행하지 않음
        if (!GameManager.instance.isGameStarted || GameManager.instance.isGameover)
        {
            return;
        }

        // 발판 배치
        if (Time.time >= lastSpawnTime + timeBetSpawn)
        {
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

            // 발판 위치 설정
            float yPos = Random.Range(yMin, yMax);
            Vector2 platformPosition = new Vector2(xPos, yPos);

            // 현재 순번의 발판을 비활성화하고 재활성화
            GameObject currentPlatform = platforms[currentPlatformIndex];
            
            // 먼저 해당 발판의 모든 HP 물약 오브젝트 비활성화 (이전에 추가된 것이 있다면)
            Transform potionTransform = currentPlatform.transform.Find("HealthPotion(Clone)");
            if (potionTransform != null)
            {
                Destroy(potionTransform.gameObject);
            }
            
            // 발판 재활성화
            currentPlatform.SetActive(false);
            currentPlatform.transform.position = platformPosition;
            currentPlatform.SetActive(true);
            
            // Platform 스크립트 접근해서 장애물 활성화 처리 확인
            Platform platformScript = currentPlatform.GetComponent<Platform>();
            if (platformScript != null)
            {
                // Platform 스크립트의 OnEnable이 자동으로 장애물 활성화 처리함
                Debug.Log($"발판 {currentPlatform.name} 활성화 - 장애물 랜덤 배치됨");
            }
            
            // HP 물약 생성 (랜덤 확률로)
            if (Random.value < potionSpawnChance && healthPotionPrefab != null)
            {
                SpawnHealthPotion(currentPlatform);
            }

            // 다음 순번으로
            currentPlatformIndex = (currentPlatformIndex + 1) % count;
        }
    }
    
    // HP 물약 스폰 메서드
    private void SpawnHealthPotion(GameObject platform)
    {
        if (platform == null || healthPotionPrefab == null) return;
        
        // 발판 위의 안전한 위치 계산
        Vector3 spawnPosition = platform.transform.position + new Vector3(0f, 1.5f, 0f);
        
        // HP 물약 생성
        GameObject potion = Instantiate(healthPotionPrefab, spawnPosition, Quaternion.identity);
        potion.transform.SetParent(platform.transform); // 발판의 자식으로 설정
        
        // 태그 및 콜라이더 설정
        potion.tag = "HealthPotion";
        BoxCollider2D collider = potion.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = potion.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
        collider.size = new Vector2(0.5f, 0.5f);
        
        Debug.Log($"HP 물약 생성됨: 발판={platform.name}, 위치={spawnPosition}");
    }

    // platforms 배열에 접근할 수 있도록 메서드
    public GameObject[] GetPlatforms()
    {
        return platforms;
    }
}