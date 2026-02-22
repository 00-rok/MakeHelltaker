using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Monster : MonoBehaviour
{
    private Animator monsterAnimator;

    public GameObject destructionEffectPrefab;
    private Vector3 target_pos;
    public GameObject[] bonePrefabs;
    public Tilemap obstacleTilemap;
    private bool isDead = false;
    public float explosionForce=20f;

    public float moveSpeed = 15f;

    //  추가: 몬스터 충돌 체크를 위한 레이어 마스크
    // (인스펙터에서 플레이어가 사용하는 것과 동일한 몬스터 레이어를 설정해야 합니다!)
    public LayerMask monsterLayer;
    public LayerMask pushable_blockLayer;

    void Awake()
    {
        monsterAnimator = GetComponentInChildren<Animator>();
        if (monsterAnimator == null)
        {
            //Debug.LogError(gameObject.name + ": Animator 컴포넌트를 찾을 수 없습니다! kicked 애니메이션을 실행할 수 없습니다.");
        }
        target_pos = transform.position;
    }
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target_pos, Time.deltaTime * moveSpeed);

        // (선택 사항) 목표 위치에 완전히 도착했을 때 추가 로직을 처리할 수 있습니다.
        if (transform.position == target_pos)
        {
            // 몬스터의 다음 행동 로직 (예: AI 다시 시작, 다음 이동 계획 등)
        }
    }
    // =======================================================
    // 1. GetKicked 함수 수정: 몬스터 충돌 체크 로직 추가
    // =======================================================
    public void GetKicked(Vector3 kickDirection)
    {
        // 몬스터가 이미 죽고 있다면 추가적인 킥을 무시합니다.
        if (isDead) return;

        if (monsterAnimator != null)
        {
            // 1. kicked 애니메이션 실행
            monsterAnimator.SetTrigger("kicked");
        }

        Vector3 newTargetPos = transform.position + kickDirection;

        // 2. 이동할 다음 위치에 벽 또는 다른 몬스터가 있는지 체크
        bool isWall = IsWallAt(newTargetPos);

        PushableBlock ispushablewall = Ispushable_blockAt(newTargetPos);

        // ⭐ 목표 위치에 있는 다른 몬스터를 찾습니다.
        Monster hitMonster = IsMonsterAt(newTargetPos);

        // 벽 또는 다른 몬스터가 있다면 충돌 처리
        if (isWall || hitMonster || ispushablewall != null)
        {
            //SetDieState();
            // 1) 내가 파괴됩니다 (벽이든 몬스터든 나를 파괴).
            SetDieState();

            // 2) ⭐ 부딪힌 몬스터가 있다면, 그 몬스터도 파괴합니다.
            /*if (hitMonster != null)
            {
                // 부딪힌 몬스터도 파괴 효과를 재생하고 삭제됩니다.
                hitMonster.DestroySelf(newTargetPos);
            }*/
        }
        else
        {
            // 장애물이 없다면 이동 시작
            target_pos = newTargetPos;
        }
    }
    private PushableBlock Ispushable_blockAt(Vector3 nextTargetPos)
    {
        if (pushable_blockLayer.value == 0)
        {
            //Debug.LogWarning(gameObject.name + ": pushable_blockLayer 설정되지 않았습니다! 몬스터 충돌 체크 불가능.");
            return null;
        }

        // Physics2D.OverlapPointAll을 사용하여 해당 위치의 모든 Collider를 감지합니다.
        Collider2D[] hits = Physics2D.OverlapPointAll(nextTargetPos, pushable_blockLayer);

        foreach (Collider2D hit in hits)
        {
            // 감지된 오브젝트가 '나 자신'이 아니면서 'pushable_blockLayer' 컴포넌트가 있는지 확인
            if (hit.gameObject != gameObject)
            {
                PushableBlock ispsuhable_block = hit.GetComponent<PushableBlock>();
                if (ispsuhable_block != null)
                {
                    //Debug.Log("이동하려는 위치에 다른 몬스터(" + hit.gameObject.name + ")가 있습니다!");
                    return ispsuhable_block; // 다른 몬스터를 찾았으니 해당 몬스터 스크립트 반환
                }
            }
        }

        return null; // 다른 몬스터 없음
    }


    // =======================================================
    // ⭐ 추가: 목표 위치에 다른 몬스터가 있는지 체크하는 함수
    // =======================================================
    private Monster IsMonsterAt(Vector3 nextTargetPos)
    {
        if (monsterLayer.value == 0)
        {
            //Debug.LogWarning(gameObject.name + ": monsterLayer가 설정되지 않았습니다! 몬스터 충돌 체크 불가능.");
            return null;
        }

        // Physics2D.OverlapPointAll을 사용하여 해당 위치의 모든 Collider를 감지합니다.
        Collider2D[] hits = Physics2D.OverlapPointAll(nextTargetPos, monsterLayer);

        foreach (Collider2D hit in hits)
        {
            // 감지된 오브젝트가 '나 자신'이 아니면서 'Monster' 컴포넌트가 있는지 확인
            if (hit.gameObject != gameObject)
            {
                Monster monster = hit.GetComponent<Monster>();
                if (monster != null)
                {
                    //Debug.Log("이동하려는 위치에 다른 몬스터(" + hit.gameObject.name + ")가 있습니다!");
                    return monster; // 다른 몬스터를 찾았으니 해당 몬스터 스크립트 반환
                }
            }
        }

        return null; // 다른 몬스터 없음
    }


    private bool IsWallAt(Vector3 worldPosition)
    {
        if (obstacleTilemap == null)
        {
            //Debug.LogError("Monster 스크립트에 obstacleTilemap이 연결되지 않았습니다!");
            return false;
        }

        // 월드 좌표를 Tilemap의 Cell 좌표로 변환
        Vector3Int cellPosition = obstacleTilemap.WorldToCell(worldPosition);

        // 해당 Cell에 Tile이 있는지 확인 (Tile이 있으면 벽이 있다는 뜻)
        return obstacleTilemap.HasTile(cellPosition);
    }

    // =======================================================
    // 4. 파괴 처리 함수 (이름을 DestroySelf로 변경)
    // =======================================================
    public void DestroySelf(Vector3 effectPosition)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log(gameObject.name + "가 충돌로 인해 파괴 효과 실행 후 삭제됩니다.");

        // 1. 파괴 효과 프리팹을 위치에 생성합니다.
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, effectPosition, Quaternion.identity);
        }
        else
        {
            //Debug.LogWarning("Destruction Effect Prefab이 Monster 스크립트에 연결되지 않았습니다!");
        }

        // 2. 몬스터 오브젝트를 씬에서 즉시 삭제합니다.
        Destroy(gameObject);
    }
    public void SetDieState()
    {
        int Spawn_bone_num = Random.Range(15, 20);
        for (int i = 0; i < Spawn_bone_num; i++)
        {
            // 🚨 무작위 뼈 조각 프리팹 선택
            GameObject selectedPrefab = bonePrefabs[Random.Range(0, bonePrefabs.Length)];

            // 몬스터의 위치에서 뼈 조각 생성
            GameObject fragment = Instantiate(selectedPrefab, transform.position, Quaternion.identity);

            // Rigidbody2D 가져오기
            Rigidbody2D rb = fragment.GetComponent<Rigidbody2D>();


            if (rb != null)
            {
                // 랜덤한 방향 벡터 생성
                Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0f, 1f)).normalized;
                // X축: 좌우 랜덤
                // Y축: 위쪽 방향으로만 (터지는 느낌)

                // 폭발력 적용 (AddForce 사용)
                rb.AddForce(randomDirection * explosionForce, ForceMode2D.Impulse);

                // 뼈 조각이 너무 오래 남아있지 않도록 일정 시간 후 파괴
                Destroy(fragment, 1f);
            }
        }
        Destroy(gameObject);
    }
}