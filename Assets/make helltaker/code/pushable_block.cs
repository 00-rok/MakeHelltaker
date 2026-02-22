using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PushableBlock : MonoBehaviour
{
    private Vector3 target_pos;
    public Tilemap obstacleTilemap; // 씬의 벽 타일맵 연결

    // ⭐ 다른 움직이는 벽(PushableBlock)과 충돌 체크를 위한 레이어 마스크
    public LayerMask pushableBlockLayer;

    public float moveSpeed = 15f;
    private bool isMoving = false;

    void Awake()
    {
        target_pos = transform.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target_pos, Time.deltaTime * moveSpeed);

        // 이동이 멈췄을 때 상태 업데이트
        if (transform.position == target_pos)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }
    }

    // =======================================================
    // ⭐ 플레이어에게 발차기를 받았을 때 호출되는 함수 (bool 반환)
    // =======================================================
    public bool GetPushed(Vector3 pushDirection) // ✅ 이 부분이 bool을 반환해야 합니다!
    {
        // 1. 이미 이동 중이라면 추가적인 발차기를 무시합니다.
        if (isMoving) return false; // ⭐ 실패 (이동 불가) 반환

        Vector3 newTargetPos = transform.position + pushDirection;

        // 2. ⭐ 이동할 다음 위치에 장애물 체크 (벽 또는 다른 블록)
        bool isWall = IsWallAt(newTargetPos);
        bool isOtherBlock = IsOtherPushableBlockAt(newTargetPos);

        // 벽이 있거나 다른 블록이 있다면 움직이지 않습니다.
        if (isWall || isOtherBlock)
        {
            Debug.Log("움직이는 벽이 다른 장애물과 충돌하여 움직임을 거부합니다.");
            // 발차기는 성공했지만 벽돌은 움직이지 않습니다.
            return false; // ⭐ 실패 (이동 불가) 반환
        }
        else
        {
            // 장애물이 없다면 새로운 목표 위치로 이동
            target_pos = newTargetPos;
            isMoving = true;
            return true; // ⭐ 성공 (이동 시작) 반환
        }
    }

    // =======================================================
    // 내부 함수: 벽 타일 체크
    // =======================================================
    private bool IsWallAt(Vector3 worldPosition)
    {
        if (obstacleTilemap == null)
        {
            Debug.LogError("PushableBlock 스크립트에 obstacleTilemap이 연결되지 않았습니다!");
            return false;
        }
        // 월드 좌표를 Cell 좌표로 변환하여 타일 존재 여부 확인
        Vector3Int cellPosition = obstacleTilemap.WorldToCell(worldPosition);
        return obstacleTilemap.HasTile(cellPosition);
    }

    // =======================================================
    // 내부 함수: 다른 움직이는 벽 체크
    // =======================================================
    private bool IsOtherPushableBlockAt(Vector3 nextTargetPos)
    {
        if (pushableBlockLayer.value == 0)
        {
            Debug.LogWarning(gameObject.name + ": pushableBlockLayer가 설정되지 않았습니다! 블록 충돌 체크 불가능.");
            return false;
        }

        // OverlapPoint를 사용하여 해당 위치에 Collider가 있는지 감지합니다.
        Collider2D hit = Physics2D.OverlapPoint(nextTargetPos, pushableBlockLayer);

        if (hit != null)
        {
            // 감지된 오브젝트가 '나 자신'이 아닌지 확인
            if (hit.gameObject != gameObject)
            {
                PushableBlock block = hit.GetComponent<PushableBlock>();
                if (block != null)
                {
                    Debug.Log("이동하려는 위치에 다른 움직이는 벽(" + hit.gameObject.name + ")이 있습니다!");
                    return true;
                }
            }
        }

        return false;
    }
}