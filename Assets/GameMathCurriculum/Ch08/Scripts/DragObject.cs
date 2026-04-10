using UnityEngine;

public class DragObject : MonoBehaviour
{
    public bool isReturning;
    public Vector3 orinalPosition;
    public Vector3 startPosition;
    public float timeReturn = 2f;
    private Terrain terrain;
    private float timer;

    private void Start()
    {
        terrain = Terrain.activeTerrain;
    }

    public void DragStart()
    {
        isReturning = false;
        orinalPosition = transform.position;
        startPosition = Vector3.zero;
        timer = 0f;
    }

  
    public void Return()
    {
        Debug.Log("Return");
        timer = 0f;
        isReturning = true;
        startPosition = transform.position;
    }

    public void DragEnd()
    {
        Debug.Log("DragEnd");
        isReturning = false;
        timer = 0f;
        orinalPosition = Vector3.zero;
        startPosition = Vector3.zero;
    }

    private void Update()
    {
        if (isReturning)
        {
            timer += Time.deltaTime / timeReturn;
            Vector3 newPos = Vector3.Lerp(startPosition, orinalPosition, timer);
            newPos.y = terrain.SampleHeight(newPos) + 0.5f;
            transform.position = newPos;

            if (timer >= 1f)
            {
                isReturning = false;
                transform.position = orinalPosition;
                timer = 0f;
            }
        }
    }
}