using UnityEngine;

public class Clear_stage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int curr_stage;

    public LayerMask Clear_Layer;
    void Start()
    {
        curr_stage = 1;
    }
    public void Clear()
    {
        if(curr_stage == 1)
        {
            TalkManager.Instance.id = 2003;
            TalkManager.Instance.talkindex = 0;
            TalkManager.Instance.isTalk = true;
        }
        else if(curr_stage == 2)
        {
            TalkManager.Instance.id = 3001;
            TalkManager.Instance.talkindex = 0;
        }
        TalkManager.Instance.Talk();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
