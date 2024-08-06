using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    Vector2 beakerPosition = Vector2.zero; // �ٵ� �̰� Canvas �� ��ġ�� Vector2�� �³�?
    //
    // �������� �� �÷��̾��� �ֱ� Ǯ�̰� ����� List
    List<List<Tuple<int, int>>> playersChoice;
    //
    // �� �������� ���� << �������� ���� ���� ���� ���� ���� ��
    int totalStageNum = 12;
    //
    // �������� ��ư�� �̸� �־�α� << ���߿� �������� Ŭ����� ���� ��ư ��� ��
    public GameObject[] stageButtons;
    //
    // ���� �������� ��ȣ
    int curStageNum;
    //
    // ��� ��ư�� Ȱ��ȭ ���� -> �ű� ��Ŀ�� �Ѱ� ���� �ߴµ� �ش� ��Ŀ�� �����ϰ� ���� �ʰ� ������ �ٲ���� ��
    bool isCanceled = false;
    //

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
                //ù��° ��Ŀ ������ Ǯ������ ��� ��ư�� Disable�� ������ ��

                //

                playersChoice[curStageNum].Add(new Tuple<int,int>(firstSelectedBeakerNum, secondSelectedBeakerNum));

                MoveRGBToAnotherBeaker(firstSelectedBeakerNum, secondSelectedBeakerNum);
            }
        }
    }

    // �������� ��ư Ŭ�� �� �߻��ϴ� �Լ�
    public void StageBtnClicked(Button button)
    {
        // ��ư �̸���� �ش� �������� ����
        switch (button.gameObject.name)
        {
            case "Tutorial1": // ��Ŀ �̸����� �����ϱ� �ϴµ� �ٸ� ���� ��� ��õ�޽��ϴ�.
                curStageNum = 0;
                break;
            case "Stage1":
                curStageNum = 1;
                break;
        }
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

        stageBeaker = new BeakerSetting(stageDataSO.stageDatas[stageNum].beakerSize,
            stageDataSO.stageDatas[stageNum].beakerRGB, stageDataSO.stageDatas[stageNum].answerBeaker);

        SetBeakerUI(stageBeaker);
    }

    private void SetBeakerUI(BeakerSetting beakerSetting)
    {
        // ��Ŀ ����� ���� ������� �������� �ҷ��� ��ġ��Ŵ
        for(int i=0; i< beakerSetting.beakerSize.Count; i++)
        {
            // ù ��ġ�� ������ instantiate
            // GameObject uiInstance = Intantiate(BeakerPrefab11, beakerPosition); // 11 ũ���� ��Ŀ ������ ����
            // �ν��Ͻ�ȭ�� UI�� ��ġ ���� -> �� ������ �߰��Ǵ� ��Ŀ�� canvas�� ���� ������ �ڽ��� ��ġ���� ���� ������ ����� ������Ű�� ��
            // RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
            // rectTransform.SetParent(Canvas.main.GetComponent<RectTransform>(), false); // ĵ������ �θ�� ���� -> ĵ���� �̸� ���� ����
        }

    }

    public void BeakerSelected(Button button) // ����� ��Ŀ�� �̸��� 404 << �� �� ��
    {
        if(firstBeakerSelected)
        {
            secondSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            secondBeakerSelected = true;
        }
        else
        {
            firstSelectedBeakerNum = Convert.ToInt32(button.gameObject.name);
            firstBeakerSelected = true;
            // ��� ��ư�� ����� �� �ֵ��� Enable�� ������ ��

        }
    }

    void MoveRGBToAnotherBeaker(int fromBeaker, int toBeaker)
    {
        if (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker]) // �ش� ��ȣ�� ��Ŀ�� ����ִ� ������ ���� ��
        {
            while (stageBeaker.curBeakerAmount[toBeaker] < stageBeaker.beakerSize[toBeaker] // �� ������ �� ä���������� from �ʿ��� �Űܴ��� or
                || stageBeaker.curBeakerAmount[fromBeaker] > 0) // from�� ��Ŀ�� ���� ���� RGB ���� 0 ���� �� ������ ����
            {
                stageBeaker.beakerStack[toBeaker].Push(stageBeaker.beakerStack[fromBeaker].Peek()); // from ��Ŀ�� �� ���� Ȯ���ؼ� to ��Ŀ���� �������
                stageBeaker.curBeakerAmount[fromBeaker]--; // from ��Ŀ�� ���� �Ѱ� ����
                stageBeaker.curBeakerAmount[toBeaker]++; // to ��Ŀ�� ���� �Ѱ� ����

                // UI ����Ǵ� �� (��Ŀ �� �߰��ǰ� ���ҵǴ� ��) �� ���⿡�� �׶����� �ٷιٷ� üũ���ִ°� ������

            }
        }
        // �� ������ ������ �۵� ����
    }

    public void AnswerBtnClicked()
    {
        Stack<char> playerAnswerStack = stageBeaker.beakerStack[stageBeaker.playerAnswerBeakerNum];
        Stack<char> AnswerStack = stageBeaker.beakerAnswer;
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
        // ������ ���õǾ� �ִ� ��Ŀ �����յ� ������ �״ٰ� ���� Destory �� ��
        // Destroy( );

        // �������� �����
        SetStage(curStageNum);
    }

    public void GoBackBtnClicked() // �������� â���� ���ư��� ��ư�� ����
    {
        // ���� ������� �ִ� ��Ŀ �����յ� ���� Destroy

        // Stage ���� ĵ���� Active;
    }

    public void SelectCancelBtnClicked() // ���� ��� ��ư�� ����
    {
        firstBeakerSelected = false;
        firstSelectedBeakerNum = 1995; // 1995�� �ǹ̰� �ִ� ���� �ƴϰ� �׳� �����Ѵٴ� �ǹ̷� �־�׽��ϴ�. 0�̶�� �ϸ� 0��° ��ư�̶� ���εŹ�����...
        // ��� ��ư�� �ٽ� Disable�� ������ ��

    }

    // ���� ���
}