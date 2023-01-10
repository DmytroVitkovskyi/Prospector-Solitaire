using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string suit; // ����� ����� (C,D,H ��� S)
    public int rank; // ����������� ����� (1-14)
    public Color color = Color.black; // ���� �������
    public string colS = "Black"; // ��� "Red". ��� �����

    // ���� ������ ������ ��� ������� ������� Decorator
    public List<GameObject> decoGOs = new List<GameObject>();
    // ���� ������ ������ ��� ������� ������� Pip
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; // ������� ������ ������� �����
    public CardDefinition def; // ����������� �� DeckXML.xml

    // ������ ����������� SpriteRenderer ����� � ��������� � ���� ������� ��������
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
        SetSortOrder(0); // ��������� ���������� ���������� ����
    }

    // ���� spriteRenderers �� ��������, ��� ������� ��������� ���
    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    // �������������� ���� sortingLayerName �� ���� ����������� SpriteRenderer
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach (var item in spriteRenderers)
        {
            item.sortingLayerName = tSLN;
        }
    }

    // �������������� ���� sortingOrder ���� ����������� SpriteRenderer
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        // ��������� ����� ���� ��������� � ������ spriteRenderers
        foreach (var item in spriteRenderers)
        {
            if (item.gameObject == this.gameObject)
            {
                item.sortingOrder = sOrd;
                continue;
            }

            // ������ �������� ������� ������ ����� ���
            // ���������� ���������� ����� ��� ����������, � ����������� �� �����
            switch (item.gameObject.name)
            {
                case "back": // ���� ��� "back"
                    // ���������� ���������� ���������� �����
                    //   ��� ����������� ������ ������ ��������
                    item.sortingOrder = sOrd + 2;
                    break;
                case "face": // ���� ��� "face"
                default:     // ��� �� ������
                    // ���������� ������������� ���������� �����
                    //   ��� ����������� ������ ����
                    item.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }
}

[System.Serializable] // ������������� ����� �������� ��� ������ � ����������
public class Decorator
{
    // ���� ����� ������ ���������� �� DeckXML � ������ ������ �� �����
    public string type; // ������, ������������ ����������� �����, �����
                        // type = "pip"
    public Vector3 loc; // �������������� ������� �� �����
    public bool flip = false; // ������� ���������� ������� �� ���������
    public float scale = 1f; // ������� �������
}


[System.Serializable] 
public class CardDefinition
{
    // ���� ����� ������ ���������� � ����������� �����
    public string face; // ������, ������������ ������� ������� �����
    public int rank; // ����������� ����� (1-13)
    public List<Decorator> pips = new List<Decorator>(); // ������
}