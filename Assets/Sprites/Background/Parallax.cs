using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    private float lastCamX;
    private float totalCamTravel;
    public GameObject cam;
    public float parallaxEffect; // 1 is not moving
    public float autoScroll = 0f; // for clouds or things moving automatically

    void Start()
    {
        lastCamX = cam.transform.position.x;
        totalCamTravel = 0f;
        length = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float deltaX = cam.transform.position.x - lastCamX;
        lastCamX = cam.transform.position.x;

        float movement = deltaX * (1f - parallaxEffect) + autoScroll * Time.deltaTime;

        transform.position = new Vector3(
            transform.position.x + movement,
            transform.position.y,
            transform.position.z
        );

        totalCamTravel += movement;

        if (totalCamTravel > length)
        {
            transform.position -= new Vector3(length, 0, 0);
            totalCamTravel -= length;
        }
        else if (totalCamTravel < -length)
        {
            transform.position += new Vector3(length, 0, 0);
            totalCamTravel += length;
        }
    }
}