using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public GameObject[] moveEffectPrefabs;

    //  kickEffectPrefabs[0], [1]: 큰 이펙트 (성공)
    //  kickEffectPrefabs[2], [3]: 작은 이펙트 (실패/충돌)
    public GameObject[] kickEffectPrefabs;

    // 플레이어 스크립트가 호출하는 함수
    //  isRefused: true면 작은 이펙트 (인덱스 2, 3), false면 큰 이펙트 (인덱스 0, 1) 사용
    public void PlayKickEffect(Vector3 position, bool isRefused = false)
    {
        // 최소한 큰 이펙트 2개, 작은 이펙트 2개가 필요하다고 가정 (총 4개)
        if (kickEffectPrefabs == null || kickEffectPrefabs.Length < 4)
        {
            Debug.LogWarning("Kick Effect Prefabs 배열이 FXManager에 연결되지 않았거나 4개 미만입니다! 이펙트가 재생되지 않습니다.");
            return;
        }

        int randomIndex;
        int startIndex;
        int endIndex;

        if (isRefused)
        {
            // 작은 이펙트 (Refused/Collision): 인덱스 2 또는 3
            startIndex = 2;
            endIndex = 4; // Random.Range(inclusive, exclusive)
            Debug.Log("작은 Kick 이펙트 (충돌/실패) 재생.");
        }
        else
        {
            // 큰 이펙트 (Success/Kick): 인덱스 0 또는 1
            startIndex = 0;
            endIndex = 2;
            Debug.Log("큰 Kick 이펙트 (성공/이동) 재생.");
        }

        // 배열에서 랜덤 인덱스 선택
        randomIndex = Random.Range(startIndex, endIndex);

        GameObject selectedPrefab = kickEffectPrefabs[randomIndex];

        if (selectedPrefab != null)
        {
            // 요청된 위치(몬스터의 현재 위치)에 이펙트 생성
            Instantiate(selectedPrefab, position, Quaternion.identity);
            Debug.Log($"Kick 이펙트 ({randomIndex}번) 생성 위치: {position}");
        }
    }

    public void PlayMoveEffect(Vector3 position)
    {
        // 1. 배열이 비어있는지 확인
        if (moveEffectPrefabs == null || moveEffectPrefabs.Length == 0)
        {
            Debug.LogWarning("Move Effect Prefabs 배열이 비어있거나 FXManager에 연결되지 않았습니다! 이펙트가 재생되지 않습니다.");
            return; // 함수 실행 중지
        }

        // 2.핵심: 0부터 배열의 길이 직전까지의 인덱스를 랜덤으로 선택
        int randomIndex = Random.Range(0, moveEffectPrefabs.Length);

        // 3. 랜덤으로 선택된 프리팹 가져오기
        GameObject selectedPrefab = moveEffectPrefabs[randomIndex];

        // 4. 프리팹이 유효한지 최종 확인 후 생성
        if (selectedPrefab != null)
        {
            Instantiate(selectedPrefab, position, Quaternion.identity);
            Debug.Log($"랜덤 무브 이펙트 ({randomIndex}번) 생성 위치: {position}");
        }
    }

}