using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset layoutXML;
    public TextAsset deckXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 5f; // �������� ����� �������� 5 �������
    public TextMeshProUGUI gameOverText, roundResultText, highScoreText;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;
    public FloatingScore fsRun;

    private void Awake()
    {
        S = this; // ���������� �������-�������� Prospector
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<TextMeshProUGUI>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<TextMeshProUGUI>().text = hScore;

        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<TextMeshProUGUI>();
        }

        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<TextMeshProUGUI>();
        }

        // ������ �������
        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }
    private void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;
        
        deck = GetComponent<Deck>();  // �������� ��������� Deck
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        //Card c;
        //for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        //{
        //    c = deck.cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum%13)*3, cNum/13*4, 0);
        //}

        layout = GetComponent<Layout>(); // �������� ��������� Layout
        layout.ReadLayout(layoutXML.text); // �������� ��� ���������� LayoutXML
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (var item in lCD)
        {
            tCP = item as CardProspector;
            lCP.Add(tCP);
        }
        return lCP;
    }
    
    // ������� Draw ������� ���� ����� � ������� drawPile � ���������� �
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    // ��������� ����� � ��������� ��������� - "�����"
    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        // ��������� �����
        foreach (var item in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = item.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * item.x,
                layout.multiplier.y * item.y,
                item.layerID);
            cp.layoutID = item.id;
            cp.slotDef = item;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(item.layerName); // ��������� ���� ����������
            tableau.Add(cp);
        }

        // ��������� ������ ����, �������� ����������� ������
        foreach (var item in tableau)
        {
            foreach (var item2 in item.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(item2);
                item.hiddenBy.Add(cp);
            }
        }
        // ������� ��������� ������� �����
        MoveToTarget(Draw());

        // ��������� ������ ��������� ����
        UpdateDrawPile();
    }

    // ���������� ������� ������� ����� � ������ ���������� ����
    void MoveToDiscard(CardProspector cd)
    {
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    // ������ ����� cd ����� ������� ������
    void MoveToTarget(CardProspector cd)
    {
        if (target != null)
        {
            MoveToDiscard(target);
        }
        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    // ������������ ������ ��������� ����, ����� ���� �����, ������� ���� ��������
    void UpdateDrawPile()
    {
        CardProspector cd;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i*dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f*i);
            cd.faceUp = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    // CardClicked ���������� � ����� �� ������ �� ����� �����
    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case eCardState.drawpile:
                // ������ �� ����� ����� � ������ ��������� ���� ��������
                //   � ����� ������� �����
                MoveToDiscard(target); // ����������� ������� ����� � discardPile
                MoveToTarget(Draw()); // ����������� ������� ��������� ����� 
                                      //   �� ����� �������
                UpdateDrawPile();     // �������� ��������� ������ ��������� ����
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;
            case eCardState.tableau:
                // ��� ����� � �������� ��������� ����������� �����������
                //    � ����������� �� ����� �������
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    // �����, ���������� ������� �������� ����, �� ����� ������������
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    // ���� ������� ����������� �� �����������, ����� �� ����� ������������
                    validMatch = false;
                }
                if (!validMatch) return; // �����, ���� ����� �� ����� ������������

                // �� ��������� �����: ����� ����� �����������.
                tableau.Remove(cd); // ������� �� ������ tableau
                MoveToTarget(cd);  // ������� ��� ����� �������
                SetTableauFaces(); // ��������� ����� � �������� ���������
                                   // ������� �������� ���� ��� �����
                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);
                break;
            case eCardState.target:
                // ������ �� ������� ����� ������������
                break;
        }
        // ��������� ���������� ����
        CheckForGameOver();
    }

    // ��������� ���������� ����
    void CheckForGameOver()
    {
        // ���� �������� ��������� ��������, ���� ���������
        if (tableau.Count == 0)
        {
            // ������� GameOver � ��������� ������
            GameOver(true);
            return;
        }
        // ���� ���� ��� ��������� �����, ���� �� �����������
        if (drawPile.Count > 0)
        {
            return;
        }

        // ��������� ������� ���������� �����
        foreach (var item in tableau)
        {
            if(AdjacentRank(item, target))
            {
                // ���� ���� ���������� ���, ���� �� �����������
                return;
            }
        }
        // ��� ��� ���������� ����� ���, ���� �����������
        // ������� GameOver � ��������� ������
        GameOver(false);
    }

    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null) 
        {
            score += fsRun.score;
        }
        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            // print("Game Over. You Won! :)");
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            // print("Game Over. You Lost! :(");
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }
        
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        // ������������� ����� � �������� ���� � �������� ���������
        SceneManager.LoadScene("__Prospector_Scene_0");
    }
    // ���������� true, ���� ��� ����� ������������� ������� �����������
    //   (� ������ ������������ �������� ����������� ����� ����� � �������)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // ���� ����� �� ���� �������� ������� �������� ����,
        //   ������� ����������� �� �����������.
        if (!c0.faceUp || !c1.faceUp) return false;
        // ���� ����������� ���� ���������� �� 1, ������� ����������� �����������
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return true;
        }
        if (c0.rank == 1 && c1.rank == 13) return true;
        if (c0.rank == 13 && c1.rank == 1) return true;

        // ����� ������� false
        return false;
    }
    // ����������� ����� ����� layoutID � ��������� CardProspector � ���� �������
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (var item in tableau)
        {
            // ����� �� ���� ������ � tableau
            if (item.layoutID == layoutID)
            {
                // ���� ����� ����� ����� ��������� � �������, ������� �
                return item;
            }
        }
        // ���� ������ �� ������� ������� null
        return null;
    }

    // ������������ ����� � �������� ��������� ������� �������� ����� ��� ����
    void SetTableauFaces()
    {
        foreach (var item in tableau)
        {
            bool faceUp = true; // ������������, ��� ����� ������ ���� �������� �������
            // �������� �����
            foreach (var item2 in item.hiddenBy)
            {
                // ���� ����� �� ����, ������������� �������, ������������ � �������� ���������
                if (item2.state == eCardState.tableau)
                {
                    faceUp = false; // ��������� ������� �������� ����
                }
            }
            item.faceUp = faceUp; // ��������� ����� ��� ��� �����
        }
    }

    // ������������ �������� FloatingScore
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // � ������ ������, ��������� � ���������� ����
            //    ����������� ���� � �� �� ��������
            case eScoreEvent.draw:      // ����� ��������� �����
            case eScoreEvent.gameWin:   // ������ � ������
            case eScoreEvent.gameLoss:  // �������� � ������
                // �������� fsRun � Scoreboard
                if (fsRun != null)
                {
                    // ������� ����� ��� ������ �����
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    // ����� ��������������� fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; // �������� fsRun, ����� ������� ������
                }
                break;
            case eScoreEvent.mine: // �������� ����� �� �������� ���������
                // ������� FloatingScore ��� ����������� ����� ���������� �����
                FloatingScore fs;
                // ����������� �� ������� ��������� ���� mousePosition � fsPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
            case eScoreEvent.mineGold:
                break;            
            default:
                break;
        }
    }
}
