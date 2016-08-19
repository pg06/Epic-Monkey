using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorPositionMacro
{
    public class Coordinates
    {
        private string label;
        private int x;
        private int y;
        private int number;

        public Coordinates(string label_, int x_, int y_, int number_)
        {
            this.label = label_;
            this.x = x_;
            this.y = y_;
            this.number = number_;
        }

        public string Label
        {
            get { return label; }
        }
        public int X
        {
            get { return x; }
        }
        public int Y
        {
            get { return y; }
        }
        public int NUMBER
        {
            get { return number; }
        }
    }
}
