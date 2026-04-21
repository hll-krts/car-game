using UnityEngine;
using TMPro;

public class CustomTimeManager : MonoBehaviour
{
    private static int Minutes, Seconds;
    private static float Milliseconds;
    private static string Milliseconds_String;

    private static int _LastMinutes = 0, _LastSeconds = 0;
    private static float _LastMilliseconds = 0;

    private static int _BestMinutes = 0, _BestSeconds = 0;
    private static float _BestMilliseconds = 0;

    private bool timerStarted;

    [SerializeField] private TextMeshProUGUI MinuteText, SecondsText, MilliText;
    [SerializeField] private TextMeshProUGUI LastMinuteText, LastSecondsText, LastMilliText;
    [SerializeField] private TextMeshProUGUI BestMinuteText, BestSecondsText, BestMilliText;

    //private void Start()
    //{
    //    MinuteText = MinuteTextObj.GetComponent<TextMeshProUGUI>();
    //    SecondsText = SecondsTextObj.GetComponent<TextMeshProUGUI>();
    //    MilliText = MilliTextObj.GetComponent<TextMeshProUGUI>();
    //}


    private void Update()
    {
        if (timerStarted == true)
        {
            Milliseconds += Time.deltaTime * 100;
            if (Milliseconds >= 100)
            {
                Seconds++;
                Milliseconds = 0;
            }
            if (Seconds >= 60)
            {
                Minutes++;
                Seconds = 0;
            }
            if (Milliseconds < 10)
            {
                Milliseconds_String = "00" + Mathf.FloorToInt(Milliseconds).ToString();
            }
            else if (Milliseconds < 100)
            {
                Milliseconds_String = "0" + Mathf.FloorToInt(Milliseconds).ToString();
            }
            else
            {
                Milliseconds_String = Mathf.FloorToInt(Milliseconds).ToString();
            }
            MinuteText.text = Minutes.ToString("00") + ":";
            SecondsText.text = Seconds.ToString("00") + ".";
            MilliText.text = Milliseconds_String;
        }
    }


    public void BestLapTime()
    {
        _BestMilliseconds = _LastMilliseconds;
        _BestSeconds = _LastSeconds;
        _BestMinutes = _LastMinutes;

        BestMinuteText.text = Minutes.ToString("00") + ":";
        BestSecondsText.text = Seconds.ToString("00") + ".";
        BestMilliText.text = Milliseconds_String;
    }
    public void LastLapTime()
    {
        _LastMinutes = Minutes;
        _LastSeconds = Seconds;
        _LastMilliseconds = Milliseconds;

        if (Milliseconds < 10)
        {
            Milliseconds_String = "00" + Mathf.FloorToInt(Milliseconds).ToString();
        }
        else if (Milliseconds < 100)
        {
            Milliseconds_String = "0" + Mathf.FloorToInt(Milliseconds).ToString();
        }
        else
        {
            Milliseconds_String = Mathf.FloorToInt(Milliseconds).ToString();
        }
        LastMinuteText.text = Minutes.ToString("00") + ":";
        LastSecondsText.text = Seconds.ToString("00") + ".";
        LastMilliText.text = Milliseconds_String;

        if (_BestMinutes >= _LastMinutes && _BestSeconds >= _LastSeconds && _BestMilliseconds > _LastMilliseconds)
        {
            BestLapTime();
        }
    }
    public void CheckLapTime()
    {
        LastLapTime();
        TimerReset();
    }
    public void TimerReset()
    {
        Minutes = 0;
        Seconds = 0;
        Milliseconds = 0;
    }

    public void StartTimer()
    {
        timerStarted = true;
    }
    public void StopTimer()
    {
        timerStarted = false;
        CheckLapTime();
    }
}
