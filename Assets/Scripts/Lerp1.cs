// UMD IMDM290 
// Instructor: Myungin Lee
    // [a <-----------> b]
    // Lerp : Linearly interpolates between two points. 
    // https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Vector3.Lerp.html

using UnityEngine;

public class Lerp1 : MonoBehaviour
{
    [Header("Lerp")]
    public GameObject parent;
    GameObject[] spheres;
    static int numSphere = 1000; 
    float time = 0f;
    Vector3[] startPosition, endPosition;


   [Header("Rotation")]
    // Public variables can be adjusted in the Unity Inspector
    public float rotationSpeedX = 0f;
    public float rotationSpeedY = 225f; // e.g., rotate around the Y-axis
    public float rotationSpeedZ = 0f;
    public float currRot = 0f;

    bool heartFormed = false;
    bool heartRotationDone = true;
    bool heartScaleDone = false;


    [Header("Glow Trail")]
    public Material trailMaterial;
    public float trailTime = 1.2f;


    // Start is called before the first frame update
    void Start()
    {

        // Assign proper types and sizes to the variables.
        spheres = new GameObject[numSphere];
        startPosition = new Vector3[numSphere]; 
        endPosition = new Vector3[numSphere]; 
        
        // Define target positions. Start = random, End = heart 
        for (int i = 0; i < numSphere; i++){
            // Random start positions
            float r = 15f;
            startPosition[i] = new Vector3(r * Random.Range(-1f, 1f), r * Random.Range(-1f, 1f), r * Random.Range(-1f, 1f));        

            r = 7.5f; // radius of the circle
            // Circular end position
            // endPosition[i] = new Vector3(r * Mathf.Sin(i * 2 * Mathf.PI / numSphere), r * Mathf.Cos(i * 2 * Mathf.PI / numSphere));
            // Heart end position

            float sint = Mathf.Sin(i * 2 * Mathf.PI / numSphere);
            float cost = Mathf.Cos(i * 2 * Mathf.PI / numSphere);
            endPosition[i] =
                new Vector3(r * (Mathf.Sqrt(2) * Mathf.Pow(sint, 3)), 5f + r * (-Mathf.Pow(cost, 3) - Mathf.Pow(cost, 2) + 2 * cost), 20f);
        }
        // Let there be spheres..
        for (int i = 0; i < numSphere; i++){
            // Draw primitive elements:
            // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GameObject.CreatePrimitive.html
            spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere); 


            spheres[i].transform.localScale = Vector3.one * 0.1f; // Scale down the spheres
            spheres[i].transform.parent = parent.transform;

            // Position
            spheres[i].transform.position = startPosition[i];

            // Color. Get the renderer of the spheres and assign colors.
            Renderer sphereRenderer = spheres[i].GetComponent<Renderer>();
            // HSV color space: https://en.wikipedia.org/wiki/HSL_and_HSV
            float hue = (float)i / numSphere; // Hue cycles through 0 to 1
            Color color = Color.HSVToRGB(hue, 1f, 1f); // Full saturation and brightness
            sphereRenderer.material.color = color;


                        // --- Add Trail Renderer ---
            TrailRenderer trail = spheres[i].AddComponent<TrailRenderer>();
            trail.time = trailTime;
            trail.startWidth = 0.08f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.02f;
            trail.alignment = LineAlignment.View;
            
            // Assign material if provided, otherwise create a default
            if (trailMaterial != null)
            {
                trail.material = trailMaterial;
            }
            else
            {
                trail.material = new Material(Shader.Find("Sprites/Default"));
            }

            // Optional: make trail match sphere color
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(.5f, 0f),
                    new GradientAlphaKey(0f, .0f)
                }
            );
            trail.colorGradient = g;

        }
    }

    // Update is called once per frame
    void Update()
    {
        // Measure Time            
        time += Time.deltaTime; // Time.deltaTime = The interval in seconds from the last frame to the current one

        // what to update over time?
        if (heartFormed ){
            Rotate();

            GradientAlphaKey[] a = new GradientAlphaKey[] {
                    new GradientAlphaKey(.2f, 0f),
                    new GradientAlphaKey(0f, .0f)
                };
            UpdateSpheresAndTrailColor(a,time);

        } else {
            FormHeart();

            GradientAlphaKey[] a = new GradientAlphaKey[] {
                    new GradientAlphaKey(.4f, 0f),
                    new GradientAlphaKey(0f, .0f)
                };
            UpdateSpheresAndTrailColor(a,time);
        }
    }

    void FormHeart()
    {
        // lerpFraction variable defines the point between startPosition and endPosition (0~1)
        // let it oscillate over time using sin function
        float lerpFraction = Mathf.Sin((time/1.5f) - 0.5f) * 0.5f + 0.5f;


        for (int i = 0; i < numSphere; i++){
            // Lerp : Linearly interpolates between two points.
            // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector3.Lerp.html
            // Vector3.Lerp(startPosition, endPosition, lerpFraction)
            
            // Lerp logic. Update position
            spheres[i].transform.position = Vector3.Lerp(startPosition[i], endPosition[i], lerpFraction);
            // For now, start positions and end positions are fixed. But what if you change it over time?
            // startPosition[i]; endPosition[i];
        }
        if (lerpFraction >= .999f) {
            heartFormed = true;
        } 
    }

    void Rotate()
    {
        // Alternatively, rotate around a specific world axis (e.g., Vector3.up)
        // transform.Rotate(Vector3.up, rotationSpeedY * Time.deltaTime);
        parent.transform.Rotate(rotationSpeedX * Time.deltaTime, rotationSpeedY * Time.deltaTime, rotationSpeedZ * Time.deltaTime);

        currRot += rotationSpeedY * Time.deltaTime;
    }

    void UpdateSpheresAndTrailColor(GradientAlphaKey[] a, float currentTime)
    {
        // Update sphere and trail colors in unison - same color, same time
        for (int i = 0; i < numSphere; i++)
        {
            // Calculate the color once
            float hue = (float)i / numSphere;
            Color color = Color.HSVToRGB(Mathf.Abs(hue * Mathf.Sin(currentTime)), Mathf.Cos(currentTime), 2f + Mathf.Cos(currentTime));
            
            // Update sphere with this color
            Renderer sphereRenderer = spheres[i].GetComponent<Renderer>();
            sphereRenderer.material.color = color;

            // Update trail with the exact same color
            TrailRenderer trail = spheres[i].GetComponent<TrailRenderer>();
            if (trail != null)
            {
               Gradient g = new Gradient();
                g.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(color, 0f),
                        new GradientColorKey(color, 1f)
                    },a);
                trail.colorGradient = g;
            }
        }
    }
}