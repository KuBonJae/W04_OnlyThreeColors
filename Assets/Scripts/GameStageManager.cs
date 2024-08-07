using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class GameStageManager : MonoBehaviour
{
    // 비커에 대한 프리팹들을 [SerializeField]로 미리 받아둔다
    // StageData에 List 추가해서 이쪽에 프리팹들 넣어놔도 될 것 같기도
    [SerializeField]
    private StageDataSO stageDataSO;
    //
    // 세팅될 비커 데이터
    BeakerSetting stageBeaker;
    //
    // 비커 선택 여부 & 선택된 비커 번호
    bool firstBeakerSelected = false;
    int firstSelectedBeakerNum = 1995;
    bool secondBeakerSelected = false;
    int secondSelectedBeakerNum = 1995;
    //
    // 답안 제출 버튼 클릭 여부
    bool submitAnswer = false;
    //
    // 프리팹이 처음 생성되는 위치
    Vector3 beakerPosition = Vector3.zero;
    //
    // 스테이지 별 플레이어의 최근 풀이가 저장될 List
    List<List<Tuple<int, int>>> playersChoice;
    //
    // 총 스테이지 갯수 << 스테이지 갯수 따라서 숫자 변경 해줄 것
    int totalStageNum = 12;
    //
    // 스테이지 버튼들 미리 넣어두기 << 나중에 스테이지 클리어시 다음 버튼 언락 용
    public GameObject[] tutStageButtons;
    public GameObject[] easyStageButtons;
    public GameObject[] normalStageButtons;
    public GameObject[] hardStageButtons;
    //
    // 클리어된 스테이지 수 -> 버튼 수와 비교해서 버튼 수의 50% 이상이 클리어 되면 다음 스테이지 버튼을 오픈
    private int tutStageClearCount = 0;
    private int easyStageClearCount = 0;
    private int normalStageClearCount = 0;
    private int hardStageClearCount = 0;
    //
    // 다음 스테이지 버튼 오픈 여부, 이미 오픈된 상태면 굳이 다시 SetActive(true) 할 필요 없도록 -> Tut는 항시 오픈
    private bool easyStageOpened = false;
    private bool normalStageOpened = false;
    private bool hardStageOpened = false;
    //
    // 현재 스테이지 번호
    int curStageNum;
    //
    // 취소 버튼의 활성화 여부 -> 옮길 비커를 한개 선택 했는데 해당 비커를 선택하고 싶지 않게 마음이 바뀌었을 때
    bool isCanceled = false;
    //
    // 캔버스 미리 받아두기
    public GameObject canvas;
    //

    private void Awake()
    {
        SetStage(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        playersChoice = new List<List<Tuple<int, int>>>();
        for(int i=0;i< totalStageNum;i++) // 스테이지 갯수만큼 답지 List<Tuple> 을 생성해서 미리 넣어준다.
        {
            playersChoice.Add(new List<Tuple<int, int>>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(firstBeakerSelected) // 옮길 비커가 선택 되었는가?
        {
            isCanceled = true;
            // 첫 비커가 선택된 상태면 취소 버튼을 Enable로 변경한다.

            //
            if (secondBeakerSelected) // 옮겨질 비커도 선택 되었는가?
            {
                // 둘다 선택 시 일단 selected 바로 초기화
                firstBeakerSelected = secondBeakerSelected = isCanceled = false;
                EventSystem.current.SetSelectedGameObject(null); // 버튼 선택된 것 해제 << 이거 해제 안하면 같은 버튼 클릭이 연속으로 안됨
                canvas.transform.GetChild(firstSelectedBeakerNum).Find("Indicator").gameObject.SetActive(false);
                //첫번째 비커 선택이 풀렸으니 취소 버튼도 Disable로 변경할 것

                //

                playersChoice[curStageNum].Add(new Tuple<int,int>(firstSelectedBeakerNum, secondSelectedBeakerNum));

                MoveRGBToAnotherBeaker(firstSelectedBeakerNum, secondSelectedBeakerNum);
            }
        }
    }

    // 스테이지 버튼 클릭 시 발생하는 함수
    public void StageBtnClicked(Button button)
    {
        // 버튼 이름들로 해당 스테이지 세팅
        switch (button.gameObject.name)
        {
            case "Tutorial1": // 비커 이름으로 세팅하긴 하는데 다른 좋은 방식 추천받습니다.
                curStageNum = 0;
                break;
            case "Stage1":
                curStageNum = 1;
                break;
        }
        // 비커 세팅 함수
        SetStage(curStageNum);
    }

    private void SetStage(int stageNum)
    {
        // 해당 스테이지 정보에 맞는 비커들을 화면에 세팅 해줘야 한다.
        /*List<int> beakerSize;
        List<string> beakerString;

        switch (stageNum) // 각 스테이지 별 스펙을 여기에 적는다. 좀 더 깔끔하게 데이터 유지할 수 있는 방법이 있을까?
        {
            case 0:
                beakerSize = new List<int>() { 11, 4, 7 };
                beakerString = new List<string>() { "RRRRRRRRRRR" };
                stageBeaker = new BeakerSetting(beakerSize, beakerString, "RRRRRRRRRRR");
                break;
            case 1:
                beakerSize = new List<int>() { 11, 4, 7, 9 };
                beakerString = new List<string>() { "RRRRRRRRRRR" };
                stageBeaker = new BeakerSetting(beakerSize, beakerString, "RRRRRRRRRRR");
                break;
        }*/

        stageBeaker = new BeakerSetting(stageDataSO.stageDatas[stageNum].beakerSize,
            stageDataSO.stageDatas[stageNum].beakerRGB, stageDataSO.stageDatas[stageNum].answerBeaker);

        SetBeakerUI(stageBeaker);
    }

    private void SetBeakerUI(BeakerSetting beakerSetting)
    {
        // 비커 사이즈에 따라 순서대로 프리팹을 불러와 위치시킴
        for(int i=0; i< beakerSetting.beakerSize.Count; i++)
        {
            // 첫 위치에 프리팹 instantiate
            // GameObject uiInstance = Instantiate(BeakerPrefab11, beakerPosition); // 11 크기의 비커 프리팹 생성
            // 인스턴스화된 UI의 위치 조정 -> 그 다음에 추가되는 비커는 canvas의 가장 마지막 자식의 위치에서 일정 옆으로 띄워서 생성시키면 됨
            // RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
            // rectTransform.SetParent(Canvas.main.GetComponent<RectTransform>(), false); // 캔버스를 부모로 설정 -> 캔버스 이름 따라 변경

            // 임시 Instantiate 연습 -> 해당 방식으로 활용하면 될 듯
            GameObject uiInstance = Instantiate(stageDataSO.stageDatas[curStageNum].beakerPrefabs[i]);
            uiInstance.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = i.ToString();
            RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
            rectTransform.SetParent(canvas.GetComponent<RectTransform>(), false);
            uiInstance.GetComponent<RectTransform>().localPosition = new Vector3(-100 + i * 200, 0, 0); // -> 로컬 위치는 Global에서 정하고 들어갈 것
            uiInstance.GetComponent<Button>().onClick.AddListener(() => BeakerSelected(uiInstance.GetComponent<Button>()));

            Stack<char> charSampleStack = new Stack<char>(beakerSetting.beakerStack[i]);
            int count = 0;
            while(charSampleStack.Count > 0)
            {
                char RGB = charSampleStack.Pop();
                switch(RGB)
                {
                    case 'R':
                        canvas.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = Color.red;
                        break;
                    case 'G':
                        canvas.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = Color.green;
                        break;
                    case 'B':
                        canvas.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = Color.blue;
                        break;
                }
                count++;
            }
            
        }

    }

    public void BeakerSelected(Button button) // 제출용 비커의 이름은 404 << 로 할 것
    {
        if(firstBeakerSelected)
        {
            //secondSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            secondSelectedBeakerNum = Convert.ToInt32(button.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>().text);
            secondBeakerSelected = true;
            Debug.Log("second Btn clicked");
        }
        else
        {
            //firstSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            firstSelectedBeakerNum = Convert.ToInt32(button.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>().text);
            // 선택 버튼의 indicator 표시
            button.transform.Find("Indicator").gameObject.SetActive(true);
            firstBeakerSelected = true;
            Debug.Log("first Btn clicked");
            // 취소 버튼을 사용할 수 있도록 Enable로 변경할 것

        }
    }

    void MoveRGBToAnotherBeaker(int fromBeaker, int toBeaker)
    {
        if (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker]) // 해당 번호의 비커가 비어있는 공간이 있을 것
        {
            while (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker] // 빈 공간이 다 채워질때까지 from 쪽에서 옮겨담음 or
                && stageBeaker.curBeakerAmount[fromBeaker] > 0) // from쪽 비커의 현재 남은 RGB 수가 0 개가 될 때까지 진행 
            {
                char RGB = stageBeaker.beakerStack[fromBeaker].Pop();
                stageBeaker.beakerStack[toBeaker].Push(RGB); // from 비커의 맨 위를 확인해서 to 비커에게 집어넣음
                // UI 변경되는 것 (비커 색 추가되고 감소되는 것) 은 여기에서 그때마다 바로바로 체크해주는게 좋을듯
                switch (RGB)
                {
                    case 'R':
                        canvas.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = Color.red;
                        break;
                    case 'G':
                        canvas.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = Color.green;
                        break;
                    case 'B':
                        canvas.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = Color.blue;
                        break;
                }
                canvas.transform.GetChild(fromBeaker).Find("Image" + (stageBeaker.curBeakerAmount[fromBeaker]).ToString()).GetComponent<Image>().color = Color.white;
                //
                stageBeaker.curBeakerAmount[fromBeaker]--; // from 비커의 숫자 한개 감소
                stageBeaker.curBeakerAmount[toBeaker]++; // to 비커의 숫자 한개 증가
            }
        }
        // 빈 공간이 없으면 작동 안함
    }

    public void AnswerBtnClicked()
    {
        Stack<char> playerAnswerStack = stageBeaker.beakerStack[stageBeaker.playerAnswerBeakerNum];
        Stack<char> AnswerStack = stageBeaker.beakerAnswer;
        bool stageClear = true; // 별 일 없으면 스테이지 클리어로 체크할 boolean

        if(AnswerStack.Count != playerAnswerStack.Count)
        {
            // 갯수가 당연히 다르면 실패가 확실
            stageClear = false;
        }
        else
        {
            while(playerAnswerStack.Count > 0) // 어차피 둘이 갯수는 동일하니 하나만 체크해도 ㄱㅊ
            {
                if(playerAnswerStack.Peek() != AnswerStack.Peek()) // 둘이 맨 앞이 다르면 다른 string 이라는 것이다
                {
                    // 실패했다고 표시
                    stageClear = false;
                    break;
                }
                else
                {
                    playerAnswerStack.Pop();
                    AnswerStack.Pop(); // 맨 앞에 꺼내버림
                }
                // 맨 앞에서 꺼내서 비교하는걸 반복하다가 끝까지 반복했는데 모두 동일했다면 stageClear은 그대로 true가 될 것이다
            }
        }

        if(stageClear)
        {
            // 스테이지 클리어 캔버스 SetActive(true)

            // 비커 프리팹들 먼저 삭제

            // 다음 스테이지 언락
            // curStageNum << 을 활용해서 현재 클리어한 stage 버튼은 색을 약간 어둡게 만들어 주고, curStageNum + 1 번호의 버튼을 Enable() 시킴
            
            // 스테이지 캔버스로 변경

        }
        else 
        {
            // 클리어 못했으면 할 행동? UI? 여기에 넣어서 관리할 것

        }
    }

    public void ResetBtnClicked() // 리셋 버튼과 연결
    {
        // 기존에 세팅되어 있는 비커 프리팹들 보관해 뒀다가 먼저 Destory 할 것
        // Destroy( );

        // 스테이지 재시작
        SetStage(curStageNum);
    }

    public void GoBackBtnClicked() // 스테이지 창으로 돌아가기 버튼과 연결
    {
        // 현재 만들어져 있는 비커 프리팹들 먼저 Destroy

        // Stage 선택 캔버스 Active;
    }

    public void SelectCancelBtnClicked() // 선택 취소 버튼과 연결
    {
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995; // 1995가 의미가 있는 것은 아니고 그냥 리셋한다는 의미로 넣어뒀습니다. 0이라고 하면 0번째 버튼이랑 매핑돼버려서...
        // 취소 버튼은 다시 Disable로 변경할 것

    }
}