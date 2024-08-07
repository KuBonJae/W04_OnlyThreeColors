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
    // �������� �� �÷��̾��� �ֱ� Ǯ�̰� ����� List
    List<List<Tuple<int, int>>> playersChoice;
    // �������� �� �÷��̾��� ����ŸƮ ��ư Ƚ��(Ŭ���� ui���� ������ ����ŸƮ ����)
    List<int> playersRestart = new List<int>();
    // �������� �� �����ڰ� �����ϴ� Ǯ�� Ƚ��
    public List<int> devAnswerCount;
    //
    // �ִ� ������ �� �ִ� Ƚ��, ���� ������ Ƚ��
    const int maxMoveCount = 100;
    int curMoveCount = 0;
    //
    // �� �������� ���� << �������� ���� ���� ���� ���� ���� ��
    int totalStageNum = 32;
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
    //
    // ���� �������� ��ư ���� ����, �̹� ���µ� ���¸� ���� �ٽ� SetActive(true) �� �ʿ� ������ -> Tut�� �׽� ����
    private bool easyStageOpened = false;
    private bool normalStageOpened = false;
    private bool hardStageOpened = false;
    //
    // ���� �������� ��ȣ
    int curStageNum;
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
    //
    // ���� ���� �Ǿ� �ִ� ��Ŀ �����յ� ����
    private List<GameObject> beakerPrefabsOnDisplay = new List<GameObject>();
    //
    // ���� �������� Ŭ���� �� ���µ� ������ ��ư �̸� �޾Ƶα�
    private GameObject AnswerSheetBtn;
    //
    // ���� ����� ���� ��ũ��Ʈ
    SoundManager soundManager;

    void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        playersChoice = new List<List<Tuple<int, int>>>();
        for(int i=0;i< totalStageNum;i++) // �������� ������ŭ ���� List<Tuple> �� �����ؼ� �̸� �־��ش�.
        {
            playersChoice.Add(new List<Tuple<int, int>>());
        }
        for(int i=0;i<40;i++)
        {
            playersRestart.Add(0);
        }
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
                playersChoice[curStageNum].Add(new Tuple<int,int>(firstSelectedBeakerNum, secondSelectedBeakerNum));
                MoveRGBToAnotherBeaker(firstSelectedBeakerNum, secondSelectedBeakerNum);
            }
        }

        // ���� ���� Ƚ���� 0 �̸��̸� ��� (100������ ����)
        if(curMoveCount > 100)
        {
            DestoryBeakerPrefabs();
            StartCoroutine("ResetStage"); // ������ ���� �� ���� ����ŸƮ
        }
        //
        // ���� Ƚ�� ui ����
        ResetCountdown.GetComponent<TextMeshProUGUI>().text = "���� Ƚ�� : " + (maxMoveCount - curMoveCount).ToString();
        //
    }

    // �������� ��ư Ŭ�� �� �߻��ϴ� �Լ�
    public void StageBtnClicked(Button button)
    {
        // ��ư �̸���� �ش� �������� ����
        switch (button.gameObject.name)
        {
            #region Ʃ�丮��
            case "Tutorial1": // �������� ��ư �̸����� �����ϱ� �ϴµ� �ٸ� ���� ��� ��õ�޽��ϴ�.
                curStageNum = 0;
                AnswerSheetBtn = tutAnswerButtons[curStageNum];
                break;
            case "Tutorial2":
                curStageNum = 1;
                AnswerSheetBtn = tutAnswerButtons[curStageNum];
                break;
            #endregion
            #region ���� ���
            case "Easy1":
                curStageNum = 10;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy2":
                curStageNum = 11;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy3":
                curStageNum = 12;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy4":
                curStageNum = 13;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy5":
                curStageNum = 14;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy6":
                curStageNum = 15;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy7":
                curStageNum = 16;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy8":
                curStageNum = 17;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy9":
                curStageNum = 18;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            case "Easy10":
                curStageNum = 19;
                AnswerSheetBtn = easyAnswerButtons[curStageNum - 10];
                break;
            #endregion
            #region �̵�� ���
            case "Mid1":
                curStageNum = 20;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid2":
                curStageNum = 21;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid3":
                curStageNum = 22;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid4":
                curStageNum = 23;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid5":
                curStageNum = 24;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid6":
                curStageNum = 25;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid7":
                curStageNum = 26;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid8":
                curStageNum = 27;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid9":
                curStageNum = 28;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            case "Mid10":
                curStageNum = 29;
                AnswerSheetBtn = normalAnswerButtons[curStageNum - 20];
                break;
            #endregion
            #region �ϵ� ���
            case "Hard1":
                curStageNum = 30;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard2":
                curStageNum = 31;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard3":
                curStageNum = 32;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard4":
                curStageNum = 33;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard5":
                curStageNum = 34;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard6":
                curStageNum = 35;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard7":
                curStageNum = 36;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard8":
                curStageNum = 37;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard9":
                curStageNum = 38;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
            case "Hard10":
                curStageNum = 39;
                AnswerSheetBtn = hardAnswerButtons[curStageNum - 30];
                break;
                #endregion
        }
        // �������� ĵ���� ���� �� ���� ĵ���� ON
        selectStageUI.SetActive(false);
        doGameUI.SetActive(true);
        // ��Ŀ ���� �Լ�
        SetStage(curStageNum);
    }

    private void SetStage(int stageNum)
    {
        // �ش� �������� ������ �´� ��Ŀ���� ȭ�鿡 ���� ����� �Ѵ�.
        /*List<int> beakerSize;
        List<string> beakerString;

        switch (stageNum) // �� �������� �� ������ ���⿡ ���´�. �� �� ����ϰ� ������ ������ �� �ִ� ����� ������?
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
            RectTransform rectTransform = beakerInstance.GetComponent<RectTransform>();
            rectTransform.SetParent(canvas_Beaker.GetComponent<RectTransform>(), false);
            beakerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + i * 200, 0, 0); // -> ���� ��ġ�� Global���� ���ϰ� �� ��
            beakerInstance.GetComponent<Button>().onClick.AddListener(() => BeakerSelected(beakerInstance.GetComponent<Button>()));

            Stack<char> charBeakerStack = new Stack<char>(beakerSetting.beakerStack[i]);
            int count = 0;
            while(charBeakerStack.Count > 0)
            {
                char RGB = charBeakerStack.Pop();
                switch(RGB)
                {
                    case 'R':
                        canvas_Beaker.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = Color.red;
                        break;
                    case 'G':
                        canvas_Beaker.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = Color.green;
                        break;
                    case 'B':
                        canvas_Beaker.transform.GetChild(i).Find("Image" + (count + 1).ToString()).GetComponent<Image>().color = Color.blue;
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
        RectTransform answerRectTransform = answerInstance.GetComponent<RectTransform>();
        answerRectTransform.SetParent(canvas_Beaker.GetComponent<RectTransform>(), false);
        answerInstance.GetComponent<RectTransform>().localPosition = new Vector3(-750 + (beakerSetting.beakerSize.Count) * 200, 0, 0); // -> ���� ��ġ�� Global���� ���ϰ� �� ��
        Stack<char> answerCharSampleStack = new Stack<char>(beakerSetting.beakerAnswer);
        int c = 0;
        while (answerCharSampleStack.Count > 0)
        {
            c++;
            char RGB = answerCharSampleStack.Pop();
            switch (RGB)
            {
                case 'R':
                    canvas_Beaker.transform.GetChild(beakerSetting.beakerSize.Count).Find("Image" + (c).ToString()).GetComponent<Image>().color = Color.red;
                    break;
                case 'G':
                    canvas_Beaker.transform.GetChild(beakerSetting.beakerSize.Count).Find("Image" + (c).ToString()).GetComponent<Image>().color = Color.green;
                    break;
                case 'B':
                    canvas_Beaker.transform.GetChild(beakerSetting.beakerSize.Count).Find("Image" + (c).ToString()).GetComponent<Image>().color = Color.blue;
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
            // ���� ��ư�� indicator ǥ��
            button.transform.Find("Indicator").gameObject.SetActive(true);
            firstBeakerSelected = true;
        }
    }

    void MoveRGBToAnotherBeaker(int fromBeaker, int toBeaker)
    {
        if (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker]) // �ش� ��ȣ�� ��Ŀ�� ����ִ� ������ ���� ��
        {
            while (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker] // �� ������ �� ä���������� from �ʿ��� �Űܴ��� or
                && stageBeaker.curBeakerAmount[fromBeaker] > 0) // from�� ��Ŀ�� ���� ���� RGB ���� 0 ���� �� ������ ���� 
            {
                char RGB = stageBeaker.beakerStack[fromBeaker].Pop();
                stageBeaker.beakerStack[toBeaker].Push(RGB); // from ��Ŀ�� �� ���� Ȯ���ؼ� to ��Ŀ���� �������
                // UI ����Ǵ� �� (��Ŀ �� �߰��ǰ� ���ҵǴ� ��) �� ���⿡�� �׶����� �ٷιٷ� üũ���ִ°� ������
                switch (RGB)
                {
                    case 'R':
                        canvas_Beaker.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = Color.red;
                        break;
                    case 'G':
                        canvas_Beaker.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = Color.green;
                        break;
                    case 'B':
                        canvas_Beaker.transform.GetChild(toBeaker).Find("Image" + (stageBeaker.curBeakerAmount[toBeaker] + 1).ToString()).GetComponent<Image>().color = Color.blue;
                        break;
                }
                canvas_Beaker.transform.GetChild(fromBeaker).Find("Image" + (stageBeaker.curBeakerAmount[fromBeaker]).ToString()).GetComponent<Image>().color = Color.white;
                //
                stageBeaker.curBeakerAmount[fromBeaker]--; // from ��Ŀ�� ���� �Ѱ� ����
                stageBeaker.curBeakerAmount[toBeaker]++; // to ��Ŀ�� ���� �Ѱ� ����
            }

            // ���� ������ ī��Ʈ �߰�
            curMoveCount++;
            if (curMoveCount == 1) // �ش� ������������ ó�� ����� �÷��̾��� Move
                playersChoice[curStageNum].Clear(); // ���� �� ���
            //playersChoice[curStageNum].Add(new Tuple<int, int>(fromBeaker, toBeaker));

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
        }
        else 
        {
            // Ŭ���� �������� �� �ൿ? UI? ���⿡ �־ ������ ��

        }
    }

    public void ResetBtnClicked(Button button) // ���� ��ư�� ����
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy
        DestoryBeakerPrefabs();
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        // Ŭ���� ui�� ��ư�� �����ٸ� �������� Ŭ���� ������ ���� ����� ��ư ���� �� Ŭ���� �� �߰�
        if (button.transform.Find("Clear") != null)
        {
            if (curStageNum < 10)
                tutStageClearCount++;
            else if (curStageNum >= 10 || curStageNum < 20)
                easyStageClearCount++;
            else if (curStageNum >= 20 || curStageNum < 30)
                normalStageClearCount++;
            else
                hardStageClearCount++;

            AnswerSheetBtn.SetActive(true);
        }
        else // �÷��� ȭ�鿡�� ���� ��ư ������
        {
            playersRestart[curStageNum]++; // ����� Ƚ�� ����
        }
        // �������� �����
        StartCoroutine("ResetStage");
    }

    IEnumerator ResetStage()
    {
        yield return null;
        // �������� �����
        SetStage(curStageNum);
    }

    public void GoBackBtnClicked(Button button) // �������� â���� ���ư��� ��ư�� ����
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy
        DestoryBeakerPrefabs();
        // Stage ���� ĵ���� Active;
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        if(doGameUI.activeSelf)
            doGameUI.SetActive(false);
        // Ŭ���� ui�� ��ư�� �����ٸ� �������� Ŭ���� ������ ���� ����� ��ư ���� �� Ŭ���� �� �߰�
        if (button.transform.Find("Clear") != null)
        {
            if (curStageNum < 10)
                tutStageClearCount++;
            else if (curStageNum >= 10 || curStageNum < 20)
                easyStageClearCount++;
            else if (curStageNum >= 20 || curStageNum < 30)
                normalStageClearCount++;
            else
                hardStageClearCount++;

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
        answerPanel.Find("Stage Text").GetComponent<TextMeshProUGUI>().text = "������ �������� : " + playerAnswerStageNum.ToString();
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
        //playersChoice[curStageNum].Clear(); // ���� �� ���
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995;
        secondBeakerSelected = false;
        secondSelectedBeakerNum = 1995; // ù ���� �� ������������ Ȱ��Ǵ� �Ķ���͵� �ʱ�ȭ
    }

    public void QuitBtnClicked()
    {
        Application.Quit();
    }
}