using CTFAK.CCN;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.Utils;
using System.Collections.Generic;
using System.IO;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class GDActions
    {
        public static GDJSON.Action SetAltVal(Action action, GameData gameData, string Modify)
        {
            ByteWriter Parameter1 = new ByteWriter(new MemoryStream());
            action.Items[0].Write(Parameter1);
            ByteWriter Parameter2 = new ByteWriter(new MemoryStream());
            action.Items[1].Write(Parameter2);

            ByteReader reader = new ByteReader(Parameter1.GetBuffer());
            reader.Skip(4);
            short AlterableValue = reader.ReadInt16();

            reader = new ByteReader(Parameter2.GetBuffer());
            reader.Skip(12);
            int ToModify = reader.ReadInt32();

            var newAction = new GDJSON.Action();
            newAction.type.value = "ModVarObjet";

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "AlterableValue" + GDevelopTranslator.AltCharacter(AlterableValue), //Alterable Value
                Modify, //Modifier
                ToModify.ToString() //To Modify
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetXorY(Action action, GameData gameData, string XorY)
        {
            ByteWriter Parameter = new ByteWriter(new MemoryStream());
            action.Items[0].Write(Parameter);

            ByteReader reader = new ByteReader(Parameter.GetBuffer());
            reader.Skip(12);
            int ToModify = reader.ReadInt32();

            var newAction = new GDJSON.Action();
            newAction.type.value = "Mettre" + XorY;

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "=", //Modifier
                ToModify.ToString() //To Modify
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetAnimation(Action action, GameData gameData)
        {
            ByteWriter Parameter = new ByteWriter(new MemoryStream());
            action.Items[0].Write(Parameter);

            ByteReader reader = new ByteReader(Parameter.GetBuffer());
            reader.Skip(4);
            short Animation = reader.ReadInt16();

            var newAction = new GDJSON.Action();
            newAction.type.value = "SetAnimationName";

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                $"\"Animation {Animation}\"" //Animation Name
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action DefaultType(Action action, GameData gameData, bool Object, string Type)
        {
            var newAction = new GDJSON.Action();
            newAction.type.value = Type;

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>();
            if (Object) Parameters.Add(ObjectName);
            else Parameters.Add("");

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetPosition(Action action, GameData gameData)
        {
            ByteWriter Parameter = new ByteWriter(new MemoryStream());
            action.Items[0].Write(Parameter);

            ByteReader reader = new ByteReader(Parameter.GetBuffer());
            reader.Skip(8);
            short X = reader.ReadInt16();
            short Y = reader.ReadInt16();

            var newAction = new GDJSON.Action();
            newAction.type.value = "MettreXY";

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "=", //Modifier
                X.ToString(), //X Position
                "=", //Modifier
                Y.ToString(), //Y Position
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetOpacity(Action action, GameData gameData)
        {
            ByteWriter Parameter = new ByteWriter(new MemoryStream());
            action.Items[0].Write(Parameter);

            ByteReader reader = new ByteReader(Parameter.GetBuffer());
            reader.Skip(12);
            short Opacity = reader.ReadInt16();

            var newAction = new GDJSON.Action();
            newAction.type.value = "SetEffectDoubleParameter";

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "\"Alpha Blending Coefficient\"", //Effect Name
                "\"opacity\"", //Effect Parameter Name
                (1 - (Opacity / 255)).ToString(), //Effect Parameter To Modify
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action PlaySoundChannel(Action action, GameData gameData, bool Loop)
        {
            ByteWriter Parameter1 = new ByteWriter(new MemoryStream());
            action.Items[0].Write(Parameter1);
            ByteWriter Parameter2 = new ByteWriter(new MemoryStream());
            action.Items[1].Write(Parameter2);

            ByteReader reader = new ByteReader(Parameter1.GetBuffer());
            reader.Skip(8);
            string SoundFile = reader.ReadUniversal();

            reader = new ByteReader(Parameter2.GetBuffer());
            reader.Skip(12);
            int Channel = reader.ReadInt32();

            var newAction = new GDJSON.Action();
            newAction.type.value = "PlaySoundCanal";

            foreach (SoundItem sound in gameData.Sounds.Items)
                if (sound.Name == SoundFile)
                {
                    var snddata = sound.Data;
                    var sndext = ".wav";
                    if (snddata[0] == 0xff || snddata[0] == 0x49)
                        sndext = ".mp3";
                    SoundFile += sndext;
                    break;
                }

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                "", //?
                SoundFile, //Sound File
                Channel.ToString(), //Channel
            };

            if (Loop)
                Parameters.Add("yes");
            else
                Parameters.Add("no");

            Parameters.Add("100");
            Parameters.Add("1");

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetChannelVolume(Action action, GameData gameData)
        {
            ByteWriter Parameter1 = new ByteWriter(new MemoryStream());
            action.Items[0].Write(Parameter1);
            ByteWriter Parameter2 = new ByteWriter(new MemoryStream());
            action.Items[1].Write(Parameter2);

            ByteReader reader = new ByteReader(Parameter1.GetBuffer());
            reader.Skip(12);
            int Channel = reader.ReadInt32();

            reader = new ByteReader(Parameter2.GetBuffer());
            reader.Skip(12);
            int ModifyTo = reader.ReadInt32();

            var newAction = new GDJSON.Action();
            newAction.type.value = "ModVolumeSoundCanal";

            var Parameters = new List<string>
            {
                "", //?
                Channel.ToString(), //Channel
                "=", //Modifier
                ModifyTo.ToString() //Modify To
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action ToFrame(Action action, GameData gameData, int frameID = -1)
        {
            int toFrame = frameID + 1;
            if (frameID == -1)
            {
                ByteWriter Parameter1 = new ByteWriter(new MemoryStream());
                action.Items[0].Write(Parameter1);

                ByteReader reader = new ByteReader(Parameter1.GetBuffer());
                reader.Skip(4);
                toFrame = reader.ReadInt16();
            }

            var newAction = new GDJSON.Action();
            newAction.type.value = "Scene";

            var Parameters = new List<string>
            {
                "", //?
                $"\"{gameData.frames[toFrame].name}\"", //Scene Name
                "no" //Stop paused scenes
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }
    }
}
