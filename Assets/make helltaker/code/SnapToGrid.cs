using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    // ✨ 그리드 크기 (1이면 정수 좌표에 맞춥니다.)
    // 0.5f로 설정하면 0.5, 1.5, 2.5... 좌표에 맞춥니다.
    public float gridSize = 0.5f;

    // LateUpdate는 모든 Update 함수가 실행된 후 호출됩니다.
    // 다른 오브젝트의 이동이 끝난 후 정렬해야 정확합니다.
    void LateUpdate()
    {
        // 현재 오브젝트의 위치를 가져옵니다.
        Vector3 currentPosition = transform.position;

        // 1. 현재 좌표를 gridSize로 나눕니다. (예: 2.3 -> 2.3)
        // 2. Mathf.Round()를 사용하여 가장 가까운 정수로 반올림합니다.
        // 3. 다시 gridSize를 곱하여 그리드 크기에 맞는 좌표를 얻습니다.

        float snappedX = Mathf.Round(currentPosition.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(currentPosition.y / gridSize) * gridSize;
        // Z축은 보통 0이므로 변경하지 않습니다. 필요하다면 X, Y와 동일하게 처리합니다.

        // 새로운 위치를 설정합니다.
        transform.position = new Vector3(snappedX, snappedY, currentPosition.z);
    }
}
