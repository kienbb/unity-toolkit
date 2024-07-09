using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Random = UnityEngine.Random;
//using System.Security.Policy;

namespace FreeTimeGames
{
    public static class AbiExtensions
    {        
        public static IList<T> Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
            return ts;
        }
        public static string ToStringReadDHMS(this TimeSpan time)
        {
            return time.Days < 1 ? (time.Hours < 1 ? (time.Minutes < 1 ? $"{time.Seconds}s" : $"{time.Minutes}m {time.Seconds}s") : $"{time.Hours}h {time.Minutes}m {time.Seconds}s") : $"{time.Days}d {time.Hours}h {time.Minutes}m {time.Seconds}s";
        }
        public static string ToStringRead(this TimeSpan time)
        {
            return time.ToString(@"d\:h\:m\:s");
        }
        public static string ToStringReadDHM(this TimeSpan time)
        {
            return time.Days < 1 ? (time.Hours < 1 ? $"{time.Minutes}M" : $"{time.Hours}H {time.Minutes}M") : $"{time.Days}D {time.Hours}H {time.Minutes}M";
        }

        public static void SetText(this Text txt, string content)
        {
            txt.text = content;
        }

        public static T ToEnum<T>(this string value, bool ignoreCase = true)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static string ToStringLine(this IEnumerable<int> list)
        {
            string result = "";
            int c = list.Count();
            for (int i = 0; i < c; i++)
            {
                result += $"{list.ElementAt(i)};";
            }
            if (c > 0)
                result = result.Remove(result.Length - 1);
            return result;
        }

        public static string ToStringLine(this IEnumerable<float> list)
        {
            string result = "";
            int c = list.Count();
            for (int i = 0; i < c; i++)
            {
                result += $"{list.ElementAt(i)};";
            }
            if (c > 0)
                result = result.Remove(result.Length - 1);
            return result;
        }

        public static string ToStringLine(this IEnumerable<string> list)
        {
            string result = "";
            int c = list.Count();
            for (int i = 0; i < c; i++)
            {
                result += $"{list.ElementAt(i)};";
            }
            if (c > 0)
                result = result.Remove(result.Length - 1);
            return result;
        }

        public static List<int> ToListInt(this string str, int ignoreValue = -1)
        {
            if (string.IsNullOrEmpty(str))
                return new List<int>();

            var res = str.Split(';').ToList().ConvertAll(x => int.Parse(x));
            res.RemoveAll(x => x == ignoreValue);
            return res;
        }

        public static List<float> ToListFloat(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return new List<float>();

            return str.Split(';').ToList().ConvertAll(x => float.Parse(x));
        }

        public static List<string> ToListString(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return new List<string>();

            return str.Split(';').ToList();
        }

        public static int IndexOf<T>(this T[] arr, T ele)
        {
            int result = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(ele))
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        public static T[] Insert<T>(this T[] arr, T ele)
        {
            Array.Resize(ref arr, arr.Length + 1);
            arr[arr.Length - 1] = ele;
            return arr;
        }

        public static string ShowTimeSpan(this TimeSpan iTimeSpan, bool needToShowHourAsZero = true)
        {
            if (iTimeSpan.Days > 0)
            {
                return $"{iTimeSpan.Days}d {iTimeSpan.Hours:00}:{iTimeSpan.Minutes:00}:{iTimeSpan.Seconds:00}";
            }
            if (needToShowHourAsZero)
            {
                return $"{iTimeSpan.Hours:00}:{iTimeSpan.Minutes:00}:{iTimeSpan.Seconds:00}";
            }
            if (iTimeSpan.Hours <= 0)
            {
                return $"{iTimeSpan.Minutes:00}:{iTimeSpan.Seconds:00}";
            }
            return $"{iTimeSpan.Hours:00}:{iTimeSpan.Minutes:00}:{iTimeSpan.Seconds:00}";
        }

