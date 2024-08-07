using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using SimpleJSON;
using Models;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TMP_InputField inputName;

    GameStageManager gameStageManager;

    void Awake()
    {
        gameStageManager = FindObjectOfType<GameStageManager>();
    }

    private void Start()
    {
        List<int> test = new List<int>();
        test.Add(0);
        test.Add(1);
        User user = new User("asd", test);
    }

    public void RegisterUserInFirebase()
    {
        List<int> scoresByStages = new List<int>();

        for (int i = 10; i < 35; i++)
        {
            //only save 10~14, 20~24, 30~34
            if(i % 10 > 4)
            {
                i += 5;
                continue;
            }
            scoresByStages.Add(gameStageManager.playersChoice[i].Count);
        }

        User user = new User(inputName.text, scoresByStages);
        RestClient.Put($"https://three-colors-and-beakers-default-rtdb.firebaseio.com/Users/.json", user.ToJson()).Then(_ =>
        {

        });
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