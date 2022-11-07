using System.Diagnostics;
using System.Windows.Forms;
using Chip8Emulator.Debugger;

namespace Chip8Emulator
{
    public partial class GUI : Form
    {
        private readonly int WIDTH = 1280; //40x the original 64x32 screen size. 
        private readonly int HEIGHT = 640;
        private readonly int SCALE = 20;

        private readonly Color pixelOff;
        private readonly Color pixelOn;

        private readonly Bitmap _screen;
        private readonly EmulatorController _emulatorController;
        private Keyboard _keyboard;
        public GUI()
        {
            InitializeComponent();

            pBox.KeyDown += PBox_KeyDown;
            pBox.KeyUp += PBox_KeyUp;
            this.Load += Screen_Load;

            _screen = new Bitmap(WIDTH, HEIGHT);
            
            pixelOff = Color.Black;
            pixelOn = Color.White;

            _keyboard = new Keyboard();
            _emulatorController = new EmulatorController(this, _keyboard);
        }

        private void PBox_KeyDown(object sender, KeyEventArgs args)
        {
            _keyboard.KeyPressed(args);
        }

        private void PBox_KeyUp(object sender, KeyEventArgs args)
        {
            _keyboard.KeyUp(args);
        }

        protected override void OnLoad(EventArgs e)
        {
            DebugScreen debugger = new DebugScreen();
            debugger.Show();
            Task.Run(_emulatorController.EmulationLoop);
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            DebugScreen debugger = new DebugScreen();
            debugger.Show();
        }

        public void Clear()
        {
            for(int i = 0; i < WIDTH; i++)
            {
                for(int j = 0; j < HEIGHT; j++)
                { 
                    _screen.SetPixel(i, j, pixelOff);
                }
            }

            pBox.Image = _screen;
        }

        public void DrawDisplay(byte[,] buffer)
        {
            for (int i = 0; i < buffer.GetLength(0); i++)
            {
                for (int j = 0; j < buffer.GetLength(1); j++)
                {
                    if (buffer[i, j] == 0)
                    {
                        _screen.SetPixel(i, j, pixelOff);
                    }  
                    else
                    {
                        _screen.SetPixel(i, j, pixelOn);
                    }
                }
            }

            pBox.Image = _screen;
        }

        public bool GetPixelOn(int row, int col)
        {
            Color pixel = _screen.GetPixel(row, col);

            if(pixel.Equals(pixelOn))
            {
                return true;
            }

            return false;
        }

        public void SetPixel(int row, int col, bool turnOn)
        {
            
            if (turnOn)
            {
                _screen.SetPixel(row, col, pixelOn);
            } else
            {
                _screen.SetPixel(row, col, pixelOff);
            }
        }

        public int GetWidth()
        {
            return WIDTH;
        }

        public int GetHeight()
        {
            return HEIGHT;
        }

        public int GetScale()
        {
            return SCALE;
        }
    }
}