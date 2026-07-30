using UnityEngine;

public class JerryCan : MonoBehaviour
{
    public float maxFuel = 100f;
    public float currentFuel;
    public float fuelDrain = 25f;

    private void Start()
    {
        currentFuel = maxFuel;
    }

    public void PourFuel()
    {
        currentFuel -= fuelDrain;
    }
}