        public static string ToShortString(this double iValue)
        {
            bool flag = true;
            if (double.IsNaN(iValue))
            {
                return "???";
            }
            string text = iValue.ToString("e5");
            int num = text.IndexOf("e");
            if (num < 0)
            {
                return iValue.ToString("G3");
            }
            int result = 0;
            int.TryParse(text.Substring(num + 1), out result);
            text = text.Substring(0, num);
            string text2 = text;
            string empty = string.Empty;
            if (result < 3 || (result < 6 && !flag))
            {
                if (iValue < 1.0)
                {
                    return ((float)(int)(iValue * 100.0) / 100f).ToString();
                }
                return Math.Floor(iValue).ToString();
            }
            text2 = text2.Remove(1, 1);
            text2 = text2.Remove(3 + result % 3);
            text2 = text2.Insert(1 + result % 3, ".");
            int num2 = result / 3;
            if (0 == 0)
            {
                if (num2 < 2)
                {
                    empty = "K";
                }
                else if (num2 < 3)
                {
                    empty = "M";
                }
                else if (num2 < 4)
                {
                    empty = "B";
                }
                else if (num2 < 5)
                {
                    empty = "T";
                }
                else if (num2 < 6)
                {
                    empty = "aa";
                }
                else if (num2 < 7)
                {
                    empty = "bb";
                }
                else if (num2 < 8)
                {
                    empty = "cc";
                }
                else if (num2 < 9)
                {
                    empty = "dd";
                }
                else if (num2 < 10)
                {
                    empty = "ee";
                }
                else if (num2 < 11)
                {
                    empty = "ff";
                }
                else if (num2 < 12)
                {
                    empty = "gg";
                }
                else if (num2 < 13)
                {
                    empty = "hh";
                }
                else if (num2 < 14)
                {
                    empty = "ii";
                }
                else if (num2 < 15)
                {
                    empty = "jj";
                }
                else if (num2 < 16)
                {
                    empty = "kk";
                }
                else if (num2 < 17)
                {
                    empty = "ll";
                }
                else if (num2 < 18)
                {
                    empty = "mm";
                }
                else if (num2 < 19)
                {
                    empty = "nn";
                }
                else if (num2 < 20)
                {
                    empty = "oo";
                }
                else if (num2 < 21)
                {
                    empty = "pp";
                }
                else if (num2 < 22)
                {
                    empty = "qq";
                }
                else if (num2 < 23)
                {
                    empty = "rr";
                }
                else if (num2 < 24)
                {
                    empty = "ss";
                }
                else if (num2 < 25)
                {
                    empty = "tt";
                }
                else if (num2 < 26)
                {
                    empty = "uu";
                }
                else if (num2 < 27)
                {
                    empty = "vv";
                }
                else if (num2 < 28)
                {
                    empty = "ww";
                }
                else if (num2 < 29)
                {
                    empty = "xx";
                }
                else if (num2 < 30)
                {
                    empty = "yy";
                }
                else
                {
                    if (num2 >= 31)
                    {
                        return iValue.ToString("G3");
                    }
                    empty = "zz";
                }
                return text2 + empty;
            }
        }

        public static string ShowTimeSpanHoursMinutesSeconds(this TimeSpan iTimeSpan)
        {
            return $"{(int)iTimeSpan.TotalHours:00}:{iTimeSpan.Minutes:00}:{iTimeSpan.Seconds:00}";
        }

        public static string ShowTimeSpanHoursMinutes(this TimeSpan iTimeSpan)
        {
            return $"{(int)iTimeSpan.TotalHours:00}:{iTimeSpan.Minutes:00}";
        }

        public static string ShowTimeSpanHoursMinutesAMPM(this TimeSpan iTimeSpan)
        {
            bool flag = false;
            int num = iTimeSpan.Hours;
            if (num >= 12)
            {
                flag = true;
            }
            if (num >= 13)
            {
                num -= 12;
            }
            string str = $"{num:00}:{iTimeSpan.Minutes:00}";
            if (flag)
            {
                return str + "pm";
            }
            return str + "am";
        }

