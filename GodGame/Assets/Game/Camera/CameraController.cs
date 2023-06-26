using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Vector3 _offset;

    public void FocusOnUnit(Vector3 unitPos)
    {
        transform.DOMove(unitPos + _offset, 0.3f);
    }
}
