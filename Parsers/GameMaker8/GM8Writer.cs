using CTFAK.CCN;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace ZelCTFTranslator.Parsers.GameMaker
{
    public class GM8Writer
    {
        private static ByteWriter writer = new ByteWriter(new MemoryStream());
        private static ByteWriter ZlibWriter = new ByteWriter(new MemoryStream());

        public static void Write(GameData gameData)
        {
            Logger.Log("Creating Game Maker 8 Project File.");
            writer = new ByteWriter(new MemoryStream());
            // Game Maker 8 Header
            writer.WriteInt32(1234321);
            // Version
            writer.WriteInt32(810);
            // Game ID
            writer.WriteInt32(RandomNumberGenerator.GetInt32(2147483647));
            // GUID
            writer.WriteInt32(RandomNumberGenerator.GetInt32(2147483647));
            writer.WriteInt16((short)RandomNumberGenerator.GetInt32(32767));
            writer.WriteInt16((short)RandomNumberGenerator.GetInt32(32767));
            writer.WriteInt64(RandomNumberGenerator.GetInt32(2147483647) * 
                              RandomNumberGenerator.GetInt32(2147483647));

            // Version
            writer.WriteInt32(800);

            Logger.Log("Writing Settings.");
            //Zlib Writer
            WriteSettings(gameData);
        }

        public static void WriteSettings(GameData gameData)
        {
            // Settings
                // Start in full-screen mode
                    ZlibWriter.WriteInt32(gameData.header.Flags["FullscreenAtStart"] ? 1 : 0);
                // Interpolate colors between pixels
                    ZlibWriter.WriteInt32(gameData.ExtHeader.Flags["AntiAliasingWhenResizing"] ? 1 : 0);
                // Don't draw a border when in windowed mode
                    ZlibWriter.WriteInt32(gameData.header.NewFlags["NoThickFrame"] ? 1 : 0);
                // Display the cursor
                    ZlibWriter.WriteInt32(1);
                // Scaling
                    ZlibWriter.WriteInt32(gameData.header.Flags["Stretch"] ? 2 : 1);
                // Allow Resize
                    ZlibWriter.WriteInt32(0);
            ZlibWriter.WriteInt32(0);   // Always on Top    
            ZlibWriter.WriteInt32(0);   // Border Color
            ZlibWriter.WriteInt32(0);   // Set Resolution
            ZlibWriter.WriteInt32(0);   // Color Depth
            ZlibWriter.WriteInt32(0);   // Resolution
            ZlibWriter.WriteInt32(0);   // Frequency
            ZlibWriter.WriteInt32(0);   // Don't Show Button
            ZlibWriter.WriteInt32(0);   // VSync
            ZlibWriter.WriteInt32(1);   // Disable Screensavers
            ZlibWriter.WriteInt32(1);   // F4 to Fullscreen
            ZlibWriter.WriteInt32(1);   // F1 for Game Info
            ZlibWriter.WriteInt32(1);   // Esc to Close Game
            ZlibWriter.WriteInt32(1);   // F5/F6 to Save/Load
            ZlibWriter.WriteInt32(1);   // F9 to Screenshot
            ZlibWriter.WriteInt32(1);   // Close as Esc
            ZlibWriter.WriteInt32(0);   // Game Process Priority
            ZlibWriter.WriteInt32(0);   // Freeze on Lost Focus
            ZlibWriter.WriteInt32(1);   // Loading Bar Mode
            ZlibWriter.WriteInt32(0);   // Back Bar Image
            ZlibWriter.WriteInt32(0);   // Front Bar Image
            ZlibWriter.WriteInt32(0);   // Show Custom Load Image
            ZlibWriter.WriteInt32(0);   // Custom Load Image Exists
            ZlibWriter.WriteInt32(0);   // Custom Load Image Transparent
            ZlibWriter.WriteInt32(0);   // Custom Load Image Alpha
            ZlibWriter.WriteInt32(1);   // Scale Progress Bar

            // Icon
            var iconWriter = new ByteWriter(new MemoryStream());
            iconWriter.WriteInt32(0); // Reserved
            iconWriter.WriteInt32(0); // Type
            iconWriter.WriteInt32(1); // Icon Count

            Bitmap icon = gameData.Icon;
            iconWriter.WriteInt8((byte)icon.Width);  // Icon Width
            iconWriter.WriteInt8((byte)icon.Height); // Icon Height
            iconWriter.WriteInt8(255);               // Color Count

            MemoryStream iconWrite = new MemoryStream();
            icon.Save(iconWrite, System.Drawing.Imaging.ImageFormat.Jpeg); // Reading Icon

            iconWriter.WriteInt8(0);  // Reserved
            iconWriter.WriteInt16(1); // Planes
            iconWriter.WriteInt16(3); // Bits per Pixel
            iconWriter.WriteInt32((int)iconWrite.Length); // Size
            iconWriter.WriteInt32(0); // Offset

            // Writing Icon
            iconWriter.WriteBytes(iconWrite.GetBuffer());
            iconWrite.Dispose();

            // Writing Icon Writer to Settings Writer
            ZlibWriter.WriteInt32((int)iconWriter.Size());
            ZlibWriter.WriteBytes(iconWriter.GetBuffer());

            // Back to Settings
            ZlibWriter.WriteInt32(1); // Display Errors
            ZlibWriter.WriteInt32(0); // Log Errors
            ZlibWriter.WriteInt32(0); // Abort on Error
            ZlibWriter.WriteInt32(0); // Treat Un-init as 0

            // Project Information
            ZlibWriter.WriteUnicode(gameData.author); // Author
            ZlibWriter.WriteUnicode("100");           // Version

            ZlibWriter.WriteDouble(DateTime.UtcNow.Millisecond / 86400000); // Date Modified

            ZlibWriter.WriteUnicode("Decompiled using CTFAK 2.0 and ZelTranslator"); // Information

            // Application Information
            ZlibWriter.WriteInt32(1); // Version Major
            ZlibWriter.WriteInt32(0); // Version Minor
            ZlibWriter.WriteInt32(0); // Version Release
            ZlibWriter.WriteInt32(0); // Version Build

            ZlibWriter.WriteUnicode(""); // Company
            ZlibWriter.WriteUnicode(""); // Product
            ZlibWriter.WriteUnicode(""); // Copyright
            ZlibWriter.WriteUnicode(""); // Description

            ZlibWriter.WriteDouble(DateTime.UtcNow.Millisecond / 86400000); // Date Modified
        }
    }
}
