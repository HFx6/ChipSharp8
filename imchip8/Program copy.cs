// using System;
// using System.Numerics;
// using ImGuiNET;
// using Veldrid;
// using Veldrid.Sdl2;
// using Veldrid.StartupUtilities;

// namespace Sharp8
// {
//     class Program
//     {
//         private static Chip _chip;
//         private static Sdl2Window _window;
//         private static GraphicsDevice _gd;
//         private static ImGuiController _controller;

//  static void Main(string[] args)
// {
//     VeldridStartup.CreateWindowAndGraphicsDevice(
//         new WindowCreateInfo(50, 50, 640, 320, WindowState.Normal, "Chip8 Emulator"),
//         new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
//         out _window,
//         out _gd);

//     ImGui.CreateContext();
//     _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

//     _chip = Chip.BootChip("C:/Users/Hayden/Documents/github/Projects/imchip8/imchip8/roms/ibm_logo.ch8");

//     while (_window.Exists)
//     {
//         InputSnapshot snapshot = _window.PumpEvents();
//         if (!_window.Exists) { break; }
//         _controller.Update(1f / 60f, snapshot); // Use appropriate timestep for your application

//         RenderUI();

//         _gd.SwapBuffers(_gd.MainSwapchain);
//     }

//     _gd.Dispose();
// }

//         private static void RenderUI()
//         {
//             ImGui.Begin("Chip8 Emulator");

//             for (int i = 0; i < _chip.gfx.Length; i++)
//             {
//                 Vector4 pixelColor = _chip.gfx[i] > 0 ? new Vector4(1, 1, 1, 1) : new Vector4(0, 0, 0, 1);
//                 ImGui.PushStyleColor(ImGuiCol.Button, pixelColor);
//                 ImGui.PushStyleColor(ImGuiCol.ButtonHovered, pixelColor);
//                 ImGui.PushStyleColor(ImGuiCol.ButtonActive, pixelColor);

//                 if (ImGui.Button($"##pixel{i}", new Vector2(10, 10)))
//                 {
//                     // Handle click event here if needed
//                 }

//                 ImGui.PopStyleColor(3);

//                 bool nextLine = (i + 1) % 64 == 0;
//                 if (!nextLine)
//                 {
//                     ImGui.SameLine();
//                 }
//             }

//             ImGui.End();
//         }
//     }
// }