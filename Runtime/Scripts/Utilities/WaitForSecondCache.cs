using UnityEngine;
using System.Collections.Generic;


public class WaitForSecondCache
{
    public readonly static WaitForSeconds WAIT_TIME_MIN = new WaitForSeconds(0.05f);
    public readonly static WaitForSeconds WAIT_TIME_ZERO_POINT_ONE = new WaitForSeconds(0.1f);
    public readonly static WaitForSeconds WAIT_TIME_QUARTER = new WaitForSeconds(0.25f);
    public readonly static WaitForSeconds WAIT_TIME_ZERO_POINT_THIRTY_FIVE = new WaitForSeconds(0.35f);
    public readonly static WaitForSeconds WAIT_TIME_HAFT = new WaitForSeconds(0.5f);
    public readonly static WaitForSeconds WAIT_TIME_ONE = new WaitForSeconds(1f);
    public readonly static WaitForSeconds WAIT_TIME_ONE_POINT_FIVE = new WaitForSeconds(1.5f);
    public readonly static WaitForSeconds WAIT_TIME_TWO = new WaitForSeconds(2f);
    public readonly static WaitForSeconds WAIT_TIME_THREE = new WaitForSeconds(3f);
    public readonly static WaitForSeconds WAIT_TIME_FOUR = new WaitForSeconds(4f);
    public readonly static WaitForSeconds WAIT_TIME_FIVE = new WaitForSeconds(5f);
    public readonly static WaitForSeconds WAIT_TIME_EIGHT = new WaitForSeconds(8f);

    public readonly static WaitForSecondsRealtime WAIT_REAL_ZERO_POINT_ONE = new WaitForSecondsRealtime(0.1f);
    public readonly static WaitForSecondsRealtime WAIT_REAL_TWO = new WaitForSecondsRealtime(2f);
    public readonly static WaitForSecondsRealtime WAIT_REAL_ONE = new WaitForSecondsRealtime(1f);
    public readonly static WaitForSecondsRealtime WAIT_REAL_THREE = new WaitForSecondsRealtime(3f);

    private static Dictionary<float, WaitForSeconds> _dictionaryWPS = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds GetWFSCache(float key)
    {
        if(!_dictionaryWPS.ContainsKey(key))
        {
            _dictionaryWPS[key] = new WaitForSeconds(key);
        }

        return _dictionaryWPS[key];
    }
}
