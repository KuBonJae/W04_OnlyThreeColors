using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using SimpleJSON;
using Models;
using TMPro;
using System;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TMP_InputField inputName;
    [SerializeField] GameObject answerTexts;
    [SerializeField] GameObject warningText;

    GameStageManager gameStageManager;
    TextMeshProUGUI[] texts;

    void Awake()
    {
        gameStageManager = FindObjectOfType<GameStageManager>();
    }

    private void Start()
    {
        texts = answerTexts.GetComponentsInChildren<TextMeshProUGUI>();
        ResetLeaderboard();
    }

    IEnumerator PopupWarningText()
    {
        warningText.SetActive(true);
        yield return new WaitForSeconds(1f);
        warningText.SetActive(false);
    }

    public void RegisterUserInFirebase()
    {
        if (inputName.text.Length > 10)
        {
            StartCoroutine(PopupWarningText());
            return;
        }

        List<int> scoresByStages = new List<int>();

        for (int i = 10; i < 35; i++)
        {
            //only save 10~14, 20~24, 30~34
            if(i % 10 > 5)
            {
                i = ((i/10) + 1)* 10;
                continue;
            }
            scoresByStages.Add(gameStageManager.playersChoice[i].Count);
        }

        User user = new User(inputName.text, scoresByStages);

        RestClient.Get("https://three-colors-and-beakers-default-rtdb.firebaseio.com/Users/.json").Then(response =>
        {
            var json = JSON.Parse(response.Text);
            print(json);

            for (int i = 0; i < json.Count; i++)
            {
                string userName = json[i]["name"].Value;
                var scoreOfStage = json[i]["scoresByStages"];
                int countInDB = 0, countInLocal = 0;

                for (int j = 0; j < scoreOfStage.Count; j++)
                {
                    if (int.Parse(scoreOfStage[j].Value) != 0)
                    {
                        countInDB++;
                    }

                    if ((scoresByStages[j]) != 0)
                    {
                        countInLocal++;
                    }
                }
                
                // if the name is already in local and clear counts in db is bigger than in local, do not store.
                if (userName == inputName.text && countInDB > countInLocal)
                {
                    return;
                }
            }

            RestClient.Put($"https://three-colors-and-beakers-default-rtdb.firebaseio.com/Users/{user.name}.json", user.ToJson()).Then(_ =>
            {

            });

        });

    }

    public void UpdateLeaderBoardByStage(int stageNum)
    {
        ResetLeaderboard();

        List<Tuple<string, int>> playerScoreListByStage = new();
        RestClient.Get("https://three-colors-and-beakers-default-rtdb.firebaseio.com/Users/.json").Then(response =>
        {
            var json = JSON.Parse(response.Text);
            print(json);

            for (int i = 0; i < json.Count; i++)
            {
                string userName = json[i]["name"].Value;
                int scoreOfStage = int.Parse(json[i]["scoresByStages"][stageNum].Value);
                if (scoreOfStage != 0)
                {
                    playerScoreListByStage.Add(new Tuple<string, int>(userName, scoreOfStage));
                }
            }

            Debug.Log("size of players who clear stage : " + playerScoreListByStage.Count);

            // 이 아래부터는 원래 해당 람다식 밖에 적으려고 했으나, 서버에서 불러오는 시간이 있다보니 오히려 아래 코드를 밖에 빼면 해당 람다식보다 먼저 호출되는 경우가 발생
            // 그러므로 서버에서 불러와야만 이루어지는 작업은 모두 해당 람다식에서 이루어져야 한다.

            playerScoreListByStage.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            // only show 50 players
            if (playerScoreListByStage.Count > 50)
            {
                playerScoreListByStage.RemoveRange(50, playerScoreListByStage.Count - 50);
            }
            for (int i = 0; i < playerScoreListByStage.Count; i++)
            {
                texts[i].SetText($"{i + 1}. {playerScoreListByStage[i].Item1} : {playerScoreListByStage[i].Item2}");
            }
        });
    }

        public void UpdateLeaderBoardShowingClearCounts()
        {
            ResetLeaderboard();

            List<Tuple<string, int>> playerScoreListByStage = new();
            RestClient.Get("https://three-colors-and-beakers-default-rtdb.firebaseio.com/Users/.json").Then(response =>
            {
                var json = JSON.Parse(response.Text);
                print(json);

                for (int i = 0; i < json.Count; i++)
                {
                    string userName = json[i]["name"].Value;
                    var scoreOfStage = json[i]["scoresByStages"];
                    int count = 0;

                    for (int j=0; j< scoreOfStage.Count; j++)
                    {
                        if (int.Parse(scoreOfStage[j].Value) != 0)
                        {
                            count++;
                        }
                    }

                    if(count !=0)
                    {
                        playerScoreListByStage.Add(new Tuple<string, int>(userName, count) );
                    }
                }

                playerScoreListByStage.Sort((x, y) => y.Item2.CompareTo(x.Item2));

                // only show 50 players
                if (playerScoreListByStage.Count > 50)
                {
                    playerScoreListByStage.RemoveRange(50, playerScoreListByStage.Count - 50);
                }
                for (int i = 0; i < playerScoreListByStage.Count; i++)
                {
                    texts[i].SetText($"{i+1}. {playerScoreListByStage[i].Item1} : {playerScoreListByStage[i].Item2}");
                }
            });
        }

    void ResetLeaderboard()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].SetText("");
        }
    }
}


[System.Serializable]
public class User
{
    public string name;
    public List<int> scoresByStages;

    public User(string _name, List<int> scoreBystage)
    {
        this.name = _name;
        this.scoresByStages = scoreBystage;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}