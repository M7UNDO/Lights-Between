using UnityEngine;

public class ElectricTorchOnOff : MonoBehaviour
{
    EmissionMaterialGlassTorchFadeOut _emissionMaterialFade;
    BatteryPowerPickup _batteryPower;

    public enum LightChoose
    {
        noBattery,
        withBattery
    }

    public LightChoose modoLightChoose;

    [Space]
    public bool _PowerPickUp = false;

    [Space]
    public float intensityLight = 2.5f;

    private bool _flashLightOn = false;

    [SerializeField]
    float _lightTime = 0.05f;

    public PlayerInputHandler _input;

    private bool _lastFlashInput;

    private Light _light;

    private void Awake()
    {
        _batteryPower = FindFirstObjectByType<BatteryPowerPickup>();
        _input = FindFirstObjectByType<PlayerInputHandler>();
        _light = GetComponent<Light>();
    }

    void Start()
    {
        GameObject _scriptControllerEmissionFade = GameObject.Find("default");

        if (_scriptControllerEmissionFade != null)
        {
            _emissionMaterialFade = _scriptControllerEmissionFade.GetComponent<EmissionMaterialGlassTorchFadeOut>();
        }

        if (_scriptControllerEmissionFade == null)
        {
            Debug.Log("Cannot find 'EmissionMaterialGlassTorchFadeOut' script");
        }
    }

    void Update()
    {
        switch (modoLightChoose)
        {
            case LightChoose.noBattery:
                NoBatteryLight();
                break;

            case LightChoose.withBattery:
                WithBatteryLight();
                break;
        }
    }

    void InputKey()
    {
        if (_input.toggleFlashlight && !_lastFlashInput)
        {
            _flashLightOn = !_flashLightOn;
        }

        _lastFlashInput = _input.toggleFlashlight;
    }

    void NoBatteryLight()
    {
        InputKey();

        if (_flashLightOn)
        {
            _light.intensity = intensityLight;
            _emissionMaterialFade.OnEmission();
        }
        else
        {
            _light.intensity = 0f;
            _emissionMaterialFade.OffEmission();
        }
    }

    void WithBatteryLight()
    {
        InputKey();

        if (_flashLightOn)
        {
            _light.intensity = intensityLight;

            intensityLight -= Time.deltaTime * _lightTime;

            _emissionMaterialFade.TimeEmission(_lightTime);

            if (intensityLight < 0)
            {
                intensityLight = 0;
            }

            if (_PowerPickUp)
            {
                intensityLight = _batteryPower.PowerIntensityLight;
            }
        }
        else
        {
            _light.intensity = 0f;
            _emissionMaterialFade.OffEmission();

            if (_PowerPickUp)
            {
                intensityLight = _batteryPower.PowerIntensityLight;
            }
        }
    }
}