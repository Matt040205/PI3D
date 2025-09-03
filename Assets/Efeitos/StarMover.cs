using UnityEngine;

public class StarMover : MonoBehaviour
{
    
    private Vector3 p0_startPoint; 
    private Vector3 p1_controlPoint; 
    private Vector3 p2_endPoint; 

    private float duration; 
    private float elapsedTime = 0f;

    public void SetPath(Vector3 start, Vector3 control, Vector3 end, float travelDuration)
    {
        this.p0_startPoint = start;
        this.p1_controlPoint = control;
        this.p2_endPoint = end;
        this.duration = travelDuration;

        transform.position = start;
    }

    void Update()
    {
        if (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); 

            Vector3 position = Mathf.Pow(1 - t, 2) * p0_startPoint +
                               2 * (1 - t) * t * p1_controlPoint +
                               Mathf.Pow(t, 2) * p2_endPoint;

            transform.position = position;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}