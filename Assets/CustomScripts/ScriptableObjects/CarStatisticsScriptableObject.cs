using UnityEngine;

[CreateAssetMenu(fileName = "CarStatisticsDatabase", menuName = "Scriptable Objects/CarStatisticsDB")]
public class CarStatisticsDatas : ScriptableObject
{
    public CarStatistics[] _carStatistics;

    public int CarCount
    {
        get { return _carStatistics.Length; }
    }
}