        public static string ShowTimeMMSS(this int timeInSeconds)
        {
            int num = timeInSeconds / 60;
            int num2 = timeInSeconds % 60;
            return $"{num:00}:{num2:00}";
        }

        public static string ShowTimeMSS(this int timeInSeconds)
        {
            int num = timeInSeconds / 60;
            int num2 = timeInSeconds % 60;
            return $"{num:0}:{num2:00}";
        }

        public static string ToRoman(this int number)
        {
            if (number < 0 || number > 3999)
            {
                return string.Empty;
            }
            if (number < 1)
            {
                return string.Empty;
            }
            if (number >= 1000)
            {
                return "M" + ToRoman(number - 1000);
            }
            if (number >= 900)
            {
                return "CM" + ToRoman(number - 900);
            }
            if (number >= 500)
            {
                return "D" + ToRoman(number - 500);
            }
            if (number >= 400)
            {
                return "CD" + ToRoman(number - 400);
            }
            if (number >= 100)
            {
                return "C" + ToRoman(number - 100);
            }
            if (number >= 90)
            {
                return "XC" + ToRoman(number - 90);
            }
            if (number >= 50)
            {
                return "L" + ToRoman(number - 50);
            }
            if (number >= 40)
            {
                return "XL" + ToRoman(number - 40);
            }
            if (number >= 10)
            {
                return "X" + ToRoman(number - 10);
            }
            if (number >= 9)
            {
                return "IX" + ToRoman(number - 9);
            }
            if (number >= 5)
            {
                return "V" + ToRoman(number - 5);
            }
            if (number >= 4)
            {
                return "IV" + ToRoman(number - 4);
            }
            if (number >= 1)
            {
                return "I" + ToRoman(number - 1);
            }
            return string.Empty;
        }


#if UNITY_EDITOR
        public static GameObject GetAssetsGameObject(string name)
        {
            GameObject result = null;

            string[] guids = AssetDatabase.FindAssets("t:prefab " + name, new[] { "Assets/_IdleTd/Prefabs" });
            if (guids.Length > 0)
            {
                string pathToPrefab = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<GameObject>(pathToPrefab);
            }

            return result;
        }
#endif
        public static Rect GetViewportRect(this Bounds bounds, Camera cam)
        {
            Vector3 cen = bounds.center;
            Vector3 ext = bounds.extents;

            Vector2 min = cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z));
            Vector2 max = min;


            //0
            Vector2 point = min;
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

            //1
            point = cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z));
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);


            //2
            point = cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z));
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

            //3
            point = cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z));
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

            //4
            point = cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z));
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

            //5
            point = cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z));
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

            //6
            point = cam.WorldToViewportPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z));
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

            //7
            point = cam.WorldToViewportPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z));
            min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
            max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public static T GetRandom<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null || enumerable.Count() == 0)
                return default(T);
            int n = Random.Range(0, enumerable.Count());
            return enumerable.ElementAt(n);
        }

        public static List<T> GetRandom<T>(this IEnumerable<T> enumerable, int count)
        {
            if (enumerable == null)
                return null;

            if (enumerable.Count() <= count)
            {
                return enumerable.ToList();
            }

            List<T> ret = new List<T>();
            while (ret.Count < count)
            {
                int n = Random.Range(0, enumerable.Count());
                T e = enumerable.ElementAt(n);
                if (!ret.Contains(e))
                {
                    ret.Add(e);
                }
            }

            return ret;
        }

        private static AnimationCurve jumpingCurve = new AnimationCurve()
        {
            keys = new Keyframe[] {
                new Keyframe(0, 0,  6.2f,   6.2f,   0.333f, 0.17f),
                new Keyframe(0.5f, 1f, 0,   0,   0.5f, 0.5f),
                new Keyframe(1f, 0,  -6.2f,   -6.2f,   0.17f, 0.333f),
            }
        };

        public static Coroutine JumpTo(this Transform jumper, Vector3 endPos, float height, float jumpTime, Action actionEnd = null, Vector3? startPos = null)
        {
            return CoroutineRunner.Instance.StartCoroutine(RoutineJumpTo(jumper, endPos, height, jumpTime, actionEnd, startPos));
        }
        private static IEnumerator RoutineJumpTo(Transform jumper, Vector3 endPos, float height, float jumpTime, Action actionEnd = null, Vector3? startPosCustom = null)
        {
            float flyTime = 0f;
            float absHeight = height + Mathf.Abs(jumper.position.y - endPos.y);
            Vector3 startPos = startPosCustom ?? jumper.position;
            jumper.transform.position = startPos;
            while (flyTime < jumpTime)
            {
                if (jumper == null)
                {
                    actionEnd?.Invoke();
                    yield break;
                }
                float eval = flyTime / jumpTime; ;
                jumper.position = Vector3.Lerp(startPos, endPos, eval) + jumpingCurve.Evaluate(eval) * absHeight * Vector3.up;
                flyTime += Time.deltaTime;
                yield return null;
            }
            actionEnd?.Invoke();
            jumper.position = endPos;
        }

        public static Vector2 GetViewportPos(this GameObject element)
        {
            return CalculateViewportPos(element);
        }

        public static Vector2 CalculateViewportPos(GameObject element)
        {
            Vector2 viewPortPos = Vector2.zero;
            //if (element.transform is RectTransform)
            //{
            //    ///là ui element
            //    ///
            //    RectTransform eleRecTr = element.transform as RectTransform;
            //    viewPortPos = CameraList.Instance.CameraUI.WorldToViewportPoint(eleRecTr.position);
            //}
            //else
            //{
            //    ///là model 3d, lấy theo render box bounder
            //    ///
            //    var mr = element.GetComponentsInChildren<Renderer>();
            //    if (mr == null || mr.Length == 0)
            //    {
            //        //Debug.LogError($"Element {element.name} doesnt have any Renderer Component in its children", element);
            //        return Vector2.zero;
            //    }

            //    var minx = mr.Select(x => x.bounds.GetViewportRect(CameraList.Instance.CameraGame).min.x).OrderBy(x => x).ToList()[0];
            //    var miny = mr.Select(x => x.bounds.GetViewportRect(CameraList.Instance.CameraGame).min.y).OrderBy(x => x).ToList()[0];
            //    var maxx = mr.Select(x => x.bounds.GetViewportRect(CameraList.Instance.CameraGame).max.x).OrderByDescending(x => x).ToList()[0];
            //    var maxy = mr.Select(x => x.bounds.GetViewportRect(CameraList.Instance.CameraGame).max.y).OrderByDescending(x => x).ToList()[0];
            //    var min = new Vector2(minx, miny);
            //    var max = new Vector2(maxx, maxy);

            //    viewPortPos = (min + max) * 0.5f;
            //}

            return viewPortPos;
        }

        public static Dictionary<int, Color> colorByLevel = new Dictionary<int, Color>()
        {
            [0] = new Color(1f, 1f, 1f, 1f),
            [1] = new Color(0.25f, 1f, 0.3f, 1f),
            [2] = new Color(0.9f, 0.25f, 1f, 1f),
        };
        public static Dictionary<int, string> frameByLevel = new Dictionary<int, string>()
        {
            [0] = "frame_0",
            [1] = "frame_1",
            [2] = "frame_2",
        };
        public static string Break(this string input)
        {
            return input.Replace("\\n", "\n");
        }

        public static int ToInt(this string input)
        {
            int r = -1;
            int.TryParse(input, out r);
            return r;
        }
            
    }

    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;
        public static CoroutineRunner Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindObjectOfType<CoroutineRunner>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = "===COROUTINE RUNNER===";
                        instance = go.AddComponent<CoroutineRunner>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        public static Coroutine Run(IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }
        
    }
}
