using System.Collections;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] Transform weatherObject;
    [SerializeField] GameObject rain;
    [SerializeField] bool rainy;
    [SerializeField] float rainDelaySec = 0.1f;
    [SerializeField] float rainInitialYPos = 10f;
    [SerializeField] float rainXRange = 10f;

    void Start()
    {
        StartCoroutine(goRain());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator goRain()
    {
        yield return new WaitForSeconds(rainDelaySec);
        if (rainy)
        {
            Vector3 loc = new Vector3(Random.Range(-rainXRange, rainXRange), rainInitialYPos, 0);
            Instantiate(rain, loc, Quaternion.identity, weatherObject);
        }
        StartCoroutine(goRain());
    }
}
