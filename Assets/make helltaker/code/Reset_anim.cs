using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset_anim : MonoBehaviour
{
    public GameManager Scene;
    public GameObject abc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EndAnim()
    {
        abc.SetActive(false);
    }
    public void LoadScene()
    {
        //GameManager.Instance.Reset();
        Scene.Reset();
    }
}
