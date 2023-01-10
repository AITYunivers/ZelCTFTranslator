using CTFAK;
using CTFAK.CCN;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using Ionic.BZip2;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ZelCTFTranslator.Parsers.Game_Maker_Studio_2;
using ZelCTFTranslator.Parsers.Game_Maker_Studio_2.YYPs;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class GMS2Writer
    {
        public static int SpriteOrder = 0;
        public static int ObjectOrder = 0;
        public static int FrameOrder = 0;
        public static int FolderOrder = 1;

        public static string CleanString(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(str, "").Trim(' ');
        }

        public static char RandomChar()
        {
            char newChar = (char)50;
            int type = RandomNumberGenerator.GetInt32(0, 2);
            switch (type)
            {
                case 0:
                    newChar = (char)RandomNumberGenerator.GetInt32(48, 57);
                    break;
                case 1:
                    newChar = (char)RandomNumberGenerator.GetInt32(65, 90);
                    break;
                case 2:
                    newChar = (char)RandomNumberGenerator.GetInt32(97, 122);
                    break;
            }
            return newChar;
        }

        public static void Write(GameData gameData)
        {
            var outName = gameData.name ?? "Unknown Game";
            outName = CleanString(outName);
            var outPath = $"Dumps\\{outName}\\GMS2";
            Directory.CreateDirectory(outPath);

            var ProjectJSON = new ProjectYYP.RootObject();
            ProjectJSON.name = gameData.name;

            // Object ID Generators 
            foreach (var obj in gameData.frameitems.Values)
            {
                var str = "";
                for (int i = 0; i < 8; i++)
                    str += RandomChar();

                GMS2ObjectIDs.ObjectIDs.Add(obj.handle, str);
            }

            // Options
            var Options = new List<ProjectYYP.Option>();

            var OptionMain = new ProjectYYP.Option();
            OptionMain.name = "Main";
            OptionMain.path = "options/main/options_main.yy";
            Options.Add(OptionMain);

            var OptionWindows = new ProjectYYP.Option();
            OptionWindows.name = "Windows";
            OptionWindows.path = "options/windows/options_windows.yy";
            Options.Add(OptionWindows);

            ProjectJSON.Options = Options.ToArray();
            Options.Clear();

            // Folders
            var Folders = new List<ProjectYYP.Folder>();

            var FolderSprites = new ProjectYYP.Folder();
            FolderSprites.name = "Sprites";
            FolderSprites.folderPath = "folders/Sprites.yy";
            FolderSprites.order = FolderOrder;
            FolderOrder++;
            Folders.Add(FolderSprites);

            var FolderSounds = new ProjectYYP.Folder();
            FolderSounds.name = "Sounds";
            FolderSounds.folderPath = "folders/Sounds.yy";
            FolderSounds.order = FolderOrder;
            FolderOrder++;
            Folders.Add(FolderSounds);

            var FolderScripts = new ProjectYYP.Folder();
            FolderScripts.name = "Scripts";
            FolderScripts.folderPath = "folders/Scripts.yy";
            FolderScripts.order = FolderOrder;
            FolderOrder++;
            Folders.Add(FolderScripts);

            var FolderObjects = new ProjectYYP.Folder();
            FolderObjects.name = "Objects";
            FolderObjects.folderPath = "folders/Objects.yy";
            FolderObjects.order = FolderOrder;
            FolderOrder++;
            Folders.Add(FolderObjects);

            var FolderRooms = new ProjectYYP.Folder();
            FolderRooms.name = "Rooms";
            FolderRooms.folderPath = "folders/Rooms.yy";
            FolderRooms.order = FolderOrder;
            FolderOrder++;
            Folders.Add(FolderRooms);

            ProjectJSON.Folders = Folders.ToArray();
            Folders.Clear();

            var Rooms = new List<RoomYY.RootObject>();
            var RoomOrderNodes = new List<ProjectYYP.RoomOrderNode>();
            var Resources = new List<ProjectYYP.Resource>();
            foreach (var Frame in gameData.frames)
            {
                var newRoom = new RoomYY.RootObject();
                newRoom.name = CleanString(Frame.name);

                foreach (var View in newRoom.views)
                {
                    View.wview = Frame.width;
                    View.hview = Frame.height;
                    View.wport = Frame.width;
                    View.hport = Frame.height;
                }

                newRoom.roomSettings.Width = Frame.width;
                newRoom.roomSettings.Height = Frame.height;

                var roomLayers = new List<RoomYY.Layer>();

                // Background Layer
                var bgLayer = new RoomYY.Layer();
                bgLayer.resourceType = "GMRBackgroundLayer";
                bgLayer.name = "Background";
                bgLayer.visible = true;
                roomLayers.Add(bgLayer);

                // Instance Layers
                var instances = new List<RoomYY.InstanceCreationOrder>();
                int layer = 0;
                foreach (var Layer in Frame.layers.Items)
                {
                    layer++;
                    var newLayer = new RoomYY.Layer();
                    newLayer.resourceType = "GMRInstanceLayer";
                    newLayer.name = CleanString(Layer.Name);
                    newLayer.visible = Layer.Flags["Visible"];

                    var LayerInstances = new List<RoomYY.Instance>();
                    foreach (var LayerIntance in Frame.objects)
                    {
                        if (LayerIntance.layer == layer)
                        {
                            var instance = gameData.frameitems[LayerIntance.handle];

                            var newInstance = new RoomYY.Instance();
                            newInstance.name = $"inst_{GMS2ObjectIDs.ObjectIDs[instance.handle]}";
                            
                            var objectID = new RoomYY.ObjectID();
                            objectID.name = CleanString(instance.name);
                            objectID.path = $"objects/{objectID.name}/{objectID.name}.yy";
                            newInstance.objectId = objectID;
                            LayerInstances.Add(newInstance);

                            var instanceCreation = new RoomYY.InstanceCreationOrder();
                            instanceCreation.name = newInstance.name;
                            instanceCreation.path = $"rooms/{newRoom.name}/{newRoom.name}.yy";
                        }
                    }

                    newLayer.instances = LayerInstances.ToArray();
                    LayerInstances.Clear();

                    roomLayers.Add(newLayer);
                }

                Rooms.Add(newRoom);

                var newRoomNode = new ProjectYYP.RoomOrderNode();
                var newRoomNodeID = new ProjectYYP.RoomID();

                newRoomNodeID.name = newRoom.name;
                newRoomNodeID.path = $"rooms/{newRoom.name}/{newRoom.name}.yy";
                newRoomNode.roomId = newRoomNodeID;
                RoomOrderNodes.Add(newRoomNode);

                var newRoomRes = new ProjectYYP.Resource();
                var newRoomResID = new ProjectYYP.ResourceID();

                newRoomResID.name = newRoom.name;
                newRoomResID.path = $"rooms/{newRoom.name}/{newRoom.name}.yy";
                newRoomRes.id = newRoomResID;
                Resources.Add(newRoomRes);
            }

            ProjectJSON.RoomOrderNodes = RoomOrderNodes.ToArray();
            ProjectJSON.resources = Resources.ToArray();
            RoomOrderNodes.Clear();
            Resources.Clear();

            var WriteProjectJSON = JsonConvert.SerializeObject(ProjectJSON);
            File.WriteAllText($"{outPath}\\{outName}.yyp", WriteProjectJSON);
        }

        public static void WriteToFile  (ProjectYYP.RootObject ProjectJSON,
                                       List<RoomYY.RootObject> RoomJSONs)
        {

        }
    }
}
