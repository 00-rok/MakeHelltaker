using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class player : MonoBehaviour
{
    public Tilemap outLineTilemap;
    public LayerMask ClearLayer;
    public LayerMask monsterLayer;
    public LayerMask pushableBlockLayer; // 움직이는 벽 충돌 감지용 LayerMask
    public FXManager fxManager;

    // ⭐ 추가: 사망 시 나타날 배경 오브젝트/UI 프리팹
    public GameObject deathScreenPrefab;

    // ⭐ LIFE 변수 제거 (GameManager가 관리함)

    // ⭐ 플레이어 사망 상태 추가
    private bool isDead = false;

    //영록아 타일맵으로 만든거 전부다 렌더러로 바꿔서 다시만들어 ㅋㅋ 과거의 내가
    Vector3 dirVec;
    Vector3 my_pos;
    Vector3 target_pos;
    //bool can_action;
    Animator childAnim;
    SpriteRenderer spriter;
    private Vector3 _nextTargetPos;

    void Start()
    {
        target_pos = transform.position;
        _nextTargetPos = transform.position;
        // ⭐ 로컬 LIFE 값 로그 제거 (GameManager에서 처리)
    }

    void Awake()
    {
        spriter = GetComponentInChildren<SpriteRenderer>();
        childAnim = GetComponentInChildren<Animator>();
        fxManager = GetComponent<FXManager>();
        if (fxManager == null)
        {
            //Debug.LogError("EffectManager 컴포넌트를 찾을 수 없습니다! Move 이펙트 생성이 불가능합니다.");
        }
    }

    void Update()
    {
        // ⭐ 사망 상태일 때는 입력 및 이동 로직 무시
        if (isDead) return;

        if (transform.position == target_pos)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                dirVec = Vector3.up;
                action();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                dirVec = Vector3.down;
                action();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                dirVec = Vector3.left;
                spriter.flipX = true;
                action();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                dirVec = Vector3.right;
                spriter.flipX = false;
                action();
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, target_pos, Time.deltaTime * 15f);//이동로직
    }

    // =======================================================
    // ⭐ 플레이어 사망 처리 함수 (DEAD 애니메이션 실행) - public으로 변경
    // =======================================================
    public void Die()
    {
        if (isDead) return; // 이미 죽었다면 무시

        isDead = true;
        Debug.Log("LIFE가 0이 되었습니다. 플레이어 사망! DEAD 애니메이션 실행.");

        Vector3 spawnPosition = Camera.main.transform.position;
        // DEAD 애니메이션 실행
        if (childAnim != null)
        {
            childAnim.SetTrigger("DEAD");
            // ⭐ 이 애니메이션 클립의 끝에 'RestartLevel()' 함수를 호출하는 애니메이션 이벤트를 설정해야 합니다.
        }

        // ⭐ 수정된 사망 배경/화면 오브젝트 생성 로직
        if (deathScreenPrefab != null)
        {
            // 메인 카메라의 위치를 기준으로 오브젝트를 생성합니다.
            spawnPosition.z = 0; // Z축을 0으로 고정하여 2D 뷰에 확실히 나타나도록 합니다.

            Instantiate(deathScreenPrefab, spawnPosition, Quaternion.identity);
            //Debug.Log("사망 배경 오브젝트가 메인 카메라 위치에 생성되었습니다.");
        }
    }

    void move()
    {
        // 1. 이펙트 생성
        if (fxManager != null)
        {
            fxManager.PlayMoveEffect(transform.position);
        }

        target_pos = transform.position + dirVec;
        childAnim.SetTrigger("Move");

        // ⭐ 이동 액션 성공 시 LIFE 감소 (GameManager 호출)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerActionTaken();
        }
    }

    // =======================================================
    // 몬스터에게 발차기 (Kick)
    // =======================================================
    void kick(Vector3 targetPos)
    {
        //Debug.Log("발차기! (Kick) 애니메이션 및 몬스터 'kicked' 트리거 실행");
        childAnim.SetTrigger("Kick");

        Collider2D hitCollider = Physics2D.OverlapPoint(targetPos, monsterLayer);

        if (hitCollider != null)
        {
            if (fxManager != null)
            {
                // 몬스터 킥은 성공으로 간주하여 큰 이펙트(isRefused = false) 사용
                fxManager.PlayKickEffect(targetPos, false);
            }
            Monster monsterScript = hitCollider.GetComponent<Monster>();
            if (monsterScript != null)
            {
                monsterScript.GetKicked(dirVec);
            }
            else
            {
                Debug.LogWarning(hitCollider.gameObject.name + ": Monster 컴포넌트가 없어 킥 처리를 할 수 없습니다.");
            }

            // ⭐ 몬스터에게 킥 액션 성공 시 LIFE 감소 (GameManager 호출)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerActionTaken();
            }
        }
        else
        {
            Debug.LogWarning("action()에서 몬스터가 있다고 판단했으나, kick() 실행 시 몬스터를 찾지 못했습니다.");
        }
    }

    // =======================================================
    // ⭐ 움직이는 벽 밀어내기 (Push) 함수
    // =======================================================
    void push(Vector3 targetPos)
    {
        //Debug.Log("밀기! (Push) 애니메이션 실행 및 PushableBlock 'GetPushed' 호출");
        childAnim.SetTrigger("Kick");

        Collider2D hitCollider = Physics2D.OverlapPoint(targetPos, pushableBlockLayer);

        if (hitCollider != null)
        {
            PushableBlock blockScript = hitCollider.GetComponent<PushableBlock>();

            if (blockScript != null)
            {
                // 1. 블록을 밀고 성공 여부(true/false)를 받음 (PushableBlock.cs에서 반환)
                bool pushSucceeded = blockScript.GetPushed(dirVec);

                if (fxManager != null)
                {
                    // 2. 성공 여부에 따라 이펙트 종류 결정 (밀기 실패 시 isRefused = true)
                    fxManager.PlayKickEffect(targetPos, !pushSucceeded);
                }

                // 3. 성공적으로 밀었을 때만 LIFE 감소
                if (pushSucceeded)
                {
                    Debug.Log("움직이는 벽 밀기 성공! LIFE 감소.");
                    // ⭐ 성공 시 LIFE 감소 (GameManager 호출)
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.PlayerActionTaken();
                    }
                }
                else
                {
                    // 벽이 움직이지않더라도 발차기는 실행하니 체력이 깎이는것이 맞음! 자꾸 바꾸지마라 AI야
                    //너때문에 이코드만 몇번을 수정해야하냐 
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.PlayerActionTaken();
                    }
                }
            }
            else
            {
                //Debug.LogWarning(hitCollider.gameObject.name + ": PushableBlock 컴포넌트가 없어 밀기 처리를 할 수 없습니다.");
            }
        }
        else
        {
            //Debug.LogWarning("action()에서 움직이는 벽이 있다고 판단했으나, push() 실행 시 블록을 찾지 못했습니다.");
        }
    }

    void action()
    {
        // 사망 상태일 때는 어떤 액션도 무시
        if (isDead) return;

        // ⭐ LIFE가 0인지 체크하는 로직은 GameManager.PlayerActionTaken() 내부에서 처리됨.

        Vector3 nextTargetPos = transform.position + dirVec;
        _nextTargetPos = nextTargetPos;

        if(IsClear(nextTargetPos))
        {
            //Debug.Log("클리어안됨 2");
            
        Collider2D hitCollider = Physics2D.OverlapPoint(nextTargetPos, ClearLayer);
            Clear_stage ClearScript = hitCollider.GetComponent<Clear_stage>();
            if (ClearScript != null)
            {

                //Debug.Log("클리어안됨 3");
                ClearScript.Clear();
            }
        }

        if (IsMonsterAtTarget(nextTargetPos))
        {
            kick(nextTargetPos);
        }
        // 몬스터 다음으로 움직이는 벽(PushableBlock)이 있는지 검사
        else if (IsPushableBlockAtTarget(nextTargetPos))
        {
            push(nextTargetPos);
        }
        else if (CanMoveToTarget(nextTargetPos))//가려는 방향에 무언가가 없는경우 움직임
        {
            move();
        }
        else//가려는 방향에 무언가가 있는경우 (벽 타일)
        {
            // 벽 타일에 막혔을 때 (액션 실패)는 LIFE 감소 없음
        }
    }
    bool IsClear(Vector3 nextTargetPos)
    {
        //Debug.Log("클리어안됨 1");
        Collider2D hitCollider = Physics2D.OverlapPoint(nextTargetPos, ClearLayer);
        if (hitCollider != null)
        {
            return true;//클리어
        }
        return false;//클리어 아님
    }

    bool IsMonsterAtTarget(Vector3 nextTargetPos)
    {
        // nextTargetPos: 이동하려는 다음 칸의 월드 좌표
        Collider2D hitCollider = Physics2D.OverlapPoint(nextTargetPos, monsterLayer);
        if (hitCollider != null)
        {
            //Debug.Log("이동하려는 위치에 몬스터(" + hitCollider.gameObject.name + ")가 있습니다!");
            return true;
        }
        return false; // 몬스터 없음
    }

    // 다음 위치에 움직이는 벽이 있는지 체크
    bool IsPushableBlockAtTarget(Vector3 nextTargetPos)
    {
        if (pushableBlockLayer.value == 0)
        {
            Debug.LogError("player 스크립트에 pushableBlockLayer가 설정되지 않았습니다!");
            return false;
        }

        Collider2D hitCollider = Physics2D.OverlapPoint(nextTargetPos, pushableBlockLayer);

        if (hitCollider != null)
        {
            Debug.Log("이동하려는 위치에 움직이는 벽(" + hitCollider.gameObject.name + ")이 있습니다!");
            return true;
        }
        return false;
    }

    bool CanMoveToTarget(Vector3 worldPosition)
    {
        if (outLineTilemap == null)
        {
            Debug.LogError("outLineTilemap 변수에 타일맵 컴포넌트가 연결되지 않았습니다!");
            return true; // 타일맵이 없으면 일단 이동 허용
        }
        Vector3Int cellPosition = outLineTilemap.WorldToCell(worldPosition);
        TileBase tile = outLineTilemap.GetTile(cellPosition);
        if (tile == null)
        {
            return true;
        }
        else
        {
            Debug.Log("이동할 목표 위치에 타일이 있습니다. 이동 불가!");
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_nextTargetPos, 0.2f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, _nextTargetPos);
    }
}