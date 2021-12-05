using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    public static float CountTime { get; private set; }
    private Text Timer;

    void Start()
    {
        CountTime = 0;
        Timer = GetComponent<Text>();
    }

    void Update()
    {
        if (Controls.is_pause) return;

        CountTime += Time.deltaTime;

        int t = (int)CountTime;

        int sec = t % 60;
        int min = t / 60;

        Timer.text = (min < 10 ? "0" + min : min.ToString()) + ":" + (sec < 10 ? "0" + sec : sec.ToString());
    }

    public void ClockReset()
    {
        CountTime = 0;
    }
}
