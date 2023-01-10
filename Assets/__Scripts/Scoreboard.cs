using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;

    [Header("Set in Inspector")]
    public GameObject prefabFloatingScore;

    [Header("Set Dynamically")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTrans;

    public int score
    {
        get { return _score; }
        set { 
            _score = value;
            scoreString = _score.ToString("N0");
        }
    }
    public string scoreString
    {
        get { return _scoreString; }
        set
        {
            _scoreString = value;
            GetComponent<TextMeshProUGUI>().text = _scoreString;
        }
    }

    private void Awake()
    {
        if (S == null)
        {
            S = this;
        }
        else
        {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }
        canvasTrans = transform.parent;
    }
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject; // Настроить обратный вызов
        fs.Init(pts);
        return fs;
    }
    public void FSCallBack(FloatingScore fs)
    {
        // Когда SendMessage вызовет эту функцию, 
        //  она должна добавить очки из вызвавшего экземпляра FloatingScore
        score += fs.score;
    }

}
