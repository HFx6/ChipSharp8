using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using System.IO;
using System.Reflection.Emit;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Reflection;


namespace ChipSharp8
{
    class Program
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;

        private static Chip _chip;
        private static KeyPad _keyPad;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showAnotherWindow = false;
        private static bool _showMemoryEditor = false;

        static void Main(string[] args)
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "Chip#8"),
                new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

            Random random = new Random();

            var stopwatch = Stopwatch.StartNew();
            float deltaTime = 0f;
            _chip = Chip.BootChip(@"./roms/si.ch8");
            _keyPad = new KeyPad(_chip);
            // Main application loop
            while (_window.Exists)
            {
                deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
                stopwatch.Restart();
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(deltaTime, snapshot);

                ChipDisplay();
                _keyPad.Render();
                _chip.EmulateCycle();
                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        private static unsafe void ChipDisplay()
        {

            {
                RgbaByte[] RGBAdata = new RgbaByte[64 * 32];
                for (int i = 0; i < RGBAdata.Length; ++i)
                    if (_chip.gfx[i] == 0)
                    {
                        RGBAdata[i] = new RgbaByte((byte)0, 0, 0, 0);
                    }
                    else
                    {
                        RGBAdata[i] = new RgbaByte((byte)255, 255, 255, 255);

                    }

                
                Texture texture = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    64, 32, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));

               
                _gd.UpdateTexture(texture, RGBAdata, 0, 0, 0, 64, 32, 1, 0, 0);

                ImGui.Begin("Chip-8", ref _showAnotherWindow);

               
                nint ImgPtr = _controller.GetOrCreateImGuiBinding(_gd.ResourceFactory, texture); 
                ImGui.Image(ImgPtr, new System.Numerics.Vector2(640, 320));

                ImGui.End();

            }
            {
                ImGui.Begin("Registers", ref _showMemoryEditor);

                ImGui.Columns(2, "mycolumns"); // 2-ways, with border
                ImGui.Separator();
                ImGui.Text("Register"); ImGui.NextColumn();
                ImGui.Text("Value"); ImGui.NextColumn();
                ImGui.Separator();

                var registers = new List<(string, string)>
                    {
                        ("PC", $"0x{_chip.pc:X4}"),
                        ("OpCode", $"0x{_chip.opcode:X4}"),
                        ("I", $"0x{_chip.I:X4}"),
                        ("sp", $"0x{_chip.sp:X4}"),
                        ("DelayTimer", $"{_chip.delay_timer}"),
                        ("SoundTimer", $"{_chip.sound_timer}")
                    };

                foreach (var register in Enumerable.Range(0, 16))
                {
                    registers.Add(($"V{register:X}", $"0x{_chip.V[register]:X2}"));
                }

                foreach (var (register, value) in registers)
                {
                    ImGui.Text(register); ImGui.NextColumn();
                    ImGui.Text(value); ImGui.NextColumn();
                }

                ImGui.Columns(1);
                ImGui.Separator();
                ImGui.End();
            }
        }
    }
}
