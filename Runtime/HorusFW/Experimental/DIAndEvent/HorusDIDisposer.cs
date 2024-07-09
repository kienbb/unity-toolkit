using System.Collections.Generic;
using UnityEngine;

namespace HorusFW.DI
{
    public class HorusDIDisposer : MonoBehaviour
    {
        private readonly List<(object, object)> Objs = new List<(object, object)>();
        internal void Add(object obj, object containerKey)
        {
            if (obj == null)
                return;
            if (containerKey == null)
                containerKey = HorusDI.globalContainerKey;
            if (Objs.Contains((obj, containerKey)) == false)
                Objs.Add((obj, containerKey));
        }

        private void OnDestroy()
        {
            for (int i = 0; i < Objs.Count; i++)
            {                
                HorusDI.UnRegister(Objs[i].Item1, Objs[i].Item2);
            }
            Objs.Clear();
        }
    }
}
