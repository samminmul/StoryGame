using UnityEngine;

public class Rain : MonoBehaviour
{
    Transform tr;
    [SerializeField] float bottom;
    [SerializeField] float rainSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (tr.position.y > bottom)
        {
            tr.position -= new Vector3(0, rainSpeed, 0);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
