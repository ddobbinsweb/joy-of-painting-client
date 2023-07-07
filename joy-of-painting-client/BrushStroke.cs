using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joy_of_painting_client
{
    public class BrushStroke
    {
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public string Color { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
