using System.Collections.Generic;
using UnityEngine;

public class ChooseLightCookie : MonoBehaviour
{
    public List<Texture> lightCookie = new List<Texture>();

    private Light _thisLight;

    private int _scrollList = 0;

    private PlayerInputHandler _input;

    private bool _lastInput;

    void Awake()
    {
        _thisLight = GetComponent<Light>();
        _input = FindFirstObjectByType<PlayerInputHandler>();
    }

    void Update()
    {
        ChooseCookie();
    }

    void ChooseCookie()
    {
        if (_input.cycleFlashlightCookie && !_lastInput)
        {
            _scrollList++;

            if (_scrollList >= lightCookie.Count)
            {
                _scrollList = 0;
            }

            _thisLight.cookie = lightCookie[_scrollList];
        }

        _lastInput = _input.cycleFlashlightCookie;
    }
}