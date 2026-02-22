using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDestroyer : MonoBehaviour
{
    public float effectDuration = 0.2f;

    void Start()
    {
        // 지정된 시간(effectDuration) 후에 이펙트 오브젝트를 파괴합니다.
        Destroy(gameObject, effectDuration);
    }
}
