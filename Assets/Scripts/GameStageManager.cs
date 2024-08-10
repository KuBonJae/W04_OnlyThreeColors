using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class GameStageManager : MonoBehaviour
{
    // 비커에 대한 프리팹들을 [SerializeField]로 미리 받아둔다
    // StageData에 List 추가해서 이쪽에 프리팹들 넣어놔도 될 것 같기도
    [Header("SriptableObject")]
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
    bool submitAnswer = false; // -> update 문에서 답안 제출을 체크하지 않으므로 현재 쓰지 않음, 마지막에 지울 것
    //
    // 프리팹이 처음 생성되는 위치
    Vector3 beakerPosition = Vector3.zero;
    //
    // 스테이지 별 플레이어의 최고기록 풀이가 저장될 List
    public List<List<Tuple<int, int>>> playersChoice;
    List<Tuple<int, int>> playersChoice_Temp; // 최고기록과 비교할 그때그때 풀이한 List
    List<int> moveWaterAmount; // 옮겨진 물 양 -> 되돌리기 할 때 이거 참고해서 옮겨진 물 양 만큼만 옮겨야 함
    List<bool> stageCleared;
    //
    // 스테이지 별 플레이어의 리스타트 버튼 횟수(클리어 ui에서 선택한 리스타트 제외)
    List<int> playersRestart;
    // 스테이지 별 개발자가 제시하는 풀이 횟수
    public List<int> devAnswerCount;
    //
    // 최대 움직일 수 있는 횟수, 현재 움직인 횟수
    const int maxMoveCount = 100;
    int curMoveCount = 0;
    //
    // 총 스테이지 갯수 << 스테이지 갯수 따라서 숫자 변경 해줄 것
    int totalStageNum = 40;
    //
    // 스테이지 버튼들 미리 넣어두기 << 나중에 스테이지 클리어시 다음 버튼 언락 용
    [Header("Stage Buttons")]
    public GameObject[] tutStageButtons;
    public GameObject[] tutAnswerButtons;
    public GameObject[] easyStageButtons;
    public GameObject[] easyAnswerButtons;
    public GameObject[] normalStageButtons;
    public GameObject[] normalAnswerButtons;
    public GameObject[] hardStageButtons;
    public GameObject[] hardAnswerButtons;
    public GameObject normalBlocker;
    public GameObject hardBlocker;
    //
    // 클리어된 스테이지 수 -> 3 이상이 되면 다음 스테이지 버튼을 오픈
    private int tutStageClearCount = 0;
    private int easyStageClearCount = 0;
    private int normalStageClearCount = 0;
    private int hardStageClearCount = 0;
    private List<bool> alreadyCleared;
    //
    // 다음 스테이지 버튼 오픈 여부, 이미 오픈된 상태면 굳이 다시 SetActive(true) 할 필요 없도록 -> Tut는 항시 오픈
    private bool easyStageOpened = false;
    private bool normalStageOpened = false;
    private bool hardStageOpened = false;
    //
    // 현재 스테이지 번호
    public int curStageNum;
    //
    // 캔버스 미리 받아두기
    [Header("UI")]
    public GameObject canvas_Beaker;
    public GameObject selectStageUI;
    public GameObject doGameUI;
    public GameObject gameClearUI;
    public GameObject PlayerAnswerText;
    public GameObject ResetCountdown;
    public GameObject PlayerPlayCount;
    public GameObject DeveloperPlayCount;
    public GameObject AnswerPanel;
    public GameObject PracticeNote;
    public GameObject noticeCanvas;
    public GameObject TextsInGameClear;
    //
    // 현재 생성 되어 있는 비커 프리팹들 보관
    private List<GameObject> beakerPrefabsOnDisplay = new List<GameObject>();
    //
    // 현재 스테이지 클리어 시 오픈될 정답지 버튼 미리 받아두기
    private GameObject AnswerSheetBtn;
    //
    // 사운드 재생을 위한 스크립트
    SoundManager soundManager;
    //
    // 데이저 저장용 매니저
    public DataManager dataManager;
    //
    //
    // 컬러 코드
    Color[] colors;
    //
    // for Next Stage button
    [Header("About Next Stage Button")]
    public GameObject nextStageButton;
    public int offsetOfLastStagebyDifficulty;
    GameObject currentStageButton;
    //
    // CurrentStageText
    [Header("Current Stage Text")]
    public TextMeshProUGUI currentStageText;
    
    void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
        dataManager = FindObjectOfType<DataManager>();
        playersChoice = new List<List<Tuple<int, int>>>();
        playersChoice_Temp = new List<Tuple<int, int>>();
        playersRestart = new List<int>();
        stageCleared = new List<bool>();
        alreadyCleared = new List<bool>();
        moveWaterAmount = new List<int>();
        for (int i = 0; i < totalStageNum; i++) // 스테이지 갯수만큼 답지 List를 생성해서 미리 넣어준다.
        {
            playersChoice.Add(new List<Tuple<int, int>>());
            stageCleared.Add(false);
            playersRestart.Add(0);
            alreadyCleared.Add(false);
        }

        if (!File.Exists(dataManager.filePath))
            return;

        var (fromList, toList, restartList, isCleared) = dataManager.LoadData();
        if (fromList != null && toList != null && restartList != null && isCleared != null)
        {
            if(fromList.Count != totalStageNum || toList.Count != totalStageNum || restartList.Count != totalStageNum || isCleared.Count != totalStageNum)
            {
                Debug.Log("세이브 데이터 오류");
                return;
            }
            
            for (int i = 0; i < totalStageNum; i++)
            {
                for(int j = 0; j < fromList[i].Count;j++)
                {
                    // i 번쨰 스테이지의 j번째 플레이 턴 수 from, to 기록
                    playersChoice[i].Add(new Tuple<int, int>(fromList[i][j], toList[i][j]));
                }
                playersRestart[i] = (restartList[i]);
                stageCleared[i] = (isCleared[i]);
                if (isCleared[i])
                {
                    alreadyCleared[i] = true;
                    if (i < 10)
                    {
                        tutStageClearCount++;
                        tutAnswerButtons[i].SetActive(true);
                    }
                    else if (i >= 10 && i < 20)
                    {
                        easyStageClearCount++;
                        easyAnswerButtons[i % 10].SetActive(true);
                    }
                    else if (i >= 20 && i < 30)
                    {
                        normalStageClearCount++;
                        normalAnswerButtons[i % 10].SetActive(true);
                    }
                    else if (i >= 30 && i < 40)
                    {
                        hardStageClearCount++;
                        hardAnswerButtons[i % 10].SetActive(true);
                    }
                }
            }
        }
    }
     
    // Start is called before the first frame update
    void Start()
    {
        // 컬러 추가
        colors = new Color[3];
        ColorUtility.TryParseHtmlString("#FF8888", out colors[0]);
        ColorUtility.TryParseHtmlString("#88FF88", out colors[1]);
        ColorUtility.TryParseHtmlString("#8888FF", out colors[2]);
    }

    // Update is called once per frame
    void Update()
    {
        if(firstBeakerSelected) // 옮길 비커가 선택 되었는가?
        {
            if (secondBeakerSelected) // 옮겨질 비커도 선택 되었는가?
            {
                // 둘다 선택 시 일단 selected 바로 초기화
                firstBeakerSelected = secondBeakerSelected = false;
                EventSystem.current.SetSelectedGameObject(null); // 버튼 선택된 것 해제 << 이거 해제 안하면 같은 버튼 클릭이 연속으로 안됨
                canvas_Beaker.transform.GetChild(firstSelectedBeakerNum).Find("Indicator").gameObject.SetActive(false);
                if (curMoveCount == 0) // 해당 스테이지에서 처음 실행된 플레이어의 Move
                {
                    //playersChoice[curStageNum].Clear(); // 먼저 싹 비움 << 이제 최고기록 저장용으로 쓸거기 때문에 비우는건 절대 안됨
                    playersChoice_Temp.Clear(); // 임시로 사용되는 플레이어 기록 List 비우기
                }
                //playersChoice[curStageNum].Add(new Tuple<int,int>(firstSelectedBeakerNum, secondSelectedBeakerNum));
                playersChoice_Temp.Add(new Tuple<int,int>(firstSelectedBeakerNum, secondSelectedBeakerNum));
                
                MoveRGBToAnotherBeaker(firstSelectedBeakerNum, secondSelectedBeakerNum, false);
            }
        }

        // 만약 남은 횟수가 0 미만이면 사망 (100번까지 가능)
        if(curMoveCount > 500)
        {
            DestoryBeakerPrefabs();
            // 해당 부분은 LateUpdate로 전달
            //StartCoroutine("ResetStage"); // 프리팹 삭제 후 강제 리스타트
        }
        //
        // 남은 횟수 ui 변경
        ResetCountdown.GetComponent<TextMeshProUGUI>().text = "현재 횟수 : " + curMoveCount.ToString();
        //

        if(easyStageClearCount >= 3)
            normalBlocker.SetActive(false);
        if(normalStageClearCount >= 3)
            hardBlocker.SetActive(false);
    }

    private void LateUpdate()
    {
        if(curMoveCount > 500)
            StartCoroutine("ResetStage"); // 프리팹 삭제 후 강제 리스타트
    }

    // 스테이지 버튼 클릭 시 발생하는 함수
    public void StageBtnClicked(int curStageNum)
    {
        this.curStageNum = curStageNum;
        // 버튼 이름들로 해당 스테이지 세팅
        SetButtonOfStagesByCurStageNum();
        // 인게임에서 현재 스테이지 표시
        SetCurrentStageText();
        // 스테이지 캔버스 종료 및 게임 캔버스 ON
        selectStageUI.SetActive(false);
        doGameUI.SetActive(true);
        // 비커 세팅 함수
        SetStage(curStageNum);
    }

    void SetCurrentStageText()
    {
        string stageText;
        if (curStageNum < 10) // tutorial
        {
            stageText = "Tutorial";
        }
        else if (curStageNum < 20) // easy
        {
            stageText = "Easy";
        }
        else if (curStageNum < 30) // normal
        {
            stageText = "Normal";
        }
        else // hard
        {
            stageText = "Hard";
        }

        currentStageText.SetText($"{stageText} - {curStageNum % 10 + 1}");
    }

    // set button and currentStageText
    private void SetButtonOfStagesByCurStageNum()
    {
        int buttonNum;
        GameObject buttonToSet;
        if(curStageNum < 10) // tutorial
        {
            buttonNum = curStageNum;
            buttonToSet = tutAnswerButtons[buttonNum];
        }
        else if(curStageNum < 20) // easy
        {
            buttonNum = curStageNum - 10;
            buttonToSet = easyAnswerButtons[buttonNum];
        }
        else if(curStageNum < 30) // normal
        {
            buttonNum = curStageNum - 20;
            buttonToSet = normalAnswerButtons[buttonNum];
        }
        else // hard
        {
            buttonNum = curStageNum - 30;
            buttonToSet = hardAnswerButtons[buttonNum];
        }

        AnswerSheetBtn = buttonToSet;

        //switch (curStageNum)
        //{
        //    #region 튜토리얼
        //    case 0: // 스테이지 버튼 이름으로 세팅하긴 하는데 다른 좋은 방식 추천받습니다.
        //        curStageNum = 0;
        //        AnswerSheetBtn = tutAnswerButtons[curStageNum];
        //        break;
        //    case "Tutorial2":
        //        curStageNum = 1;
        //        AnswerSheetBtn = tutAnswerButtons[curStageNum];
        //        break;
        //    #endregion
        //    #region 이지 모드
        //    case "Easy1":
        //        curStageNum = 10;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy2":
        //        curStageNum = 11;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy3":
        //        curStageNum = 12;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy4":
        //        curStageNum = 13;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy5":
        //        curStageNum = 14;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy6":
        //        curStageNum = 15;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy7":
        //        curStageNum = 16;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy8":
        //        curStageNum = 17;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy9":
        //        curStageNum = 18;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    case "Easy10":
        //        curStageNum = 19;
        //        AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
        //        break;
        //    #endregion
        //    #region 미디움 모드
        //    case "Mid1":
        //        curStageNum = 20;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid2":
        //        curStageNum = 21;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid3":
        //        curStageNum = 22;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid4":
        //        curStageNum = 23;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid5":
        //        curStageNum = 24;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid6":
        //        curStageNum = 25;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid7":
        //        curStageNum = 26;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid8":
        //        curStageNum = 27;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid9":
        //        curStageNum = 28;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    case "Mid10":
        //        curStageNum = 29;
        //        AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
        //        break;
        //    #endregion
        //    #region 하드 모드
        //    case "Hard1":
        //        curStageNum = 30;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard2":
        //        curStageNum = 31;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard3":
        //        curStageNum = 32;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard4":
        //        curStageNum = 33;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard5":
        //        curStageNum = 34;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard6":
        //        curStageNum = 35;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard7":
        //        curStageNum = 36;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard8":
        //        curStageNum = 37;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard9":
        //        curStageNum = 38;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //    case "Hard10":
        //        curStageNum = 39;
        //        AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
        //        break;
        //        #endregion
        //}
    }

    private void SetStage(int stageNum)
    {
        // 해당 스테이지 정보에 맞는 비커들을 화면에 세팅 해줘야 한다.
        ResetStageParameters();

        stageBeaker = new BeakerSetting(stageDataSO.stageDatas[stageNum].beakerSize,
            stageDataSO.stageDatas[stageNum].beakerRGB, stageDataSO.stageDatas[stageNum].answerBeaker);

        SetBeakerUI(stageBeaker);
    }

    private void SetBeakerUI(BeakerSetting beakerSetting)
    {
        // 비커 사이즈에 따라 순서대로 프리팹을 불러와 위치시킴
        for (int i = 0; i < beakerSetting.beakerSize.Count; i++) 
        {
            // 첫 위치에 프리팹 instantiate
            // GameObject uiInstance = Instantiate(BeakerPrefab11, beakerPosition); // 11 크기의 비커 프리팹 생성
            // 인스턴스화된 UI의 위치 조정 -> 그 다음에 추가되는 비커는 canvas의 가장 마지막 자식의 위치에서 일정 옆으로 띄워서 생성시키면 됨
            // RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
            // rectTransform.SetParent(Canvas.main.GetComponent<RectTransform>(), false); // 캔버스를 부모로 설정 -> 캔버스 이름 따라 변경

            // 해당 방식으로 활용하면 될 듯
            GameObject beakerInstance = Instantiate(stageDataSO.stageDatas[curStageNum].beakerPrefabs[i]);
            beakerInstance.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            if (i == beakerSetting.beakerSize.Count - 1) // 가장 마지막 비커는 이름을 Submit으로 변경
            {
                beakerInstance.transform.Find("Name").gameObject.SetActive(false);
                beakerInstance.transform.Find("Submit").gameObject.SetActive(true);
            }
            RectTransform rectTransform = beakerInstance.GetComponent<RectTransform>();
            rectTransform.SetParent(canvas_Beaker.GetComponent<RectTransform>(), false);
            if(beakerSetting.beakerSize.Count > 4)
            {
                if (beakerSetting.beakerSize[i] < 24)
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 170, -350 + ((float)beakerSetting.beakerSize[i] / 2f - 1f) * 44 , 0); // -> 로컬 위치는 Global에서 정하고 들어갈 것 / -350은 2칸짜리 비커 기준
                else
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 170, 36.3f, 0); // -> 로컬 위치는 Global에서 정하고 들어갈 것 //y36.3 으로 수정
            }
            else
            {
                if (beakerSetting.beakerSize[i] < 24)
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 200, -350 + ((float)beakerSetting.beakerSize[i] / 2f - 1f) * 44, 0); // -> 로컬 위치는 Global에서 정하고 들어갈 것 / -350은 2칸짜리 비커 기준 
                else
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 200, 36.3f, 0); // -> 로컬 위치는 Global에서 정하고 들어갈 것 //y36.3 으로 수정
            }


            beakerInstance.GetComponent<Button>().onClick.AddListener(() => BeakerSelected(beakerInstance.GetComponent<Button>()));

            Stack<char> charBeakerStack = new Stack<char>(beakerSetting.beakerStack[i]);
            int count = 0;
            while(charBeakerStack.Count > 0)
            {
                char RGB = charBeakerStack.Pop();
                switch(RGB)
                {
                    case 'R':
                        canvas_Beaker.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = colors[0];
                        break;
                    case 'G':
                        canvas_Beaker.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = colors[1];
                        break;
                    case 'B':
                        canvas_Beaker.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = colors[2];
                        break;
                }
                count++;
            }
            beakerPrefabsOnDisplay.Add(beakerInstance); // 생성된 비커들 저장 -> 나중에 destroy 해야됨
        }
        // 플레이어에게 보여줄 정답 비커 (버튼 역할 x)
        GameObject answerInstance = Instantiate(stageDataSO.stageDatas[curStageNum].beakerPrefabs[beakerSetting.beakerSize.Count - 1]); // 가장 마지막 비커는 infi 비커이므로 걍 가져와서 씀
        answerInstance.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = (beakerSetting.beakerSize.Count).ToString();
        answerInstance.transform.Find("Name").gameObject.SetActive(false);
        answerInstance.transform.Find("Answer").gameObject.SetActive(true);
        // 이 부분에 정답비커의 인디케이터를 키도록 하면 될 듯
        answerInstance.transform.Find("AnswerIndicator").gameObject.SetActive(true);
        RectTransform answerRectTransform = answerInstance.GetComponent<RectTransform>();
        answerRectTransform.SetParent(canvas_Beaker.GetComponent<RectTransform>(), false);
        
        if (beakerSetting.beakerSize.Count > 4)
            answerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + ((float)beakerSetting.beakerSize.Count) * 170, 36.3f, 0); // -> 로컬 위치는 Global에서 정하고 들어갈 것
        else
            answerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + ((float)beakerSetting.beakerSize.Count) * 200, 36.3f, 0); // -> 로컬 위치는 Global에서 정하고 들어갈 것

        Stack<char> answerCharSampleStack = new Stack<char>(beakerSetting.beakerAnswer);
        int c = 0;
        while (answerCharSampleStack.Count > 0)
        {
            c++;
            char RGB = answerCharSampleStack.Pop();
            switch (RGB)
            {
                case 'R':
                    canvas_Beaker.transform.GetChild(beakerSetting.beakerSize.Count).Find("Image" + (c).ToString()).GetComponent<Image>().color = colors[0];
                    break;
                case 'G':
                    canvas_Beaker.transform.GetChild(beakerSetting.beakerSize.Count).Find("Image" + (c).ToString()).GetComponent<Image>().color = colors[1];
                    break;
                case 'B':
                    canvas_Beaker.transform.GetChild(beakerSetting.beakerSize.Count).Find("Image" + (c).ToString()).GetComponent<Image>().color = colors[2];
                    break;
            }
        }
        beakerPrefabsOnDisplay.Add(answerInstance); // 생성된 정답 비커도 저장
    }

    public void BeakerSelected(Button button) // 제출용 비커의 이름은 404 << 로 할 것
    {
        if(firstBeakerSelected)
        {
            //secondSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            secondSelectedBeakerNum = Convert.ToInt32(button.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>().text) - 1;
            if(secondSelectedBeakerNum == firstSelectedBeakerNum) // 둘이 같으면 그냥 초기화
            {
                firstSelectedBeakerNum = 1995;
                secondSelectedBeakerNum = 1995;
                button.transform.Find("Indicator").gameObject.SetActive(false);
                firstBeakerSelected = false;
                return;
            }
            secondBeakerSelected = true;
        }
        else
        {
            //firstSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            firstSelectedBeakerNum = Convert.ToInt32(button.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>().text) - 1;
            if (stageBeaker.curBeakerAmount[firstSelectedBeakerNum] == 0) // 빈 비커를 선택했다면
            {
                firstSelectedBeakerNum = 1995;
                firstBeakerSelected = false;
                return;
            }
            // 선택 버튼의 indicator 표시
            button.transform.Find("Indicator").gameObject.SetActive(true);
            firstBeakerSelected = true;
        }
    }

    void MoveRGBToAnotherBeaker(int fromBeaker, int toBeaker, bool isUndo)
    {
        if (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker]) // 해당 번호의 비커가 비어있는 공간이 있을 것
        {
            int waterMoveAmount = 0;
            while (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker] // 빈 공간이 다 채워질때까지 from 쪽에서 옮겨담음 or
                && stageBeaker.curBeakerAmount[fromBeaker] > 0) // from쪽 비커의 현재 남은 RGB 수가 0 개가 될 때까지 진행 
            {
                char RGB = stageBeaker.beakerStack[fromBeaker].Pop();
                stageBeaker.beakerStack[toBeaker].Push(RGB); // from 비커의 맨 위를 확인해서 to 비커에게 집어넣음
                // UI 변경되는 것 (비커 색 추가되고 감소되는 것) 은 여기에서 그때마다 바로바로 체크해주는게 좋을듯
                switch (RGB)
                {
                    case 'R':
                        canvas_Beaker.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = colors[0];
                        break;
                    case 'G':
                        canvas_Beaker.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = colors[1];
                        break;
                    case 'B':
                        canvas_Beaker.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = colors[2];
                        break;
                }
                canvas_Beaker.transform.GetChild(fromBeaker).Find("Image" + (stageBeaker.curBeakerAmount[fromBeaker]).ToString()).GetComponent<Image>().color = Color.white;
                //
                stageBeaker.curBeakerAmount[fromBeaker]--; // from 비커의 숫자 한개 감소
                stageBeaker.curBeakerAmount[toBeaker]++; // to 비커의 숫자 한개 증가
                waterMoveAmount++;
                if(isUndo)
                {
                    if (waterMoveAmount == moveWaterAmount[moveWaterAmount.Count - 1])
                        break; // 옮겨졌던 만큼이 옮겨지면 멈춘다.
                }
            }

            // 현재 움직인 카운트 추가
            if (isUndo)
            {
                curMoveCount--;
                moveWaterAmount.RemoveAt(moveWaterAmount.Count - 1); // 맨 뒤의 옮겨졌던 양 삭제
            }
            else
            {
                curMoveCount++;
                moveWaterAmount.Add(waterMoveAmount);
            }

            //Play Pouring SFX
            soundManager.PlayPouringSFX();
        }
        // 빈 공간이 없으면 작동 안함
    }

    public void SubmitBtnClicked()
    {
        Stack<char> playerAnswerStack = new Stack<char>(stageBeaker.beakerStack[stageBeaker.playerAnswerBeakerNum]);
        Stack<char> AnswerStack = new Stack<char>(stageBeaker.beakerAnswer);
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
            gameClearUI.SetActive(true);
            PracticeNote.SetActive(false);
            noticeCanvas.SetActive(false);
            stageCleared[curStageNum] = true;
            if (curStageNum < 10 && !alreadyCleared[curStageNum]) // 튜토리얼
            {
                tutStageClearCount++;
                alreadyCleared[curStageNum] = true;
            }
            else if (curStageNum >= 10 && curStageNum < 20 && !alreadyCleared[curStageNum])
            {
                easyStageClearCount++;
                alreadyCleared[curStageNum] = true;
            }
            else if (curStageNum >= 20 && curStageNum < 30 && !alreadyCleared[curStageNum])
            {
                normalStageClearCount++;
                alreadyCleared[curStageNum] = true;
            }
            else if(curStageNum>=30 && !alreadyCleared[curStageNum])
            {
                hardStageClearCount++;
                alreadyCleared[curStageNum] = true;
            }

            if(playersChoice[curStageNum].Count != 0) 
            {
                if (playersChoice_Temp.Count < playersChoice[curStageNum].Count) // 최고기록이 있는데 그것보다 작으면 변경
                    playersChoice[curStageNum] = new List<Tuple<int, int>>(playersChoice_Temp);
            }
            else
            {
                playersChoice[curStageNum] = new List<Tuple<int, int>>(playersChoice_Temp); // 기록이 없으면 그냥 일단 넣기
            }

            // update counts in clearUI
            UpdateGameClearUI();
        }
        else 
        {
            // 클리어 못했으면 할 행동? UI? 여기에 넣어서 관리할 것

        }
    }
    public void UpdateGameClearUI()
    {
        TextMeshProUGUI restartCountText = TextsInGameClear.transform.Find("Restart Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerCountText = TextsInGameClear.transform.Find("PlayerCount Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI makerCountText = TextsInGameClear.transform.Find("MakerCount Text").GetComponent<TextMeshProUGUI>();

        restartCountText.SetText($"재시작 횟수 : {playersRestart[curStageNum]}");
        playerCountText.SetText($"내 풀이 횟수 : {playersChoice[curStageNum].Count}");
        makerCountText.SetText($"제작자 풀이 횟수 : {devAnswerCount[curStageNum]}");

        if(curStageNum == 0 || curStageNum % 10 >= offsetOfLastStagebyDifficulty)
        {
            nextStageButton.SetActive(false);
        }
        else
        {
            nextStageButton.SetActive(true);
        }
    }



    public void ResetBtnClicked(Button button) // 리셋 버튼과 연결
    {
        // 현재 만들어져 있는 비커 프리팹들 먼저 Destroy
        DestoryBeakerPrefabs();
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        // 클리어 ui의 버튼을 눌렀다면 스테이지 클리어 했으니 갯수 답안지 버튼 오픈 및 클리어 수 추가
        if (button.transform.Find("Clear") != null || button.transform.Find("Next") != null)
        {            
            // if click next button, activate current answersheet button / increase curStagenum. 
            if(button.transform.Find("Next") != null)
            {
                SetButtonOfStagesByCurStageNum();
                IncreaseCurStageNum();
                // 인게임에서 현재 스테이지 표시
                SetCurrentStageText();
            }
            AnswerSheetBtn.SetActive(true);
        }
        else // 플레이 화면에서 리셋 버튼 누르면
        {
            playersRestart[curStageNum]++; // 재시작 횟수 증가
        }
        // 스테이지 재시작
        StartCoroutine("ResetStage");
    }

    void IncreaseCurStageNum()
    {
        // not for tutorial
        if(curStageNum % 10 >= 5)
        {
            curStageNum = (curStageNum/10) * 10 + 10;
        }
        else
        {
            curStageNum++;
        }
    }

    IEnumerator ResetStage()
    {
        yield return null;
        // 스테이지 재시작
        SetStage(curStageNum);
    }
    #region 2차 수정 : 되돌리기 버튼
    public void UndoBtnClicked()
    {
        if(playersChoice_Temp.Count > 0) // 초기 상태일 때는 아무것도 안들어가 있을테니 들어가면 갯수 0이라 뻑남
        {
            // 맨 뒤에 넣어둔 플레이어 기록에서 from과 to를 반대로 뒤집어서 옮기고 맨 뒤 기록을 삭제함
            MoveRGBToAnotherBeaker(playersChoice_Temp[playersChoice_Temp.Count - 1].Item2, playersChoice_Temp[playersChoice_Temp.Count - 1].Item1, true);
            playersChoice_Temp.RemoveAt(playersChoice_Temp.Count - 1);
        }
    }
    #endregion
    public void GoBackBtnClicked(Button button) // 스테이지 창으로 돌아가기 버튼과 연결
    {
        // 현재 만들어져 있는 비커 프리팹들 먼저 Destroy
        DestoryBeakerPrefabs();
        // Stage 선택 캔버스 Active;
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        if (doGameUI.activeSelf)
        {
            PracticeNote.SetActive(false);
            noticeCanvas.SetActive(false);
            doGameUI.SetActive(false);
        }
        // 클리어 ui의 버튼을 눌렀다면 스테이지 클리어 했으니 갯수 답안지 버튼 오픈 및 클리어 수 추가
        if (button.transform.Find("Clear") != null)
        {
            SetButtonOfStagesByCurStageNum();
            AnswerSheetBtn.SetActive(true);
        }
        //
        selectStageUI.SetActive(true);
    }


    public void AnswerPanelBtnClicked(Button button)
    {
        // AnswerPanel 버튼 자식 이름이 숫자로 되어있음
        int playerAnswerStageNum = Convert.ToInt32(button.transform.GetChild(0).name);
        // 먼저 AnswerPanel Text들 초기화
        Transform answerPanel = AnswerPanel.transform.Find("AnswerTexts");

        //while (answerPanel.childCount > 0) // -> 이렇게 해두면 다음 프레임에 지워져서 childCount 가 계속 유지됨, 절대 안지워짐, 이렇게 할거면 코루틴으로 돌릴 것
        for (int i = 0; i < answerPanel.childCount; i++)
        {
            Destroy(answerPanel.GetChild(i).gameObject); // 남은 child 없을 때 까지 삭제
        }
        // 패널에 플레이어의 풀이 순서 생성
        for (int i = 0; i < playersChoice[playerAnswerStageNum].Count; i++)
        {
            GameObject ansText = Instantiate(PlayerAnswerText);
            ansText.GetComponent<TextMeshProUGUI>().text = (i + 1).ToString() + ". "
                + (playersChoice[playerAnswerStageNum][i].Item1 + 1).ToString() + " -> " + (playersChoice[playerAnswerStageNum][i].Item2 + 1).ToString();
            ansText.transform.SetParent(answerPanel.transform, false);
        }
        // 답안 패널에 나올 다른 글자들 조정
        answerPanel = AnswerPanel.transform.Find("Texts").transform;
        string StageName = "";
        if (playerAnswerStageNum < 10) // 튜토리얼
            StageName = "선택한 스테이지 : 튜토리얼" + (playerAnswerStageNum + 1).ToString();
        else if (playerAnswerStageNum >= 10 && playerAnswerStageNum < 20) // easy
            StageName = "선택한 스테이지 : 쉬움" + (playerAnswerStageNum + 1 - 10).ToString();
        else if (playerAnswerStageNum >= 20 && playerAnswerStageNum < 30) // normal
            StageName = "선택한 스테이지 : 중간" + (playerAnswerStageNum + 1 - 20).ToString();
        else if (playerAnswerStageNum >= 30) // hard
            StageName = "선택한 스테이지 : 어려움" + (playerAnswerStageNum + 1 - 30).ToString();
        answerPanel.Find("Stage Text").GetComponent<TextMeshProUGUI>().text = StageName;
        answerPanel.Find("Count Text").GetComponent<TextMeshProUGUI>().text = "내 풀이 횟수 : " + playersChoice[playerAnswerStageNum].Count.ToString();
        answerPanel.Find("CorrectCount Text").GetComponent<TextMeshProUGUI>().text = "개발자 풀이 횟수 : " + devAnswerCount[playerAnswerStageNum].ToString();
        answerPanel.Find("Restart Text").GetComponent<TextMeshProUGUI>().text = "재시작한 횟수 : " + playersRestart[playerAnswerStageNum].ToString();
    }

    public void SelectCancelBtnClicked() // 선택 취소 버튼과 연결 -> 현재 같은 버튼 두번 누르면 리셋이라 안씀, 마지막에 지울 것
    {
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995; // 1995가 의미가 있는 것은 아니고 그냥 리셋한다는 의미로 넣어뒀습니다. 0이라고 하면 0번째 버튼이랑 매핑돼버려서...
        // 취소 버튼은 다시 Disable로 변경할 것

    }

    private void DestoryBeakerPrefabs()
    {
        // 현재 만들어져 있는 비커 프리팹들 먼저 Destroy
        for (int i = 0; i < beakerPrefabsOnDisplay.Count; i++)
        {
            Destroy(beakerPrefabsOnDisplay[i]);
        }
        beakerPrefabsOnDisplay.Clear(); // 재사용을 위해 비워두기
    }

    private void ResetStageParameters()
    {
        curMoveCount = 0;
        moveWaterAmount.Clear(); // 옮긴 카운트 삭제했으니 옮겨졌던 데이터도 물론 필요없다.
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995;
        secondBeakerSelected = false;
        secondSelectedBeakerNum = 1995; // 첫 시작 시 스테이지에서 활용되는 파라미터들 초기화
    }

    public void QuitBtnClicked()
    {
        Application.Quit();
    }

    public void SaveBtnClicked()
    {
        var (fromList, toList, restartList, clearList) = MakeListForSave();
        dataManager.SaveData(fromList, toList, restartList, clearList);
    }

    private (List<List<int>>, List<List<int>>, List<int>, List<bool>) MakeListForSave()
    {
        List<List<int>> fromList = new List<List<int>>();
        List<List<int>> toList = new List<List<int>>();
        List<int> restartList = new List<int>();
        List<bool> isCleared = new List<bool>();
        // 각 리스트들에 리스트 빈거 넣어주기
        for (int i = 0; i < totalStageNum; i++) // 스테이지 갯수만큼 답지 List를 생성해서 미리 넣어준다.
        {
            fromList.Add(new List<int>());
            toList.Add(new List<int>());
            restartList.Add(0);
            isCleared.Add(false);
        }
        //
        for (int i=0; i<totalStageNum; i++)
        {
            for(int j = 0; j < playersChoice[i].Count; j++)
            {
                fromList[i].Add(playersChoice[i][j].Item1);
                toList[i].Add(playersChoice[i][j].Item2);
            }
            restartList[i] = (playersRestart[i]);
            isCleared[i] = (stageCleared[i]);
        }
        return (fromList, toList, restartList, isCleared);
    }

    public void NoticeBtnClicked()
    {
        noticeCanvas.SetActive(!noticeCanvas.activeInHierarchy);
    }
}