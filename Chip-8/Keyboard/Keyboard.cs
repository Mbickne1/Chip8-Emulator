using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Emulator
{
    public class Keyboard
    {

        private bool[] keys;

        public Keyboard()
        {
            keys = new bool[15];

            for(int i = 0; i < 15; i++)
            {
                keys[i] = false;
            }
        }

        public void KeyPressed(KeyEventArgs args)
        {
            switch(args.KeyCode)
            {
                case Keys.D1:
                    keys[0] = true;
                    break;
                case Keys.D2:
                    keys[1] = true;    
                    break;
                case Keys.D3:
                    keys[2] = true; 
                    break;
                case Keys.D4:
                    keys[3] = true; 
                    break;
                case Keys.Q:
                    keys[4] = true;
                    break;
                case Keys.W:
                    keys[5] = true;
                    break;
                case Keys.E:
                    keys[6] = true;
                    break;
                case Keys.R:
                    keys[7] = true;
                    break;
                case Keys.A:
                    keys[8] = true;
                    break;
                case Keys.S:
                    keys[9] = true;
                    break;
                case Keys.D:
                    keys[10] = true;
                    break;
                case Keys.F:
                    keys[11] = true;
                    break;
                case Keys.Z:
                    keys[12] = true;
                    break;
                case Keys.X:
                    keys[13] = true;
                    break;
                case Keys.C:
                    keys[14] = true;
                    break;
                case Keys.V:
                    keys[15] = true;
                    break;
            }
        }

        public void KeyUp(KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.D1:
                    keys[0] = false;
                    break;
                case Keys.D2:
                    keys[1] = false;
                    break;
                case Keys.D3:
                    keys[2] = false;
                    break;
                case Keys.D4:
                    keys[3] = false;
                    break;
                case Keys.Q:
                    keys[4] = false;
                    break;
                case Keys.W:
                    keys[5] = false;
                    break;
                case Keys.E:
                    keys[6] = false;
                    break;
                case Keys.R:
                    keys[7] = false;
                    break;
                case Keys.A:
                    keys[8] = false;
                    break;
                case Keys.S:
                    keys[9] = false;
                    break;
                case Keys.D:
                    keys[10] = false;
                    break;
                case Keys.F:
                    keys[11] = false;
                    break;
                case Keys.Z:
                    keys[12] = false;
                    break;
                case Keys.X:
                    keys[13] = false;
                    break;
                case Keys.C:
                    keys[14] = false;
                    break;
                case Keys.V:
                    keys[15] = false;
                    break;
            }
        }

        public bool isPressed(int key)
        {
            return keys[key];
        }
    }
}
