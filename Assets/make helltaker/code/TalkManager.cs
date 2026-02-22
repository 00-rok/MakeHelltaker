using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.VFX;

// 1001= 1챕터보스
// 2002= 2챕터 NPC
public class TalkManager : MonoBehaviour
{
    public static TalkManager Instance;
    Dictionary<int, string[]> talkData;//고유번호,말
    Dictionary<int, Sprite> portraitData;//고유번호,이미지

    public GameObject talkPanel;
    public TextMeshProUGUI talk;
    public TextMeshProUGUI NPC_NAME;
    public Sprite[] portraitArr;
    public Image portraitImg;

    public Animator textAnim;

    public Animator img_anim;
    public bool isTalk;
    public int talkindex;
    public int id;
    public bool isNPC;
    public int count;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 파괴되지 않게 유지
            // 만약 TalkManager가 항상 Additive Load되는 씬이라면 이 코드는 생략할 수 있습니다.
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            //Destroy(gameObject);
        }
        talkData = new Dictionary<int, string[]>();
        portraitData = new Dictionary<int, Sprite>();
        

        GenerateData();
    }

    private void Start()
    {
        // 시작화면 띄우기
        id = 1003;
        isTalk = true;
        isNPC = false;
        talkPanel.SetActive(true);
        Talk();
        count = 0;

    }   
    private void Update()
    {
        if(isNPC==true)
        {
            NPC_NAME.gameObject.SetActive(true);
        }
        else
        {
            NPC_NAME.gameObject.SetActive(false);
        }
        if (isTalk)
        {
            GameManager.Instance.global_UI.SetActive(false);
            if (Input.GetKeyUp(KeyCode.Space))
            {
                isNPC = true;
                Talk();
            }
        }
        else
        {
            talkPanel.SetActive(false);
        }
        
    }
    public void Talk()
    {

        textAnim.SetTrigger("gogo");
        talkPanel.SetActive(true);
        talk.text= GetTalk(id, talkindex);
        if (talk.text == null)
        {
            talkindex = 0;
            isTalk = false;
            GameManager.Instance.global_UI.SetActive(true);
            count = 0;
            return;
        }
        if (isNPC)
        {
            if(count==0)
            {
                img_anim.SetTrigger("go_img");
                count++;
            }
            talk.text = talkData[id][talkindex].Split(':')[0];
            portraitImg.sprite = portraitArr[int.Parse(talkData[id][talkindex].Split(':')[1])];
            portraitImg.color = new Color(1, 1, 1, 1);//불투명
            
        }
        else
        {
            talk.text = talkData[id][talkindex];

            portraitImg.color = new Color(1, 1, 1, 0);//투명
        }

        talkindex++;
    }



    public string GetTalk(int id, int talkIndex)
    {
        if (talkIndex == talkData[id].Length)
        {
            return null;
        }
        else
        {

            return talkData[id][talkIndex];//이놈이 실행되면 원하는 말이 나올것임 이녀석이 실행되기위해선?
        }


    }

    public Sprite Getportrait(int id, int portraitIndex)
    {
        return portraitData[id + portraitIndex];
    }



    // 모든 대화
    void GenerateData()
    {
        //talkData.Add(해당하는 ID를 넣어주면 됩니다, new string[] { "원하는 말넣고"}
        talkData.Add(1003, new string[] { "you find youreself surronded by the void.\nPress[SPACEBAR] to continue. ",
            "Greeting little one. Please don't mind me.\n It is junst I, good old Beelzebub.:1", "Start Game:1" });
        talkData.Add(2003, new string[] { "It's not finished yet.:6", "so please go back.:5" });
        talkData.Add(3005, new string[] { "It's not finished yet.", "so please go back.", "그런데 무빙워크처럼 움직이는거야?", " 다른프레임은 없어?" });
        talkData.Add(4001, new string[] { "It's not finished yet.", "so please go back.", "그런데 무빙워크처럼 움직이는거야?", " 다른프레임은 없어?" });
        talkData.Add(301, new string[] { "It's not finished yet.", "so please go back.", "그런데 무빙워크처럼 움직이는거야?", " 다른프레임은 없어?" });


        //portraitArr[1]이면 scale을 1로
        //portraitArr[1]이 아니면 scale을 0.7로 바꾸면 해결될듯?
        portraitData.Add(1003 + 0, portraitArr[0]);//0은 NONE
        portraitData.Add(1003 + 1, portraitArr[1]);//1은 메인메뉴 1
        portraitData.Add(2003 + 0, portraitArr[2]);//2는 메인메뉴 2
        portraitData.Add(2003 + 1, portraitArr[3]);//3은 판데모니카
        portraitData.Add(1003 + 2, portraitArr[4]);//4는 판데모니카2
        portraitData.Add(1003 + 3, portraitArr[5]);//5는 만데모니카3
        portraitData.Add(1003 + 4, portraitArr[6]);//5는 만데모니카3

    }
    //scale을 isnpc가 true인경우에 전부 0.7로 바꾸기 챕터 0인 친구만 npc가 아닌걸로 바꾸면될듯? 어차피 none 있으니까 none으로 is npc를 체크하면될수도? 애초에 


}
