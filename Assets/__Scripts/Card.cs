using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string suit; // Масть карты (C,D,H или S)
    public int rank; // Достоинство карты (1-14)
    public Color color = Color.black; // Цвет значков
    public string colS = "Black"; // или "Red". Имя цвета

    // Этот список хранит все игровые объекты Decorator
    public List<GameObject> decoGOs = new List<GameObject>();
    // Этот список хранит все игровые объекты Pip
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; // Игровой объект рубашки карты
    public CardDefinition def; // Извлекается из DeckXML.xml

    // Список компонентов SpriteRenderer этого и вложенных в него игровых объектов
    public SpriteRenderer[] spriteRenderers;

    public bool faceUp
    {
        get
        {
            return !back.activeSelf;
        }
        set
        {
            back.SetActive(!value);
        }
    }

    virtual public void OnMouseUpAsButton()
    {
        print(name);
    }
    private void Start()
    {
        SetSortOrder(0); // Обеспечит правильную сортировку карт
    }

    // Если spriteRenderers не определён, эта функция определит его
    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    // Инициализирует поле sortingLayerName во всех компонентах SpriteRenderer
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach (var item in spriteRenderers)
        {
            item.sortingLayerName = tSLN;
        }
    }

    // Инициализирует поле sortingOrder всех компонентов SpriteRenderer
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        // Выполнить обход всех элементов в списке spriteRenderers
        foreach (var item in spriteRenderers)
        {
            if (item.gameObject == this.gameObject)
            {
                item.sortingOrder = sOrd;
                continue;
            }

            // Каждый дочерний игровой объект имеет имя
            // Установить порядковый номер для сортировки, в зависимости от имени
            switch (item.gameObject.name)
            {
                case "back": // если имя "back"
                    // Установить наибольший порядковый номер
                    //   для отображения поверх других спрайтов
                    item.sortingOrder = sOrd + 2;
                    break;
                case "face": // если имя "face"
                default:     // или же другое
                    // Установить промежуточный порядковый номер
                    //   для отображения поверх фона
                    item.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }
}

[System.Serializable] // Сериализуемый класс доступен для правки в инспекторе
public class Decorator
{
    // Этот класс хранит информацию из DeckXML о каждом значке на карте
    public string type; // Значок, определяющий достоинство карты, имеет
                        // type = "pip"
    public Vector3 loc; // Местоположение спрайта на карте
    public bool flip = false; // Признак переворота спрайта по вертикали
    public float scale = 1f; // Масштаб спрайта
}


[System.Serializable] 
public class CardDefinition
{
    // Этот класс хранит информацию о достоинстве карты
    public string face; // Спрайт, изображающий лицевую сторону карты
    public int rank; // Достоинство карты (1-13)
    public List<Decorator> pips = new List<Decorator>(); // Значки
}