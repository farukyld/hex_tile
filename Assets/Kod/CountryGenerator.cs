using UnityEngine;

public enum Country
{
    China,
    Nepal,
    India,
    Mongol,
    Turkiye,
    None,
}
public class CountryGenerator : MonoBehaviour
{
    public float noiseScale = 0.1f;
    public int countryCount = 5;
    public float noiseThreshold = 0.5f;  // Adjust as needed
    [Header("Feature Points")]
    public Vector3[] featurePoints; // Randomly generated or manually set
    public int featurePointCount = 10;  // Number of feature points you want
    public float areaSize = 100f;  // The size of the area in which the feature points will be generated

    private void OnEnable()
    {
        GenerateRandomFeaturePoints();
    }

    private void GenerateRandomFeaturePoints()
    {
        featurePoints = new Vector3[featurePointCount];

        for (int i = 0; i < featurePointCount; i++)
        {
            float randomX = Random.Range(-areaSize / 2, areaSize / 2);
            float randomZ = Random.Range(-areaSize / 2, areaSize / 2);

            featurePoints[i] = new Vector3(randomX, 0, randomZ);
        }
    }

    public Country DetermineCountry(Vector3 position)
    {
        Vector3 baseCountryFeature = GetClosestFeaturePoint(position);
        float noiseValue = Mathf.PerlinNoise(position.x * noiseScale, position.z * noiseScale);

        // If noise value crosses a threshold, consider adjusting the country assignment
        if (noiseValue > noiseThreshold)
        {
            Vector3 secondClosestFeaturePoint = GetSecondClosestFeaturePoint(position);
            baseCountryFeature = secondClosestFeaturePoint;
        }

        // Convert feature to country (for this example, I'm using the feature's index, but you might want to use a mapping)
        int regionIndex = System.Array.IndexOf(featurePoints, baseCountryFeature);
        return (Country)(regionIndex % countryCount); // Assuming 4 countries; adjust as needed
    }

    private Vector3 GetClosestFeaturePoint(Vector3 position)
    {
        Vector3 closestFeature = featurePoints[0];
        float minDistance = Vector3.Distance(position, closestFeature);

        foreach (Vector3 feature in featurePoints)
        {
            float distance = Vector3.Distance(position, feature);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestFeature = feature;
            }
        }

        return closestFeature;
    }

    private Vector3 GetSecondClosestFeaturePoint(Vector3 position)
    {
        Vector3 closestFeature = featurePoints[0];
        Vector3 secondClosestFeature = featurePoints[1];

        float minDistance = Vector3.Distance(position, closestFeature);
        float secondMinDistance = Vector3.Distance(position, secondClosestFeature);

        // Swap if the initial assignment was incorrect
        if (secondMinDistance < minDistance)
        {
            var temp = closestFeature;
            closestFeature = secondClosestFeature;
            secondClosestFeature = temp;

            var tempDist = minDistance;
            minDistance = secondMinDistance;
            secondMinDistance = tempDist;
        }

        for (int i = 2; i < featurePoints.Length; i++) // Starting from 2 because we've already considered 0 and 1
        {
            float distance = Vector3.Distance(position, featurePoints[i]);

            if (distance < minDistance)
            {
                secondClosestFeature = closestFeature;
                secondMinDistance = minDistance;

                closestFeature = featurePoints[i];
                minDistance = distance;
            }
            else if (distance < secondMinDistance)
            {
                secondClosestFeature = featurePoints[i];
                secondMinDistance = distance;
            }
        }

        return secondClosestFeature;
    }


    // Visualization using OnDrawGizmos
    private void OnDrawGizmos()
    {
        for (float x = -50; x <= 50; x++)
        {
            for (float z = -50; z <= 50; z++)
            {
                Vector3 pos = new Vector3(x, 0, z);
                Country country = DetermineCountry(pos);

                // Set gizmo color based on country
                switch (country)
                {
                    case Country.China:
                        Gizmos.color = Color.red/2f;
                        break;
                    case Country.Nepal:
                        Gizmos.color = Color.blue;
                        break;
                    case Country.India:
                        Gizmos.color = Color.green;
                        break;
                    case Country.Mongol:
                        Gizmos.color = Color.yellow;
                        break;
                    case Country.Turkiye:
                        Gizmos.color = Color.red;
                        break;
                    default:
                        Gizmos.color = Color.white;
                        break;
                }
                Gizmos.DrawSphere(pos, 0.5f);
            }
        }
    }
}
