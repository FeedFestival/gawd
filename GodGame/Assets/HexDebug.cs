using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexDebug : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _idTMP;
    [SerializeField]
    private TMP_Text _coordTMP;
    [SerializeField]
    private TMP_Text _NEidTMP;
    [SerializeField]
    private TMP_Text _EidTMP;
    [SerializeField]
    private TMP_Text _SEidTMP;
    [SerializeField]
    private TMP_Text _SWidTMP;
    [SerializeField]
    private TMP_Text _WidTMP;
    [SerializeField]
    private TMP_Text _NWidTMP;

    internal void SetId(int id)
    {
        _idTMP.text = id.ToString();
    }

    internal string GetId()
    {
        return _idTMP.text;
    }

    internal void SetCoord(int y, int x)
    {
        _coordTMP.text = y + "_" + x;
    }

    internal void SetNeighbor(CoordDir coordDir, int id)
    {
        switch (coordDir)
        {
            case CoordDir.NE:
                _NEidTMP.text = id.ToString();
                break;
            case CoordDir.E:
                _EidTMP.text = id.ToString();
                break;
            case CoordDir.SE:
                _SEidTMP.text = id.ToString();
                break;
            case CoordDir.SW:
                _SWidTMP.text = id.ToString();
                break;
            case CoordDir.W:
                _WidTMP.text = id.ToString();
                break;
            case CoordDir.NW:
            default:
                _NWidTMP.text = id.ToString();
                break;
        }
    }
}
