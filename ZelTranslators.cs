using CTFAK;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.Tools;
using CTFAK.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using ZelCTFTranslator.Parsers.GameMaker;
using ZelCTFTranslator.Parsers.GDevelop;

namespace ZelCTFTranslator
{
    public class GDevelopTranslator : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "GDevelop Translator";

        public void Execute(IFileReader reader)
        {
            GDWriter.Write(reader.getGameData());
        }
    }

    public class GM8Translator : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "Game Maker 8 Translator";

        public void Execute(IFileReader reader)
        {
            GM8Writer.Write(reader.getGameData());
        }
    }

    public class GMS2Translator : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "Game Maker Studio 2 Translator";

        public void Execute(IFileReader reader)
        {
            GMS2Writer.Write(reader.getGameData());
        }
    }
}
