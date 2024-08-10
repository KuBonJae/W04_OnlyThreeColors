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
    // ��Ŀ�� ���� �����յ��� [SerializeField]�� �̸� �޾Ƶд�
    // StageData�� List �߰��ؼ� ���ʿ� �����յ� �־���� �� �� ���⵵
    [Header("SriptableObject")]
    [SerializeField]
    private StageDataSO stageDataSO;
    //
    // ���õ� ��Ŀ ������
    BeakerSetting stageBeaker;
    //
    // ��Ŀ ���� ���� & ���õ� ��Ŀ ��ȣ
    bool firstBeakerSelected = false;
    int firstSelectedBeakerNum = 1995;
    bool secondBeakerSelected = false;
    int secondSelectedBeakerNum = 1995;
    //
    // ��� ���� ��ư Ŭ�� ����
    bool submitAnswer = false; // -> update ������ ��� ������ üũ���� �����Ƿ� ���� ���� ����, �������� ���� ��
    //
    // �������� ó�� �����Ǵ� ��ġ
    Vector3 beakerPosition = Vector3.zero;
    //
    // �������� �� �÷��̾��� �ְ��� Ǯ�̰� ����� List
    public List<List<Tuple<int, int>>> playersChoice;
    List<Tuple<int, int>> playersChoice_Temp; // �ְ��ϰ� ���� �׶��׶� Ǯ���� List
    List<int> moveWaterAmount; // �Ű��� �� �� -> �ǵ����� �� �� �̰� �����ؼ� �Ű��� �� �� ��ŭ�� �Űܾ� ��
    List<bool> stageCleared;
    //
    // �������� �� �÷��̾��� ����ŸƮ ��ư Ƚ��(Ŭ���� ui���� ������ ����ŸƮ ����)
    List<int> playersRestart;
    // �������� �� �����ڰ� �����ϴ� Ǯ�� Ƚ��
    public List<int> devAnswerCount;
    //
    // �ִ� ������ �� �ִ� Ƚ��, ���� ������ Ƚ��
    const int maxMoveCount = 100;
    int curMoveCount = 0;
    //
    // �� �������� ���� << �������� ���� ���� ���� ���� ���� ��
    int totalStageNum = 40;
    //
    // �������� ��ư�� �̸� �־�α� << ���߿� �������� Ŭ����� ���� ��ư ��� ��
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
    // Ŭ����� �������� �� -> 3 �̻��� �Ǹ� ���� �������� ��ư�� ����
    private int tutStageClearCount = 0;
    private int easyStageClearCount = 0;
    private int normalStageClearCount = 0;
    private int hardStageClearCount = 0;
    private List<bool> alreadyCleared;
    //
    // ���� �������� ��ư ���� ����, �̹� ���µ� ���¸� ���� �ٽ� SetActive(true) �� �ʿ� ������ -> Tut�� �׽� ����
    private bool easyStageOpened = false;
    private bool normalStageOpened = false;
    private bool hardStageOpened = false;
    //
    // ���� �������� ��ȣ
    public int curStageNum;
    //
    // ĵ���� �̸� �޾Ƶα�
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
    // ���� ���� �Ǿ� �ִ� ��Ŀ �����յ� ����
    private List<GameObject> beakerPrefabsOnDisplay = new List<GameObject>();
    //
    // ���� �������� Ŭ���� �� ���µ� ������ ��ư �̸� �޾Ƶα�
    private GameObject AnswerSheetBtn;
    //
    // ���� ����� ���� ��ũ��Ʈ
    SoundManager soundManager;
    //
    // ������ ����� �Ŵ���
    public DataManager dataManager;
    //
    //
    // �÷� �ڵ�
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
        for (int i = 0; i < totalStageNum; i++) // �������� ������ŭ ���� List�� �����ؼ� �̸� �־��ش�.
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
                Debug.Log("���̺� ������ ����");
                return;
            }
            
            for (int i = 0; i < totalStageNum; i++)
            {
                for(int j = 0; j < fromList[i].Count;j++)
                {
                    // i ���� ���������� j��° �÷��� �� �� from, to ���
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
        // �÷� �߰�
        colors = new Color[3];
        ColorUtility.TryParseHtmlString("#FF8888", out colors[0]);
        ColorUtility.TryParseHtmlString("#88FF88", out colors[1]);
        ColorUtility.TryParseHtmlString("#8888FF", out colors[2]);
    }

    // Update is called once per frame
    void Update()
    {
        if(firstBeakerSelected) // �ű� ��Ŀ�� ���� �Ǿ��°�?
        {
            if (secondBeakerSelected) // �Ű��� ��Ŀ�� ���� �Ǿ��°�?
            {
                // �Ѵ� ���� �� �ϴ� selected �ٷ� �ʱ�ȭ
                firstBeakerSelected = secondBeakerSelected = false;
                EventSystem.current.SetSelectedGameObject(null); // ��ư ���õ� �� ���� << �̰� ���� ���ϸ� ���� ��ư Ŭ���� �������� �ȵ�
                canvas_Beaker.transform.GetChild(firstSelectedBeakerNum).Find("Indicator").gameObject.SetActive(false);
                if (curMoveCount == 0) // �ش� ������������ ó�� ����� �÷��̾��� Move
                {
                    //playersChoice[curStageNum].Clear(); // ���� �� ��� << ���� �ְ��� ��������� ���ű� ������ ���°� ���� �ȵ�
                    playersChoice_Temp.Clear(); // �ӽ÷� ���Ǵ� �÷��̾� ��� List ����
                }
                //playersChoice[curStageNum].Add(new Tuple<int,int>(firstSelectedBeakerNum, secondSelectedBeakerNum));
                playersChoice_Temp.Add(new Tuple<int,int>(firstSelectedBeakerNum, secondSelectedBeakerNum));
                
                MoveRGBToAnotherBeaker(firstSelectedBeakerNum, secondSelectedBeakerNum, false);
            }
        }

        // ���� ���� Ƚ���� 0 �̸��̸� ��� (100������ ����)
        if(curMoveCount > 500)
        {
            DestoryBeakerPrefabs();
            // �ش� �κ��� LateUpdate�� ����
            //StartCoroutine("ResetStage"); // ������ ���� �� ���� ����ŸƮ
        }
        //
        // ���� Ƚ�� ui ����
        ResetCountdown.GetComponent<TextMeshProUGUI>().text = "���� Ƚ�� : " + curMoveCount.ToString();
        //

        if(easyStageClearCount >= 3)
            normalBlocker.SetActive(false);
        if(normalStageClearCount >= 3)
            hardBlocker.SetActive(false);
    }

    private void LateUpdate()
    {
        if(curMoveCount > 500)
            StartCoroutine("ResetStage"); // ������ ���� �� ���� ����ŸƮ
    }

    // �������� ��ư Ŭ�� �� �߻��ϴ� �Լ�
    public void StageBtnClicked(int curStageNum)
    {
        this.curStageNum = curStageNum;
        // ��ư �̸���� �ش� �������� ����
        SetButtonOfStagesByCurStageNum();
        // �ΰ��ӿ��� ���� �������� ǥ��
        SetCurrentStageText();
        // �������� ĵ���� ���� �� ���� ĵ���� ON
        selectStageUI.SetActive(false);
        doGameUI.SetActive(true);
        // ��Ŀ ���� �Լ�
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
        //    #region Ʃ�丮��
        //    case 0: // �������� ��ư �̸����� �����ϱ� �ϴµ� �ٸ� ���� ��� ��õ�޽��ϴ�.
        //        curStageNum = 0;
        //        AnswerSheetBtn = tutAnswerButtons[curStageNum];
        //        break;
        //    case "Tutorial2":
        //        curStageNum = 1;
        //        AnswerSheetBtn = tutAnswerButtons[curStageNum];
        //        break;
        //    #endregion
        //    #region ���� ���
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
        //    #region �̵�� ���
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
        //    #region �ϵ� ���
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
        // �ش� �������� ������ �´� ��Ŀ���� ȭ�鿡 ���� ����� �Ѵ�.
        ResetStageParameters();

        stageBeaker = new BeakerSetting(stageDataSO.stageDatas[stageNum].beakerSize,
            stageDataSO.stageDatas[stageNum].beakerRGB, stageDataSO.stageDatas[stageNum].answerBeaker);

        SetBeakerUI(stageBeaker);
    }

    private void SetBeakerUI(BeakerSetting beakerSetting)
    {
        // ��Ŀ ����� ���� ������� �������� �ҷ��� ��ġ��Ŵ
        for (int i = 0; i < beakerSetting.beakerSize.Count; i++) 
        {
            // ù ��ġ�� ������ instantiate
            // GameObject uiInstance = Instantiate(BeakerPrefab11, beakerPosition); // 11 ũ���� ��Ŀ ������ ����
            // �ν��Ͻ�ȭ�� UI�� ��ġ ���� -> �� ������ �߰��Ǵ� ��Ŀ�� canvas�� ���� ������ �ڽ��� ��ġ���� ���� ������ ����� ������Ű�� ��
            // RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
            // rectTransform.SetParent(Canvas.main.GetComponent<RectTransform>(), false); // ĵ������ �θ�� ���� -> ĵ���� �̸� ���� ����

            // �ش� ������� Ȱ���ϸ� �� ��
            GameObject beakerInstance = Instantiate(stageDataSO.stageDatas[curStageNum].beakerPrefabs[i]);
            beakerInstance.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            if (i == beakerSetting.beakerSize.Count - 1) // ���� ������ ��Ŀ�� �̸��� Submit���� ����
            {
                beakerInstance.transform.Find("Name").gameObject.SetActive(false);
                beakerInstance.transform.Find("Submit").gameObject.SetActive(true);
            }
            RectTransform rectTransform = beakerInstance.GetComponent<RectTransform>();
            rectTransform.SetParent(canvas_Beaker.GetComponent<RectTransform>(), false);
            if(beakerSetting.beakerSize.Count > 4)
            {
                if (beakerSetting.beakerSize[i] < 24)
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 170, -350 + ((float)beakerSetting.beakerSize[i] / 2f - 1f) * 44 , 0); // -> ���� ��ġ�� Global���� ���ϰ� �� �� / -350�� 2ĭ¥�� ��Ŀ ����
                else
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 170, 36.3f, 0); // -> ���� ��ġ�� Global���� ���ϰ� �� �� //y36.3 ���� ����
            }
            else
            {
                if (beakerSetting.beakerSize[i] < 24)
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 200, -350 + ((float)beakerSetting.beakerSize[i] / 2f - 1f) * 44, 0); // -> ���� ��ġ�� Global���� ���ϰ� �� �� / -350�� 2ĭ¥�� ��Ŀ ���� 
                else
                    beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 200, 36.3f, 0); // -> ���� ��ġ�� Global���� ���ϰ� �� �� //y36.3 ���� ����
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
            beakerPrefabsOnDisplay.Add(beakerInstance); // ������ ��Ŀ�� ���� -> ���߿� destroy �ؾߵ�
        }
        // �÷��̾�� ������ ���� ��Ŀ (��ư ���� x)
        GameObject answerInstance = Instantiate(stageDataSO.stageDatas[curStageNum].beakerPrefabs[beakerSetting.beakerSize.Count - 1]); // ���� ������ ��Ŀ�� infi ��Ŀ�̹Ƿ� �� �����ͼ� ��
        answerInstance.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = (beakerSetting.beakerSize.Count).ToString();
        answerInstance.transform.Find("Name").gameObject.SetActive(false);
        answerInstance.transform.Find("Answer").gameObject.SetActive(true);
        // �� �κп� �����Ŀ�� �ε������͸� Ű���� �ϸ� �� ��
        answerInstance.transform.Find("AnswerIndicator").gameObject.SetActive(true);
        RectTransform answerRectTransform = answerInstance.GetComponent<RectTransform>();
        answerRectTransform.SetParent(canvas_Beaker.GetComponent<RectTransform>(), false);
        
        if (beakerSetting.beakerSize.Count > 4)
            answerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + ((float)beakerSetting.beakerSize.Count) * 170, 36.3f, 0); // -> ���� ��ġ�� Global���� ���ϰ� �� ��
        else
            answerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + ((float)beakerSetting.beakerSize.Count) * 200, 36.3f, 0); // -> ���� ��ġ�� Global���� ���ϰ� �� ��

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
        beakerPrefabsOnDisplay.Add(answerInstance); // ������ ���� ��Ŀ�� ����
    }

    public void BeakerSelected(Button button) // ����� ��Ŀ�� �̸��� 404 << �� �� ��
    {
        if(firstBeakerSelected)
        {
            //secondSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            secondSelectedBeakerNum = Convert.ToInt32(button.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>().text) - 1;
            if(secondSelectedBeakerNum == firstSelectedBeakerNum) // ���� ������ �׳� �ʱ�ȭ
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
            if (stageBeaker.curBeakerAmount[firstSelectedBeakerNum] == 0) // �� ��Ŀ�� �����ߴٸ�
            {
                firstSelectedBeakerNum = 1995;
                firstBeakerSelected = false;
                return;
            }
            // ���� ��ư�� indicator ǥ��
            button.transform.Find("Indicator").gameObject.SetActive(true);
            firstBeakerSelected = true;
        }
    }

    void MoveRGBToAnotherBeaker(int fromBeaker, int toBeaker, bool isUndo)
    {
        if (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker]) // �ش� ��ȣ�� ��Ŀ�� ����ִ� ������ ���� ��
        {
            int waterMoveAmount = 0;
            while (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker] // �� ������ �� ä���������� from �ʿ��� �Űܴ��� or
                && stageBeaker.curBeakerAmount[fromBeaker] > 0) // from�� ��Ŀ�� ���� ���� RGB ���� 0 ���� �� ������ ���� 
            {
                char RGB = stageBeaker.beakerStack[fromBeaker].Pop();
                stageBeaker.beakerStack[toBeaker].Push(RGB); // from ��Ŀ�� �� ���� Ȯ���ؼ� to ��Ŀ���� �������
                // UI ����Ǵ� �� (��Ŀ �� �߰��ǰ� ���ҵǴ� ��) �� ���⿡�� �׶����� �ٷιٷ� üũ���ִ°� ������
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
                stageBeaker.curBeakerAmount[fromBeaker]--; // from ��Ŀ�� ���� �Ѱ� ����
                stageBeaker.curBeakerAmount[toBeaker]++; // to ��Ŀ�� ���� �Ѱ� ����
                waterMoveAmount++;
                if(isUndo)
                {
                    if (waterMoveAmount == moveWaterAmount[moveWaterAmount.Count - 1])
                        break; // �Ű����� ��ŭ�� �Ű����� �����.
                }
            }

            // ���� ������ ī��Ʈ �߰�
            if (isUndo)
            {
                curMoveCount--;
                moveWaterAmount.RemoveAt(moveWaterAmount.Count - 1); // �� ���� �Ű����� �� ����
            }
            else
            {
                curMoveCount++;
                moveWaterAmount.Add(waterMoveAmount);
            }

            //Play Pouring SFX
            soundManager.PlayPouringSFX();
        }
        // �� ������ ������ �۵� ����
    }

    public void SubmitBtnClicked()
    {
        Stack<char> playerAnswerStack = new Stack<char>(stageBeaker.beakerStack[stageBeaker.playerAnswerBeakerNum]);
        Stack<char> AnswerStack = new Stack<char>(stageBeaker.beakerAnswer);
        bool stageClear = true; // �� �� ������ �������� Ŭ����� üũ�� boolean

        if(AnswerStack.Count != playerAnswerStack.Count)
        {
            // ������ �翬�� �ٸ��� ���а� Ȯ��
            stageClear = false;
        }
        else
        {
            while(playerAnswerStack.Count > 0) // ������ ���� ������ �����ϴ� �ϳ��� üũ�ص� ����
            {
                if(playerAnswerStack.Peek() != AnswerStack.Peek()) // ���� �� ���� �ٸ��� �ٸ� string �̶�� ���̴�
                {
                    // �����ߴٰ� ǥ��
                    stageClear = false;
                    break;
                }
                else
                {
                    playerAnswerStack.Pop();
                    AnswerStack.Pop(); // �� �տ� ��������
                }
                // �� �տ��� ������ ���ϴ°� �ݺ��ϴٰ� ������ �ݺ��ߴµ� ��� �����ߴٸ� stageClear�� �״�� true�� �� ���̴�
            }
        }

        if(stageClear)
        {
            // �������� Ŭ���� ĵ���� SetActive(true)
            gameClearUI.SetActive(true);
            PracticeNote.SetActive(false);
            noticeCanvas.SetActive(false);
            stageCleared[curStageNum] = true;
            if (curStageNum < 10 && !alreadyCleared[curStageNum]) // Ʃ�丮��
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
                if (playersChoice_Temp.Count < playersChoice[curStageNum].Count) // �ְ����� �ִµ� �װͺ��� ������ ����
                    playersChoice[curStageNum] = new List<Tuple<int, int>>(playersChoice_Temp);
            }
            else
            {
                playersChoice[curStageNum] = new List<Tuple<int, int>>(playersChoice_Temp); // ����� ������ �׳� �ϴ� �ֱ�
            }

            // update counts in clearUI
            UpdateGameClearUI();
        }
        else 
        {
            // Ŭ���� �������� �� �ൿ? UI? ���⿡ �־ ������ ��

        }
    }
    public void UpdateGameClearUI()
    {
        TextMeshProUGUI restartCountText = TextsInGameClear.transform.Find("Restart Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerCountText = TextsInGameClear.transform.Find("PlayerCount Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI makerCountText = TextsInGameClear.transform.Find("MakerCount Text").GetComponent<TextMeshProUGUI>();

        restartCountText.SetText($"����� Ƚ�� : {playersRestart[curStageNum]}");
        playerCountText.SetText($"�� Ǯ�� Ƚ�� : {playersChoice[curStageNum].Count}");
        makerCountText.SetText($"������ Ǯ�� Ƚ�� : {devAnswerCount[curStageNum]}");

        if(curStageNum == 0 || curStageNum % 10 >= offsetOfLastStagebyDifficulty)
        {
            nextStageButton.SetActive(false);
        }
        else
        {
            nextStageButton.SetActive(true);
        }
    }



    public void ResetBtnClicked(Button button) // ���� ��ư�� ����
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy
        DestoryBeakerPrefabs();
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        // Ŭ���� ui�� ��ư�� �����ٸ� �������� Ŭ���� ������ ���� ����� ��ư ���� �� Ŭ���� �� �߰�
        if (button.transform.Find("Clear") != null || button.transform.Find("Next") != null)
        {            
            // if click next button, activate current answersheet button / increase curStagenum. 
            if(button.transform.Find("Next") != null)
            {
                SetButtonOfStagesByCurStageNum();
                IncreaseCurStageNum();
                // �ΰ��ӿ��� ���� �������� ǥ��
                SetCurrentStageText();
            }
            AnswerSheetBtn.SetActive(true);
        }
        else // �÷��� ȭ�鿡�� ���� ��ư ������
        {
            playersRestart[curStageNum]++; // ����� Ƚ�� ����
        }
        // �������� �����
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
        // �������� �����
        SetStage(curStageNum);
    }
    #region 2�� ���� : �ǵ����� ��ư
    public void UndoBtnClicked()
    {
        if(playersChoice_Temp.Count > 0) // �ʱ� ������ ���� �ƹ��͵� �ȵ� �����״� ���� ���� 0�̶� ����
        {
            // �� �ڿ� �־�� �÷��̾� ��Ͽ��� from�� to�� �ݴ�� ����� �ű�� �� �� ����� ������
            MoveRGBToAnotherBeaker(playersChoice_Temp[playersChoice_Temp.Count - 1].Item2, playersChoice_Temp[playersChoice_Temp.Count - 1].Item1, true);
            playersChoice_Temp.RemoveAt(playersChoice_Temp.Count - 1);
        }
    }
    #endregion
    public void GoBackBtnClicked(Button button) // �������� â���� ���ư��� ��ư�� ����
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy
        DestoryBeakerPrefabs();
        // Stage ���� ĵ���� Active;
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        if (doGameUI.activeSelf)
        {
            PracticeNote.SetActive(false);
            noticeCanvas.SetActive(false);
            doGameUI.SetActive(false);
        }
        // Ŭ���� ui�� ��ư�� �����ٸ� �������� Ŭ���� ������ ���� ����� ��ư ���� �� Ŭ���� �� �߰�
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
        // AnswerPanel ��ư �ڽ� �̸��� ���ڷ� �Ǿ�����
        int playerAnswerStageNum = Convert.ToInt32(button.transform.GetChild(0).name);
        // ���� AnswerPanel Text�� �ʱ�ȭ
        Transform answerPanel = AnswerPanel.transform.Find("AnswerTexts");

        //while (answerPanel.childCount > 0) // -> �̷��� �صθ� ���� �����ӿ� �������� childCount �� ��� ������, ���� ��������, �̷��� �ҰŸ� �ڷ�ƾ���� ���� ��
        for (int i = 0; i < answerPanel.childCount; i++)
        {
            Destroy(answerPanel.GetChild(i).gameObject); // ���� child ���� �� ���� ����
        }
        // �гο� �÷��̾��� Ǯ�� ���� ����
        for (int i = 0; i < playersChoice[playerAnswerStageNum].Count; i++)
        {
            GameObject ansText = Instantiate(PlayerAnswerText);
            ansText.GetComponent<TextMeshProUGUI>().text = (i + 1).ToString() + ". "
                + (playersChoice[playerAnswerStageNum][i].Item1 + 1).ToString() + " -> " + (playersChoice[playerAnswerStageNum][i].Item2 + 1).ToString();
            ansText.transform.SetParent(answerPanel.transform, false);
        }
        // ��� �гο� ���� �ٸ� ���ڵ� ����
        answerPanel = AnswerPanel.transform.Find("Texts").transform;
        string StageName = "";
        if (playerAnswerStageNum < 10) // Ʃ�丮��
            StageName = "������ �������� : Ʃ�丮��" + (playerAnswerStageNum + 1).ToString();
        else if (playerAnswerStageNum >= 10 && playerAnswerStageNum < 20) // easy
            StageName = "������ �������� : ����" + (playerAnswerStageNum + 1 - 10).ToString();
        else if (playerAnswerStageNum >= 20 && playerAnswerStageNum < 30) // normal
            StageName = "������ �������� : �߰�" + (playerAnswerStageNum + 1 - 20).ToString();
        else if (playerAnswerStageNum >= 30) // hard
            StageName = "������ �������� : �����" + (playerAnswerStageNum + 1 - 30).ToString();
        answerPanel.Find("Stage Text").GetComponent<TextMeshProUGUI>().text = StageName;
        answerPanel.Find("Count Text").GetComponent<TextMeshProUGUI>().text = "�� Ǯ�� Ƚ�� : " + playersChoice[playerAnswerStageNum].Count.ToString();
        answerPanel.Find("CorrectCount Text").GetComponent<TextMeshProUGUI>().text = "������ Ǯ�� Ƚ�� : " + devAnswerCount[playerAnswerStageNum].ToString();
        answerPanel.Find("Restart Text").GetComponent<TextMeshProUGUI>().text = "������� Ƚ�� : " + playersRestart[playerAnswerStageNum].ToString();
    }

    public void SelectCancelBtnClicked() // ���� ��� ��ư�� ���� -> ���� ���� ��ư �ι� ������ �����̶� �Ⱦ�, �������� ���� ��
    {
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995; // 1995�� �ǹ̰� �ִ� ���� �ƴϰ� �׳� �����Ѵٴ� �ǹ̷� �־�׽��ϴ�. 0�̶�� �ϸ� 0��° ��ư�̶� ���εŹ�����...
        // ��� ��ư�� �ٽ� Disable�� ������ ��

    }

    private void DestoryBeakerPrefabs()
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy
        for (int i = 0; i < beakerPrefabsOnDisplay.Count; i++)
        {
            Destroy(beakerPrefabsOnDisplay[i]);
        }
        beakerPrefabsOnDisplay.Clear(); // ������ ���� ����α�
    }

    private void ResetStageParameters()
    {
        curMoveCount = 0;
        moveWaterAmount.Clear(); // �ű� ī��Ʈ ���������� �Ű����� �����͵� ���� �ʿ����.
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995;
        secondBeakerSelected = false;
        secondSelectedBeakerNum = 1995; // ù ���� �� ������������ Ȱ��Ǵ� �Ķ���͵� �ʱ�ȭ
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
        // �� ����Ʈ�鿡 ����Ʈ ��� �־��ֱ�
        for (int i = 0; i < totalStageNum; i++) // �������� ������ŭ ���� List�� �����ؼ� �̸� �־��ش�.
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