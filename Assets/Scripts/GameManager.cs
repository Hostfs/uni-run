using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // 게임 상태 관리
    public bool isGameover = false;
    public bool isGameStarted = false; // 게임 시작 상태 추가
    
    // UI 오브젝트
    public Text scoreText;
    public GameObject gameoverUI;
    public GameObject startMenuUI; // 시작 메뉴 UI
    public Button startButton; // 시작 버튼
    public Button restartButton; // 재시작 버튼

    [Header("체력 UI")]
    public Slider healthSlider;
    public Image fillImage;
    public Gradient healthBarGradient;

    private int score = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }

        // 게임 시작 상태 초기화
        isGameStarted = false;
        isGameover = false;

        // 체력바 초기화
        if (healthSlider != null)
        {
            healthSlider.interactable = false;
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;

            if (healthSlider.handleRect != null)
            {
                healthSlider.handleRect.gameObject.SetActive(false);
                Debug.Log("GameManager: Slider 핸들 비활성화");
            }

            // Fill Rect 조정
            RectTransform fillRect = healthSlider.fillRect;
            if (fillRect != null)
            {
                fillRect.anchorMin = new Vector2(0f, 0f);
                fillRect.anchorMax = new Vector2(1f, 1f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                Debug.Log($"GameManager: Fill Rect 조정: anchorMin={fillRect.anchorMin}, anchorMax={fillRect.anchorMax}");
            }

            // Fill Area 조정
            Transform fillArea = healthSlider.transform.Find("Fill Area");
            if (fillArea != null)
            {
                RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
                fillAreaRect.offsetMin = Vector2.zero;
                fillAreaRect.offsetMax = Vector2.zero;
                Debug.Log("GameManager: Fill Area 크기 조정");
            }

            if (fillImage != null && healthBarGradient != null)
            {
                fillImage.color = healthBarGradient.Evaluate(1f);
            }

            Debug.Log($"GameManager: Slider 초기화: value={healthSlider.value}, bounds={healthSlider.GetComponent<RectTransform>().rect}");
            Canvas.ForceUpdateCanvases();
        }
    }

    void Start()
    {
        // 게임 시작 UI 표시
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(true);
        }
        
        // 게임오버 UI 숨기기
        if (gameoverUI != null)
        {
            gameoverUI.SetActive(false);
        }
        
        // 시작 버튼 이벤트 설정
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        
        // 재시작 버튼 이벤트 설정
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    void Update()
    {
        // 게임이 시작되지 않았거나 게임 오버 상태라면 플레이어 체력 슬라이더 업데이트 필요 없음
        if (!isGameStarted || isGameover)
        {
            return;
        }

        // Slider 값 동기화
        if (healthSlider != null && PlayerController.instance != null)
        {
            float targetValue = (float)PlayerController.instance.currentHealth / PlayerController.instance.maxHealth;
            if (Mathf.Abs(healthSlider.value - targetValue) > 0.0001f)
            {
                healthSlider.value = targetValue;
                if (fillImage != null && healthBarGradient != null)
                {
                    fillImage.color = healthBarGradient.Evaluate(targetValue);
                }
                Debug.Log($"GameManager: Slider 값 복원: {healthSlider.value} -> {targetValue}");
                Canvas.ForceUpdateCanvases();
            }
        }
    }

    public void StartGame()
    {
        // 게임 시작 처리
        isGameStarted = true;
        isGameover = false;
        
        // 시작 메뉴 UI 숨기기
        if (startMenuUI != null)
        {
            startMenuUI.SetActive(false);
        }
        
        // 점수 초기화
        score = 0;
        if (scoreText != null)
        {
            scoreText.text = "Score : 0";
        }
        
        // 플레이어 초기화
        if (PlayerController.instance != null)
        {
            PlayerController.instance.StartGame();
        }
        
        Debug.Log("게임 시작!");
    }

    public void RestartGame()
    {
        // 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore(int newScore)
    {
        if (!isGameover && isGameStarted)
        {
            score += newScore;
            scoreText.text = "Score : " + score;
        }
    }

    public void UpdateHealthBar(float healthPercent)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
            if (fillImage != null && healthBarGradient != null)
            {
                fillImage.color = healthBarGradient.Evaluate(healthPercent);
            }
            Debug.Log($"GameManager: UpdateHealthBar: Slider value set to {healthSlider.value}, Fill Rect bounds={healthSlider.fillRect.rect}");
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Debug.LogWarning("GameManager에 healthSlider가 할당되지 않았습니다.");
        }
    }

    public void OnPlayerDead()
    {
        isGameover = true;
        gameoverUI.SetActive(true);
        Debug.Log("게임 오버 UI 활성화");
    }
}