using UnityEngine;

public class DropZone : MonoBehaviour
{
    [SerializeField] private Color zoneColor;

    public Color ZoneColor => zoneColor;

    private void Start()
    {
        GetComponent<Renderer>().material.color = zoneColor;
    }
}