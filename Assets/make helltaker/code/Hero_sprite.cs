using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero_sprite : MonoBehaviour
{

    Animator anim;
    SpriteRenderer spriter;
    void Awake()
    {
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
    }

    public void LoadAnim()
    {
        Debug.Log("¿Ã∞≈ ø÷æ»µ  §µ§≤");
        GameManager.Instance.targetObject.SetActive(true);


    }
    void Update()
    {
        
    }
}
