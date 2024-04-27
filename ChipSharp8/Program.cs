using ImGuiNET;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;


namespace ChipSharp8
{
    class Program
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        // Default controller from the repo
        private static ImGuiController _controller;

        private static Chip _chip;
        private static KeyPad _keyPad;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);

        // Roms must be stored in the root of the project in a folder called "roms"
        private static string[] fileArray = Directory.GetFiles(@"./roms/", "*.ch8");
        private static Texture texture;

        private static bool isPaused = false;

        // Default rom to load is space invaders
        private static string selectedRom = @"./roms/si.ch8";

        private static Vector3 BgColor = new Vector3(0, 0, 0);
        private static Vector3 FgColor = new Vector3(1, 1, 1);


        static void Main(string[] args)
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1117, 662, WindowState.Normal, "Chip#8"),
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

            var stopwatch = Stopwatch.StartNew();
            float deltaTime = 0f;
            _chip = Chip.BootChip(selectedRom);
            _keyPad = new KeyPad(_chip);

            while (_window.Exists)
            {
                deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
                stopwatch.Restart();
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(deltaTime, snapshot);

                ChipDisplay();
                _keyPad.Render();
                if (!isPaused)
                {
                    _chip.EmulateCycle();
                }
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

        private static RgbaByte ConvertToRgbaByte(System.Numerics.Vector3 color)
        {
            return new RgbaByte(
                (byte)(Math.Clamp(color.X, 0.0f, 1.0f) * 255),
                (byte)(Math.Clamp(color.Y, 0.0f, 1.0f) * 255),
                (byte)(Math.Clamp(color.Z, 0.0f, 1.0f) * 255),
                255
            );
        }

        private static unsafe void ChipDisplay()
        {

            ImGui.Begin("Background");
            ImGui.ColorPicker3("ColorPicker3", ref BgColor);
            ImGui.End();


            ImGui.Begin("Foreground");
            ImGui.ColorPicker3("ColorPicker3", ref FgColor);
            ImGui.End();

            // Reset the texture as the docs require a perfect render based on the Chip data
            RgbaByte[] RGBAdata = new RgbaByte[64 * 32];

            RgbaByte bgColorRgba = ConvertToRgbaByte(BgColor);
            RgbaByte fgColorRgba = ConvertToRgbaByte(FgColor);
            for (int i = 0; i < RGBAdata.Length; ++i)
            {
                if (_chip.gfx[i] == 0)
                {
                    RGBAdata[i] = bgColorRgba;
                }
                else
                {
                    RGBAdata[i] = fgColorRgba;
                }
            }


            // Our display is 64*32 for the original Chip-8
            texture = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                            64, 32, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            _gd.UpdateTexture(texture, RGBAdata, 0, 0, 0, 64, 32, 1, 0, 0);

            ImGui.Begin("Chip-8");

            // ImGui.Image requires a pointer to the texture. ImGui.Net might have a better way to do this
            nint ImgPtr = _controller.GetOrCreateImGuiBinding(_gd.ResourceFactory, texture);
            ImGui.Image(ImgPtr, new System.Numerics.Vector2(640, 320));

            ImGui.End();




            ImGui.Begin("Controls");
            if (ImGui.Button("Reset"))
            {
                _chip.Reset(selectedRom);
                // TODO: Reset the keypad properly. There is a bug here where the keypad is not reset properly after rom change
                _keyPad = new KeyPad(_chip);
            }
            if (ImGui.Button(isPaused ? "Play" : "Pause"))
            {
                isPaused = !isPaused;
            }
            if (ImGui.BeginCombo("Select ROM", selectedRom))
            {
                foreach (var file in fileArray)
                {
                    bool isSelected = selectedRom == file;
                    if (ImGui.Selectable(file, isSelected))
                    {
                        selectedRom = file;
                        _chip = Chip.BootChip(selectedRom);
                    }
                    if (isSelected)
                    {
                        // From the ImGui.Net docs
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }



            ImGui.Begin("Registers");

            ImGui.Columns(2, "mycolumns");
            ImGui.Separator();
            ImGui.Text("Register"); ImGui.NextColumn();
            ImGui.Text("Value"); ImGui.NextColumn();
            ImGui.Separator();

            // Edited from the docs example this part as my original code was not working
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
