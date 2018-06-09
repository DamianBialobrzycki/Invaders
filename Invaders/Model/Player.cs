using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders.Model
{
    class Player : Ship
    {
        static PlayerSize Size = new PlayerSize(25, 15);
        const double Speed = 10;

        public Player(Point location, Size size) : base(location, size)
        {
            
        }
    }
}
