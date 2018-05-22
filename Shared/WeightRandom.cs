using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class WeightRandomItem
    {
        public object Target;
        public int Weight;
        public int Index = -1;
        public int DownLimit = 0;
        public int UpLimit = 0;
    }
    public class WeightRandom
    {
        System.Random r = new Random();
        List<WeightRandomItem> _allTargets = new List<WeightRandomItem>();
        int total = 0;
        public WeightRandom()
        {

        }
        public WeightRandom(List<WeightRandomItem> allTargets)
        {

            _allTargets = allTargets;
        }

        public void AddTargets(WeightRandomItem item)
        {
            _allTargets.Add(item);
        }
        public void AddTargetsRrange(List<WeightRandomItem> items)
        {
            _allTargets.AddRange(items);
        }
        public void Clear()
        {
            _allTargets.Clear();
            total = 0;
        }

        public void Ready()
        {
            total = 0;
            for (int i = 0; i < _allTargets.Count; i++)
            {
                _allTargets[i].DownLimit = total;
                total += _allTargets[i].Weight;
                _allTargets[i].UpLimit = total;
                //Console.WriteLine("Target:{0},Download:{1},Up{2}", _allTargets[i].Target, _allTargets[i].DownLimit, _allTargets[i].UpLimit);
            }
            r = new Random();
        }

        public WeightRandomItem Random()
        {
            //WeightRandomItem select=null;
            int val = r.Next(total);
            //Console.WriteLine("val={0}", val);
            for (int i = 0; i < _allTargets.Count; i++)
            {
                if (_allTargets[i].DownLimit <= val && _allTargets[i].UpLimit > val)
                {
                    return _allTargets[i];
                }
            }
            return _allTargets[0];
        }
    }
}
