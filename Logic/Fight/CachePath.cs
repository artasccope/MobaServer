using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Logic.Fight
{
    public class CachePath
    {
        public List<int> path;
        public int lastReturnNodeIndex = 0;

        public CachePath(List<int> path) {
            this.path = path;
        }
    }
}
