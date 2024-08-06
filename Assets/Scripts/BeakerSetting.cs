using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public class BeakerSetting
{
    public List<int> beakerSize; // ��Ŀ�� �� ������
    public List<int> curBeakerAmount; // ���� ��Ŀ�� �� �ִ� RGB �� ��
    // string = R G B �� �̷���� �� �ܾ�
    public List<Stack<char>> beakerStack;

    public int playerAnswerBeakerNum; // �÷��̾ �����ϴ� ��Ŀ ��ȣ -> ��Ŀ ���ÿ��� ���� ������ ��ȣ�� �� ��
    public Stack<char> beakerAnswer; // ��¥ ������ ������ �ִ� ���� ��Ŀ

    public BeakerSetting(List<int> L_size, List<string> L_string, string answer)
    {
        // ��Ŀ�� ũ�� ���� �� ��Ŀ�� ����, ������ ������ ��Ŀ�� ����Ʈ�� �ݵ�� ������ ��
        beakerSize = L_size;
        playerAnswerBeakerNum = L_size.Count - 1; // ������ ��Ŀ = ����� ��Ŀ
        for (int i = 0; i < L_size.Count; i++)
        {
            beakerStack.Add(new Stack<char>());
        }

        // ó���� �� ������ ���� ��Ŀ
        char[] charArray;
        for (int j = 0; j < L_size.Count; j++)
        {
            if(j < L_string.Count) // �Էµ� RGB string�� �� �������� j�� �۴� => j ��° ��Ŀ�� RGB�� ���� ����
            {
                charArray = L_string[j].ToCharArray();
                curBeakerAmount.Add(charArray.Length);
                for (int i = 0; i < charArray.Length; i++)
                {
                    beakerStack[j].Push(charArray[i]); // j ��° ��Ŀ�� ������� char �� ����
                }
            }
            else // j�� �� ũ�� -> j��° ��Ŀ ���ʹ� RGB�� �� ���� �ʰ� ����ִ�.
            {
                curBeakerAmount.Add(0); // ���� ��Ŀ ũ��� 0���� ����
            }
        }
        
        charArray = answer.ToCharArray();
        for (int i = 0; i < charArray.Length; i++) // ���� ��Ŀ ���� -> ũ�� ���Ѵ�
        {
            beakerAnswer.Push(charArray[i]);
        }
    }
}
