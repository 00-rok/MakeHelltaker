using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // 코루틴 사용을 위해 필요

// 스테이지별 설정을 위한 구조체
[System.Serializable]
public struct StageSettings
{
    public int stageNumber;
    public int maxHealth;
    public string sceneName; // 레벨 씬 이름
}

public class GameManager : MonoBehaviour
{
    // ⭐ 씬 로드 순서: 이 GameManager가 포함된 씬이 가장 먼저 로드되어야 합니다.
    public static GameManager Instance;

    public GameObject targetObject;
    public GameObject global_UI;

    public int currentHealth;
    public int maxHealth;

    private int currentStageIndex = 0; // 현재 플레이 중인 스테이지 인덱스

    [SerializeField] private StageSettings[] stageData;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI stageText;

    // 현재 로드된 레벨 씬의 이름 (재시작 및 전환 시 사용)
    private string currentlyLoadedLevelScene = "";

    void Awake()
    {
        // ⭐ DontDestroyOnLoad 없이 Instance 설정
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 초기 UI 상태 설정
        targetObject.SetActive(false);
        global_UI.SetActive(false);

        // 게임 시작 시, 첫 번째 스테이지 씬을 Additive 모드로 로드합니다.
        if (stageData.Length > 0)
        {
            LoadStageScene(currentStageIndex);
        }
        else
        {
            Debug.LogError("[GameManager] Stage Data is empty!");
        }
    }

    // 새 스테이지 씬을 추가(Additive) 로드하는 메서드
    private void LoadStageScene(int index)
    {
        if (index >= stageData.Length)
        {
            Debug.LogWarning("[GameManager] All stages cleared!");
            return;
        }

        StageSettings settings = stageData[index];
        string nextSceneName = settings.sceneName;

        currentlyLoadedLevelScene = nextSceneName;

        // Additive 모드로 씬 로드 (현재 GameManager 씬 유지)
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Additive);

        Debug.Log($"[GameManager] STAGE {settings.stageNumber} Scene ({nextSceneName}) loaded Additively.");
    }

    // 씬 로드 완료 시 호출 (새로운 레벨이 로드될 때마다)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Additive 모드로 로드된 레벨 씬일 때만 초기화
        if (mode == LoadSceneMode.Additive && scene.name == currentlyLoadedLevelScene)
        {
            int index = System.Array.FindIndex(stageData, s => s.sceneName == scene.name);
            if (index != -1)
            {
                InitializeStageSettings(stageData[index]);
            }
        }
    }

    // 씬 로드 후 스테이지 설정 초기화 (체력, UI)
    private void InitializeStageSettings(StageSettings settings)
    {
        maxHealth = settings.maxHealth;
        currentHealth = maxHealth;

        Debug.Log($"[GameManager] STAGE {settings.stageNumber} initialized: Max Health {maxHealth}.");

        UpdateHealthDisplay(currentHealth, maxHealth);
        UpdateStageDisplay(settings.stageNumber);
    }

    // ⭐ [추가] Reset 버튼 등에서 호출되어 현재 레벨 씬을 재시작하는 함수
    public void Reset()
    {
        Debug.Log("Reset 함수 호출: 현재 레벨 씬을 재시작합니다.");
        // 기존에 구현된 안전한 재시작 로직을 호출합니다.
        RestartCurrentLevel();
    }

    // ⭐ [핵심] 레벨 재시작 (Player.Die() 또는 Reset()에서 호출됨)
    public void RestartCurrentLevel()
    {
        if (string.IsNullOrEmpty(currentlyLoadedLevelScene))
        {
            Debug.LogError("[GameManager] Cannot restart: Current level scene name is missing!");
            return;
        }

        string sceneToRestart = currentlyLoadedLevelScene;

        // 씬 언로드 -> 로드 과정을 안전하게 처리하는 코루틴 시작
        StartCoroutine(RestartLevelCoroutine(sceneToRestart));
    }

    // 씬 언로드 -> 로드 과정을 안전하게 처리하는 코루틴
    private IEnumerator RestartLevelCoroutine(string sceneName)
    {
        Debug.Log($"[GameManager] Unloading scene {sceneName} for restart...");

        // 1. 현재 레벨 씬 언로드
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);

        // 언로드가 완료될 때까지 대기
        if (unloadOperation != null)
        {
            while (!unloadOperation.isDone)
            {
                yield return null;
            }
        }

        // 2. 언로드 완료 후, 같은 씬을 Additive 모드로 다시 로드
        Debug.Log($"[GameManager] Reloading scene {sceneName}...");
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        // OnSceneLoaded에서 체력 초기화가 자동으로 발생합니다.
    }

    // 스테이지 클리어 시 다음 레벨로 전환
    public void GoToNextStage()
    {
        string sceneToUnload = currentlyLoadedLevelScene;
        currentStageIndex++;

        if (currentStageIndex >= stageData.Length)
        {
            Debug.LogWarning("[GameManager] All stages cleared!");
            return;
        }

        string nextSceneName = stageData[currentStageIndex].sceneName;

        // 다음 스테이지 로드 코루틴 시작
        StartCoroutine(TransitionToNextLevelCoroutine(sceneToUnload, nextSceneName));
    }

    private IEnumerator TransitionToNextLevelCoroutine(string sceneToUnload, string nextSceneName)
    {
        // 1. 현재 씬 언로드
        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            yield return SceneManager.UnloadSceneAsync(sceneToUnload);
        }

        // 2. 다음 씬 로드
        currentlyLoadedLevelScene = nextSceneName; // 다음 씬 이름을 기록
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Additive);
    }

    public void PlayerActionTaken()
    {
        currentHealth--;
        UpdateHealthDisplay(currentHealth, maxHealth);

        if (currentHealth < 0)
        {
            // LIFE가 0이 되면 플레이어에게 사망 처리 위임
            player playerScript = FindObjectOfType<player>();
            // Die() 호출 시 Player 스크립트가 GameManager.RestartCurrentLevel()을 호출함
            playerScript?.Die();
        }
    }

    public void UpdateHealthDisplay(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = current <= 0 ? "X" : current.ToString();
        }
    }

    public void UpdateStageDisplay(int stageNumber)
    {
        if (stageText != null)
        {
            // Simple Roman numeral conversion example
            stageText.text = stageNumber == 1 ? "I" : stageNumber.ToString();
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (클린업)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}