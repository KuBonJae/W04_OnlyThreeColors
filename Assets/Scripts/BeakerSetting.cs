using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public class BeakerSetting
{
    public List<int> beakerSize; // 비커의 총 사이즈
    public List<int> curBeakerAmount; // 현재 비커에 차 있는 RGB 총 량
    // string = R G B 로 이루어진 한 단어
    public List<Stack<char>> beakerStack;

    public int playerAnswerBeakerNum; // 플레이어가 제출하는 비커 번호 -> 비커 스택에서 가장 마지막 번호가 될 듯
    public Stack<char> beakerAnswer; // 진짜 정답을 가지고 있는 정답 비커

    public BeakerSetting(List<int> L_size, List<string> L_string, string answer)
    {
        // 비커들 크기 세팅 및 비커들 제작, 마지막 제출할 비커도 리스트에 반드시 포함할 것
        beakerSize = L_size;
        playerAnswerBeakerNum = L_size.Count - 1; // 마지막 비커 = 제출용 비커
        for (int i = 0; i < L_size.Count; i++)
        {
            beakerStack.Add(new Stack<char>());
        }

        // 처음에 꽉 차있을 메인 비커
        char[] charArray;
        for (int j = 0; j < L_size.Count; j++)
        {
            if(j < L_string.Count) // 입력된 RGB string의 총 갯수보다 j가 작다 => j 번째 비커는 RGB로 가득 찬다
            {
                charArray = L_string[j].ToCharArray();
                curBeakerAmount.Add(charArray.Length);
                for (int i = 0; i < charArray.Length; i++)
                {
                    beakerStack[j].Push(charArray[i]); // j 번째 비커에 순서대로 char 를 쌓음
                }
            }
            else // j가 더 크다 -> j번째 비커 부터는 RGB로 차 있지 않고 비어있다.
            {
                curBeakerAmount.Add(0); // 현재 비커 크기는 0으로 세팅
            }
        }
        
        charArray = answer.ToCharArray();
        for (int i = 0; i < charArray.Length; i++) // 정답 비커 제작 -> 크기 무한대
        {
            beakerAnswer.Push(charArray[i]);
        }
    }
}
