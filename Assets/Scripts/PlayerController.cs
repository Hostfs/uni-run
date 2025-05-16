using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance; // 싱글톤 인스턴스

    public AudioClip deathClip; // 사망 시 재생할 오디오 클립
    public AudioClip hurtClip; // 피격 시 재생할 오디오 클립
    public AudioClip healClip; // 회복 시 재생할 오디오 클립 (추가)
    public float jumpForce = 700f; // 점프 힘

    [Header("체력 시스템")]
    public int maxHealth = 100; // 최대 체력
    public int currentHealth; // 현재 체력
    public int damageAmount = 10; // 장애물 충돌 시 데미지 (10%)
    public Slider healthSlider; // 체력 UI 슬라이더

    private int jumpCount = 0; // 누적 점프 횟수
    private bool isGrounded = false; // 바닥에 닿았는지
    private bool isDead = false; // 사망 상태
    private bool isInvincible = false; // 무적 상태
    private float invincibleTime = 1.0f; // 무적 시간 (초)

    private Rigidbody2D playerRigidbody;
    private Animator animator;
    private AudioSource playerAudio;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 PlayerController가 존재합니다!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Rigidbody2D 설정 최적화
        playerRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 충돌 누락 방지
        
        // 게임 시작 전에는 물리 시뮬레이션 비활성화
        playerRigidbody.simulated = false;

        // 체력 초기화
        currentHealth = maxHealth;
        UpdateHealthBar();

        // Slider 설정: 사용자 입력 방지, 핸들 비활성화
        if (healthSlider != null)
        {
            healthSlider.interactable = false; // 드래그 비활성화
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f; // 초기값 강제 설정
            if (healthSlider.handleRect != null)
            {
                healthSlider.handleRect.gameObject.SetActive(false); // 핸들 제거
                Debug.Log("Slider 핸들 비활성화");
            }
            Debug.Log($"Slider 초기화: value={healthSlider.value}, min={healthSlider.minValue}, max={healthSlider.maxValue}");
        }
    }

    // 게임 시작 함수
    public void StartGame()
    {
        isDead = false;
        isInvincible = false;
        
        // 체력 초기화
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // 물리 시뮬레이션 활성화
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = true;
        }
        
        Debug.Log("플레이어 게임 시작");
    }

    private void Update()
    {
        // 게임이 시작되지 않았거나 사망 상태라면 입력 처리 안함
        if (!GameManager.instance.isGameStarted || isDead)
        {
            return;
        }

        // 점프 입력 처리
        if (Input.GetMouseButtonDown(0) && jumpCount < 2)
        {
            jumpCount++;
            playerRigidbody.velocity = Vector2.zero;
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            playerAudio.Play();
        }
        else if (Input.GetMouseButtonUp(0) && playerRigidbody.velocity.y > 0)
        {
            playerRigidbody.velocity = playerRigidbody.velocity * 0.5f;
        }

        // Slider 값 주기적 동기화
        if (healthSlider != null && Mathf.Abs(healthSlider.value - (float)currentHealth / maxHealth) > 0.0001f)
        {
            UpdateHealthBar();
            Debug.Log($"Slider 값 복원: {healthSlider.value} -> {(float)currentHealth / maxHealth}");
        }

        animator.SetBool("Grounded", isGrounded);
    }

    private void LateUpdate()
    {
        // 게임이 시작되지 않았다면 건너뛰기
        if (!GameManager.instance.isGameStarted)
        {
            return;
        }
            
        // Slider 값 재확인
        if (healthSlider != null && Mathf.Abs(healthSlider.value - (float)currentHealth / maxHealth) > 0.0001f)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
            Debug.Log($"LateUpdate: Slider 값 강제 복원: {healthSlider.value} -> {(float)currentHealth / maxHealth}");
            Canvas.ForceUpdateCanvases(); // UI 강제 갱신
        }
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
            Debug.Log($"UpdateHealthBar: Slider value set to {healthSlider.value}");
        }

        // 게임 매니저의 체력바 업데이트 함수도 호출
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateHealthBar((float)currentHealth / maxHealth);
        }
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // 깜빡임 효과
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            yield return new WaitForSeconds(invincibleTime / 10);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(invincibleTime / 10);
        }

        isInvincible = false;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        UpdateHealthBar();

        if (hurtClip != null)
        {
            playerAudio.clip = hurtClip;
            playerAudio.Play();
        }

        StartCoroutine(InvincibilityCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 외부에서 호출 가능한 회복 함수
    public void HealPlayer(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // 최대 체력 초과 방지
        }
        
        UpdateHealthBar();
        
        // 회복 소리 재생
        if (healClip != null)
        {
            AudioSource.PlayClipAtPoint(healClip, transform.position);
        }
        
        Debug.Log($"HP 회복: {healAmount}, 현재 체력: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        animator.SetTrigger("Die");
        playerAudio.clip = deathClip;
        playerAudio.Play();
        playerRigidbody.velocity = Vector2.zero;
        isDead = true;
        GameManager.instance.OnPlayerDead();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 게임이 시작되지 않았다면 충돌 무시
        if (!GameManager.instance.isGameStarted)
        {
            return;
        }
            
        // 사망 지점과 충돌 시
        if (other.CompareTag("Dead") && !isDead)
        {
            Debug.Log($"사망 지점 충돌: {other.gameObject.name}, 위치: {other.transform.position}");
            Die();
            return;
        }

        // 장애물과의 트리거 충돌
        if (other.CompareTag("Obstacle") && !isDead)
        {
            Collider2D playerCollider = GetComponent<Collider2D>();
            Debug.Log($"장애물 충돌: {other.gameObject.name}, 플레이어 위치: {transform.position}, 장애물 위치: {other.transform.position}, 플레이어 콜라이더: {playerCollider.bounds}, 장애물 콜라이더: {other.bounds}");
            Debug.DrawRay(transform.position, (other.transform.position - transform.position).normalized * 2f, Color.red, 2f); // 충돌 방향 시각화
            TakeDamage(damageAmount);
        }
        
        // HealthPotion 클래스를 사용하므로 여기서는 물약 획득 처리 제거
        // 대신 HealthPotion 스크립트의 OnTriggerEnter2D에서 처리
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 게임이 시작되지 않았다면 충돌 무시
        if (!GameManager.instance.isGameStarted)
        {
            return;
        }
            
        // 바닥 감지 (점프 리셋용)
        if (collision.contacts[0].normal.y > 0.7)
        {
            isGrounded = true;
            jumpCount = 0;
            Debug.Log("바닥에 착지함");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 게임이 시작되지 않았다면 무시
        if (!GameManager.instance.isGameStarted)
        {
            return;
        }
            
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            isGrounded = false;
            Debug.Log("바닥을 떠남");
        }
    }
}