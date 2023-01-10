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
    // hiddenby - список других карт, не позвол€ющих перевернуть эту лицом вверх
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // layoutID определ€ет дл€ этой карты р€д в раскладке
    public int layoutID;
    //  ласс SlotDef хранит информацию из элемента <slot> в LayoutXML
    public SlotDef slotDef;

    override public void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
