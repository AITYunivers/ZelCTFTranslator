using CTFAK.CCN;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using static ZelCTFTranslator.Parsers.GDevelop.GDJSON;
using Action = CTFAK.CCN.Chunks.Frame.Action;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class GDActions
    {
        public static GDJSON.Action SetAltVal(Action action, GameData gameData, string Modify)
        {
            var Parameter1 = action.Items[0].Loader as AlterableValue;
            var Parameter2 = action.Items[1].Loader as ExpressionParameter;

            var newAction = new GDJSON.Action();
            newAction.type.value = "ModVarObjet";

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "AlterableValue" + GDWriter.AltCharacter(Parameter1.Value), //Alterable Value
                Modify, //Modifier
                ((int)(Parameter2.Items[0].Loader as LongExp).Value).ToString() //To Modify
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetXorY(Action action, GameData gameData, string XorY)
        {
            var Parameter1 = action.Items[0].Loader as ExpressionParameter;

            var newAction = new GDJSON.Action();
            newAction.type.value = "Mettre" + XorY;

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "=", //Modifier
                ((int)(Parameter1.Items[0].Loader as LongExp).Value).ToString() //To Modify
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetAnimation(Action action, GameData gameData)
        {
            var Parameter1 = action.Items[0].Loader as Short;

            var newAction = new GDJSON.Action();
            newAction.type.value = "SetAnimationName";

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                $"\"Animation {Parameter1.Value}\"" //Animation Name
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
            var Parameter1 = action.Items[0].Loader as Position;

            var newAction = new GDJSON.Action();
            newAction.type.value = "MettreXY";

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "=", //Modifier
                Parameter1.X.ToString(), //X Position
                "=", //Modifier
                Parameter1.Y.ToString(), //Y Position
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetOpacity(Action action, GameData gameData)
        {
            var Parameter1 = action.Items[0].Loader as ExpressionParameter;

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
                (1.0 - (((int)(Parameter1.Items[0].Loader as LongExp).Value) / 255.0)).ToString(), //Effect Parameter To Modify
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action PlaySoundChannel(Action action, GameData gameData, bool Loop)
        {
            var Parameter1 = action.Items[0].Loader as Sample;
            var Parameter2 = action.Items[1].Loader as ExpressionParameter;

            var newAction = new GDJSON.Action();
            newAction.type.value = "PlaySoundCanal";

            foreach (SoundItem sound in gameData.Sounds.Items)
                if (sound.Name == Parameter1.Name)
                {
                    var snddata = sound.Data;
                    var sndext = ".wav";
                    if (snddata[0] == 0xff || snddata[0] == 0x49)
                        sndext = ".mp3";
                    Parameter1.Name += sndext;
                    break;
                }

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var Parameters = new List<string>
            {
                "", //?
                Parameter1.Name, //Sound File
                ((int)(Parameter2.Items[0].Loader as LongExp).Value).ToString(), //Channel
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
            var Parameter1 = action.Items[0].Loader as ExpressionParameter;
            var Parameter2 = action.Items[1].Loader as ExpressionParameter;

            var newAction = new GDJSON.Action();

            newAction.type.value = "ModVolumeSoundCanal";

            var Parameters = new List<string>
            {
                "", //?
                ((int)(Parameter1.Items[0].Loader as LongExp).Value).ToString(), //Channel
                "=", //Modifier
                ((int)(Parameter2.Items[0].Loader as LongExp).Value).ToString() //Modify To
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action ToFrame(Action action, GameData gameData, int frameID = -1)
        {
            int toFrame = frameID;
            if (gameData.frames.Count - 1 > frameID)
                toFrame += 1;

            if (frameID == -1)
            {
                var Parameter1 = action.Items[0].Loader as Short;
                toFrame = Parameter1.Value;
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

        public static GDJSON.Action CreateAt(Action action, GameData gameData, int frameID)
        {
            var Parameter1 = action.Items[0].Loader as Create;

            var posx = Parameter1.Position.X.ToString();
            var posy = Parameter1.Position.Y.ToString();

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            if (Parameter1.Position.ObjectInfoParent != -1)
            {
                string ObjectName2 = $"Qualifier {Parameter1.Position.ObjectInfoParent}";
                if (Parameter1.Position.ObjectInfoParent <= gameData.frameitems.Count)
                    ObjectName2 = gameData.frameitems[Parameter1.Position.ObjectInfoParent].name;

                posx = ObjectName2 + ".X() + " + posx;
                posy = ObjectName2 + ".Y() + " + posy;
            }

            var newAction = new GDJSON.Action();
            newAction.type.value = "Create";

            var Parameters = new List<string>
            {
                "", //?
                ObjectName, //Object Name
                posx, //X Position
                posy, //Y Position
                $"\"{gameData.frames[frameID].layers.Items[Parameter1.Position.Layer].Name}\"", //Layer Name
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }

        public static GDJSON.Action SetCameraXorY(Action action, GameData gameData, string xory)
        {
            var Parameter1 = action.Items[0].Loader as ExpressionParameter;

            string ObjectName = $"Qualifier {action.ObjectInfo}";
            if (action.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[action.ObjectInfo].name;

            var newAction = new GDJSON.Action();
            newAction.type.value = "SetCameraCenter" + xory;

            var Parameters = new List<string>
            {
                "", //?
                "=", //Modifier
                ((int)(Parameter1.Items[0].Loader as LongExp).Value).ToString(), //Position
                "", //Layer Name
                "", //Camera ID
            };

            newAction.parameters = Parameters.ToArray();
            return newAction;
        }
    }
}
