using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Emit;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;


namespace Sharp8
{
    class Program
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;

        private static Chip _chip;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showAnotherWindow = false;
        private static bool _showMemoryEditor = false;

        static void Main(string[] args)
        {
            // Create window, GraphicsDevice, and all resources necessary for the demo.
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

            _chip = Chip.BootChip("C:\\Users\\Hayden\\Documents\\github\\Projects\\imchip8\\imchip8\\roms\\si.ch8");
            // Main application loop
            while (_window.Exists)
            {
                deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
                stopwatch.Restart();
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(deltaTime, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                ChipDisplay();
                _chip.EmulateCycle();
                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        private static unsafe void ChipDisplay()
        {
            {
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._0)))
                {
                    _chip.KeyDown(0x0);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._1)))
                {
                    _chip.KeyDown(0x1);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._2)))
                {
                    _chip.KeyDown(0x2);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._3)))
                {
                    _chip.KeyDown(0x3);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._4)))
                {
                    _chip.KeyDown(0x4);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._5)))
                {
                    _chip.KeyDown(0x5);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._6)))
                {
                    _chip.KeyDown(0x6);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._7)))
                {
                    _chip.KeyDown(0x7);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._8)))
                {
                    _chip.KeyDown(0x8);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._9)))
                {
                    _chip.KeyDown(0x9);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.A)))
                {
                    _chip.KeyDown(0xA);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.B)))
                {
                    _chip.KeyDown(0xB);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.C)))
                {
                    _chip.KeyDown(0xC);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.D)))
                {
                    _chip.KeyDown(0xD);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.E)))
                {
                    _chip.KeyDown(0xE);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.F)))
                {
                    _chip.KeyDown(0xF);
                }

                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._0)))
                {
                    _chip.KeyUp(0x0);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._1)))
                {
                    _chip.KeyUp(0x1);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._2)))
                {
                    _chip.KeyUp(0x2);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._3)))
                {
                    _chip.KeyUp(0x3);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._4)))
                {
                    _chip.KeyUp(0x4);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._5)))
                {
                    _chip.KeyUp(0x5);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._6)))
                {
                    _chip.KeyUp(0x6);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._7)))
                {
                    _chip.KeyUp(0x7);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._8)))
                {
                    _chip.KeyUp(0x8);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._9)))
                {
                    _chip.KeyUp(0x9);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.A)))
                {
                    _chip.KeyUp(0xA);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.B)))
                {
                    _chip.KeyUp(0xB);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.C)))
                {
                    _chip.KeyUp(0xC);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.D)))
                {
                    _chip.KeyUp(0xD);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.E)))
                {
                    _chip.KeyUp(0xE);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.F)))
                {
                    _chip.KeyUp(0xF);
                }

            }

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

                // Create a texture
                Texture texture = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    64, 32, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));

                // Update the texture
                _gd.UpdateTexture(texture, RGBAdata, 0, 0, 0, 64, 32, 1, 0, 0);

                ImGui.Begin("Chip-8", ref _showAnotherWindow);

                // Render the texture
                nint ImgPtr = _controller.GetOrCreateImGuiBinding(_gd.ResourceFactory, texture); //This returns the intPtr need for Imgui.Image()
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
