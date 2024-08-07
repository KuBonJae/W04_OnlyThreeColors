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
    bool submitAnswer = false;
    //
    // �������� ó�� �����Ǵ� ��ġ
    Vector3 beakerPosition = Vector3.zero;
    //
    // �������� �� �÷��̾��� �ֱ� Ǯ�̰� ����� List
    List<List<Tuple<int, int>>> playersChoice;
    //
    // �ִ� ������ �� �ִ� Ƚ��, ���� ������ Ƚ��
    const int maxMoveCount = 100;
    int curMoveCount = 0;
    //
    // �� �������� ���� << �������� ���� ���� ���� ���� ���� ��
    int totalStageNum = 12;
    //
    // �������� ��ư�� �̸� �־�α� << ���߿� �������� Ŭ����� ���� ��ư ��� ��
    public GameObject[] tutStageButtons;
    public GameObject[] easyStageButtons;
    public GameObject[] normalStageButtons;
    public GameObject[] hardStageButtons;
    //
    // Ŭ����� �������� �� -> ��ư ���� ���ؼ� ��ư ���� 50% �̻��� Ŭ���� �Ǹ� ���� �������� ��ư�� ����
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
    // ��� ��ư�� Ȱ��ȭ ���� -> �ű� ��Ŀ�� �Ѱ� ���� �ߴµ� �ش� ��Ŀ�� �����ϰ� ���� �ʰ� ������ �ٲ���� ��
    bool isCanceled = false;
    //
    // ĵ���� �̸� �޾Ƶα�
    public GameObject canvas_Beaker;
    public GameObject selectStageUI;
    public GameObject doGameUI;
    public GameObject gameClearUI;
    //
    // ���� ���� �Ǿ� �ִ� ��Ŀ �����յ� ����
    private List<GameObject> beakerPrefabsOnDisplay = new List<GameObject>();
    //

    private void Awake()
    {
        //SetStage(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        playersChoice = new List<List<Tuple<int, int>>>();
        for(int i=0;i< totalStageNum;i++) // �������� ������ŭ ���� List<Tuple> �� �����ؼ� �̸� �־��ش�.
        {
            playersChoice.Add(new List<Tuple<int, int>>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(firstBeakerSelected) // �ű� ��Ŀ�� ���� �Ǿ��°�?
        {
            isCanceled = true;
            // ù ��Ŀ�� ���õ� ���¸� ��� ��ư�� Enable�� �����Ѵ�.

            //
            if (secondBeakerSelected) // �Ű��� ��Ŀ�� ���� �Ǿ��°�?
            {
                // �Ѵ� ���� �� �ϴ� selected �ٷ� �ʱ�ȭ
                firstBeakerSelected = secondBeakerSelected = isCanceled = false;
                EventSystem.current.SetSelectedGameObject(null); // ��ư ���õ� �� ���� << �̰� ���� ���ϸ� ���� ��ư Ŭ���� �������� �ȵ�
                canvas_Beaker.transform.GetChild(firstSelectedBeakerNum).Find("Indicator").gameObject.SetActive(false);
                //ù��° ��Ŀ ������ Ǯ������ ��� ��ư�� Disable�� ������ ��

                //

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

        //
    }

    // �������� ��ư Ŭ�� �� �߻��ϴ� �Լ�
    public void StageBtnClicked(Button button)
    {
        // ��ư �̸���� �ش� �������� ����
        switch (button.gameObject.name)
        {
            case "Tutorial1": // �������� ��ư �̸����� �����ϱ� �ϴµ� �ٸ� ���� ��� ��õ�޽��ϴ�.
                curStageNum = 0;
                break;
            case "Stage1":
                curStageNum = 1;
                break;
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

            // �ӽ� Instantiate ���� -> �ش� ������� Ȱ���ϸ� �� ��
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
            secondSelectedBeakerNum = Convert.ToInt32(button.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>().text);
            if(secondSelectedBeakerNum == firstSelectedBeakerNum) // ���� ������ �׳� �ʱ�ȭ
            {
                firstSelectedBeakerNum = 1995;
                secondSelectedBeakerNum = 1995;
                button.transform.Find("Indicator").gameObject.SetActive(false);
                firstBeakerSelected = false;
                return;
            }
            secondBeakerSelected = true;
            Debug.Log("second Btn clicked");
        }
        else
        {
            //firstSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            firstSelectedBeakerNum = Convert.ToInt32(button.transform.Find("Name").transform.GetComponent<TextMeshProUGUI>().text);
            // ���� ��ư�� indicator ǥ��
            button.transform.Find("Indicator").gameObject.SetActive(true);
            firstBeakerSelected = true;
            Debug.Log("first Btn clicked");
            // ��� ��ư�� ����� �� �ֵ��� Enable�� ������ ��

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
            playersChoice[curStageNum].Add(new Tuple<int, int>(fromBeaker, toBeaker));
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
            // �Ʒ� �۾����� �������� Ŭ���� ĵ�������� ������ �� 
            //


            // ��Ŀ �����յ� ���� ����

            // ���� �������� ���
            // curStageNum << �� Ȱ���ؼ� ���� Ŭ������ stage ��ư�� ���� �ణ ��Ӱ� ����� �ְ�, curStageNum + 1 ��ȣ�� ��ư�� Enable() ��Ŵ
            
            // �������� ĵ������ ����

        }
        else 
        {
            // Ŭ���� �������� �� �ൿ? UI? ���⿡ �־ ������ ��

        }
    }

    public void ResetBtnClicked() // ���� ��ư�� ����
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy
        DestoryBeakerPrefabs();
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        // �������� �����
        //SetStage(curStageNum);
        StartCoroutine("ResetStage"); // �̰� �� 0.1�� ���� ���ָ� ���� ����������? << ��¥ �𸣰ڴ� ����;
    }

    IEnumerator ResetStage()
    {
        yield return null;
        // �������� �����
        SetStage(curStageNum);
    }

    public void GoBackBtnClicked() // �������� â���� ���ư��� ��ư�� ����
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy
        DestoryBeakerPrefabs();
        // Stage ���� ĵ���� Active;
        if (gameClearUI.activeSelf)
            gameClearUI.SetActive(false);
        if(doGameUI.activeSelf)
            doGameUI.SetActive(false);

        selectStageUI.SetActive(true);
    }

    public void SelectCancelBtnClicked() // ���� ��� ��ư�� ����
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
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995;
        secondBeakerSelected = false;
        secondSelectedBeakerNum = 1995; // ù ���� �� ������������ Ȱ��Ǵ� �Ķ���͵� �ʱ�ȭ
    }
}