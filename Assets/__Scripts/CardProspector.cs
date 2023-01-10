using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    public eCardState state = eCardState.drawpile;
    // hiddenby - ������ ������ ����, �� ����������� ����������� ��� ����� �����
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // layoutID ���������� ��� ���� ����� ��� � ���������
    public int layoutID;
    // ����� SlotDef ������ ���������� �� �������� <slot> � LayoutXML
    public SlotDef slotDef;

    override public void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
