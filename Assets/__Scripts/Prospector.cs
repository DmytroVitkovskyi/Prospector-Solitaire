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
    public float reloadDelay = 5f; // Задержка между раундами 5 секунды
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
        S = this; // Подготовка объекта-одиночки Prospector
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

        // Скрыть надписи
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
        
        deck = GetComponent<Deck>();  // Получить компонент Deck
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        //Card c;
        //for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        //{
        //    c = deck.cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum%13)*3, cNum/13*4, 0);
        //}

        layout = GetComponent<Layout>(); // Получить компонент Layout
        layout.ReadLayout(layoutXML.text); // Передать ему содержимое LayoutXML
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
    
    // Функция Draw снимает одну карту с вершины drawPile и возвращает её
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    // Размещает карты в начальной раскладке - "шахте"
    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        // Разложить карты
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
            cp.SetSortingLayerName(item.layerName); // Назначить слой сортировки
            tableau.Add(cp);
        }

        // Настроить списки карт, мешающих перевернуть данную
        foreach (var item in tableau)
        {
            foreach (var item2 in item.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(item2);
                item.hiddenBy.Add(cp);
            }
        }
        // Выбрать начальную целевую карту
        MoveToTarget(Draw());

        // Разложить стопку свободных карт
        UpdateDrawPile();
    }

    // Перемещает текущую целевую карту в стопку сброшенных карт
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

    // Делает карту cd новой целевой картой
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

    // Раскладывает стопку свободных карт, чтобы было видно, сколько карт осталось
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

    // CardClicked вызывается в ответ на щелчок на любой карте
    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case eCardState.drawpile:
                // Щелчок на любой карте в стопке свободных карт приводит
                //   к смене целевой карты
                MoveToDiscard(target); // Переместить целевую карту в discardPile
                MoveToTarget(Draw()); // Переместить верхнюю свободную карту 
                                      //   на место целевой
                UpdateDrawPile();     // Повторно разложить стопку свободных карт
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;
            case eCardState.tableau:
                // Для карты в основной раскладке проверяется возможность
                //    её перемещения на место целевой
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    // Карта, повернутая лицевой стороной вниз, не может перемещаться
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    // Если правило старшинства не соблюдается, карта не может перемещаться
                    validMatch = false;
                }
                if (!validMatch) return; // Выйти, если карта не может перемещаться

                // Мы оказались здесь: карту можно переместить.
                tableau.Remove(cd); // Удалить из списка tableau
                MoveToTarget(cd);  // Сделать эту карту целевой
                SetTableauFaces(); // Повернуть карты в основной раскладке
                                   // лицевой стороной вниз или вверх
                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);
                break;
            case eCardState.target:
                // Щелчок на целевой карте игнорируется
                break;
        }
        // Проверить завершение игры
        CheckForGameOver();
    }

    // Проверяет завершение игры
    void CheckForGameOver()
    {
        // Если основная раскладка опустела, игра завершена
        if (tableau.Count == 0)
        {
            // Вызвать GameOver с признаком победы
            GameOver(true);
            return;
        }
        // Если есть ещё свободные карты, игра не завершилась
        if (drawPile.Count > 0)
        {
            return;
        }

        // Проверить наличие допустимых ходов
        foreach (var item in tableau)
        {
            if(AdjacentRank(item, target))
            {
                // Если есть допустимый ход, игра не завершилась
                return;
            }
        }
        // Так как допустимых ходов нет, игра завершилась
        // Вызвать GameOver с признаком победы
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
        // Перезагрузить сцену и сбросить игру в исходное состояние
        SceneManager.LoadScene("__Prospector_Scene_0");
    }
    // Возвращает true, если две карты соответствуют правилу старшинства
    //   (с учётом циклического переноса старшинства между тузом и королем)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // Если любая из карт повёрнута лицевой стороной вниз,
        //   правило старшинства не соблюдается.
        if (!c0.faceUp || !c1.faceUp) return false;
        // Если достоинства карт отличаются на 1, правило старшинства соблюдается
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return true;
        }
        if (c0.rank == 1 && c1.rank == 13) return true;
        if (c0.rank == 13 && c1.rank == 1) return true;

        // Иначе вернуть false
        return false;
    }
    // Преобразует номер слота layoutID в экземпляр CardProspector с этим номером
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (var item in tableau)
        {
            // Поиск по всем картам в tableau
            if (item.layoutID == layoutID)
            {
                // Если номер слота карты совпадает с искомым, вернуть её
                return item;
            }
        }
        // Если ничего не найдено вернуть null
        return null;
    }

    // Поворачивает карты в основной раскладке лицевой стороной вверх или вниз
    void SetTableauFaces()
    {
        foreach (var item in tableau)
        {
            bool faceUp = true; // Предположить, что карта должна быть повёрнута лицевой
            // стороной вверх
            foreach (var item2 in item.hiddenBy)
            {
                // Если любая из карт, перекрывающих текущую, присутствует в основной раскладке
                if (item2.state == eCardState.tableau)
                {
                    faceUp = false; // Повернуть лицевой стороной вниз
                }
            }
            item.faceUp = faceUp; // Повернуть карту так или иначе
        }
    }

    // Обрабатывает движение FloatingScore
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // В случае победы, проигрыша и завершения хода
            //    выполняются одни и те же действия
            case eScoreEvent.draw:      // Выбор свободной карты
            case eScoreEvent.gameWin:   // Победа в раунде
            case eScoreEvent.gameLoss:  // Проигрыш в раунде
                // Добавить fsRun в Scoreboard
                if (fsRun != null)
                {
                    // Создать точки для кривой Безье
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    // Также скорректировать fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; // Очистить fsRun, чтобы создать заново
                }
                break;
            case eScoreEvent.mine: // Удаление карты из основной раскладки
                // Создать FloatingScore для отображения этого количества очков
                FloatingScore fs;
                // Переместить из позиции указателя мыши mousePosition в fsPosRun
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
