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
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ZelCTFTranslator.Parsers.Game_Maker_Studio_2;
using ZelCTFTranslator.Parsers.Game_Maker_Studio_2.YYPs;
using ZelCTFTranslator.Parsers.Game_Maker_Studio_2.YYs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using static ZelCTFTranslator.Parsers.Game_Maker_Studio_2.YYs.ObjectYY;
using System.Security.Cryptography.Xml;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class GMS2Writer
    {
        public static int SpriteOrder = 0;
        public static int ObjectOrder = 0;
        public static int FrameOrder = 0;
        public static int SoundOrder = 0;
        public static int FolderOrder = 1;

        public static string CleanString(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(str, "").Trim(' ');
        }
        public static string CleanStringFull(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            return rgx.Replace(str, "").Trim(' ');
        }

        public static char RandomChar()
        {
            char newChar = (char)50;
            int type = RandomNumberGenerator.GetInt32(0, 2);
            switch (type)
            {
                case 0: // Numbers
                    newChar = (char)RandomNumberGenerator.GetInt32(48, 58);
                    break;
                case 1: // Letters
                    newChar = (char)RandomNumberGenerator.GetInt32(97, 123);
                    break;
            }
            return newChar;
        }

        public static void Write(GameData gameData)
        {
            Logger.Log("\n\nGameMaker Studio 2 Translator\n\n");

            var outName = gameData.name ?? "Unknown Game";
            outName = CleanString(outName);
            var outPath = $"Dumps\\{outName}\\GMS2";

            var ProjectJSON = new ProjectYYP.RootObject();
            ProjectJSON.name = outName;

            // Object ID Generators 
            foreach (var obj in gameData.frameitems.Values)
            {
                if (GMS2ObjectIDs.ObjectIDs.Keys.Contains(obj.handle)) continue;
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
                Logger.Log("Loading Frame " + Frame.name);
                var newRoom = new RoomYY.RootObject();
                newRoom.name = $"rm{FrameOrder}_{CleanStringFull(Frame.name)}";

                var Views = new List<RoomYY.View>();
                for (int a = 0; a < 8; a++)
                {
                    var newView = new RoomYY.View();
                    newView.wview = Frame.width;
                    newView.hview = Frame.height;
                    newView.wport = Frame.width;
                    newView.hport = Frame.height;
                    Views.Add(newView);
                }
                newRoom.views = Views.ToArray();
                Views.Clear();

                newRoom.roomSettings.Width = Frame.width;
                newRoom.roomSettings.Height = Frame.height;

                var roomLayers = new List<RoomYY.Layer>();

                // Background Layer
                var bgLayer = new RoomYY.Layer();
                bgLayer.resourceType = "GMRBackgroundLayer";
                bgLayer.name = "Background";
                bgLayer.visible = true;
                bgLayer.colour = ToUIntColour(Frame.background); // Convert Frame BG color to 32-bit unsigned int for GMS
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
                    foreach (var LayerInstance in Frame.objects)
                    {
                        if (LayerInstance.layer == layer - 1)
                        {
                            var instance = gameData.frameitems[LayerInstance.objectInfo];

                            var newInstance = new RoomYY.Instance();
                            newInstance.name = $"inst_{NewInstanceID().ToUpper()}"; // EVERY instance in GMS2 has to have a unique ID, even if it represents the same object
                            newInstance.x = LayerInstance.x;
                            newInstance.y = LayerInstance.y;
                            newInstance.colour = ((long)instance.blend * 16777216) + 16777215;
                            // Quick Backdrop instances
                            if (instance.properties is Quickbackdrop quickbackdrop)
                            {
                                newInstance.scaleX = (float)Decimal.Divide(quickbackdrop.Width, gameData.Images.Items[quickbackdrop.Shape.Image].width);
                                newInstance.scaleY = (float)Decimal.Divide(quickbackdrop.Height, gameData.Images.Items[quickbackdrop.Shape.Image].height);
                                if (quickbackdrop.Shape.FillType == 1)
                                {
                                    newInstance.colour = ToUIntColour(quickbackdrop.Shape.Color1);
                                }
                            }

                            var objectID = new RoomYY.ObjectID();
                            objectID.name = CleanString(instance.name).Replace(" ", "_") + "_" + instance.handle;
                            objectID.path = $"objects/{objectID.name}/{objectID.name}.yy";
                            newInstance.objectId = objectID;
                            LayerInstances.Add(newInstance);

                            var instanceCreation = new RoomYY.InstanceCreationOrder();
                            instanceCreation.name = newInstance.name;
                            instanceCreation.path = $"rooms/{newRoom.name}/{newRoom.name}.yy";
                            instances.Add(instanceCreation);
                        }
                    }

                    newLayer.instances = LayerInstances.ToArray();
                    LayerInstances.Clear();

                    roomLayers.Add(newLayer);
                }

                var newRoomLayers = new List<RoomYY.Layer>();

                for (int ly = 1; ly <= roomLayers.Count; ly++)
                {
                    var roomLayer = roomLayers[roomLayers.Count - ly];
                    newRoomLayers.Add(roomLayer);
                }

                newRoom.instanceCreationOrder = instances.ToArray();
                newRoom.layers = newRoomLayers.ToArray();
                instances.Clear();
                roomLayers.Clear();

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
                newRoomRes.order = FrameOrder;
                FrameOrder++;
                Resources.Add(newRoomRes);
            }

            var Sprites = new List<SpriteYY.RootObject>();
            foreach (var obj in gameData.frameitems.Values)
            {
                // Counters sprites
                if (obj.properties is ObjectCommon commonCntr)
                {
                    if (commonCntr.Identifier == "CNTR" || commonCntr.Identifier == "CN" || !Settings.TwoFivePlus && commonCntr.Parent.ObjectType == 7)
                    {
                        Logger.Log($"Counter found: {obj.name}");
                        var counter = commonCntr.Counters;

                        var newSprite = new SpriteYY.RootObject();
                        var imgs = counter.Frames;
                        var baseimg = gameData.Images.Items[imgs[0]];
                        newSprite.name = $"CounterSprite_{imgs[0]}";

                        newSprite.bbox_right = baseimg.width - 1;
                        newSprite.bbox_bottom = baseimg.height - 1;
                        newSprite.width = baseimg.width;
                        newSprite.height = baseimg.height;

                        newSprite.frames = new SpriteYY.Frame[imgs.Count];

                        for (int i = 0; i < imgs.Count; i++)
                        {
                            newSprite.frames[i] = new SpriteYY.Frame();
                            newSprite.frames[i].name = Guid.NewGuid().ToString();
                            newSprite.frames[i].ctfhandle = imgs[i];
                        }

                        var newSequence = new SpriteYY.Sequence();
                        newSequence.name = $"CounterSprite_{obj.handle}";
                        newSequence.playbackSpeed = 0;
                        newSequence.length = imgs.Count;
                        newSequence.backdropWidth = gameData.header.WindowWidth;
                        newSequence.backdropHeight = gameData.header.WindowHeight;
                        newSequence.xorigin = baseimg.width;
                        newSequence.yorigin = baseimg.height;

                        var seqFrames = new List<SpriteYY.KeyFrame>();
                        int fi = 0;
                        foreach (var frame in newSprite.frames)
                        {
                            var newKeyFrame = new SpriteYY.KeyFrame();
                            newKeyFrame.id = Guid.NewGuid().ToString();
                            newKeyFrame.Key = fi;
                            newKeyFrame.Channels.ZEROREPLACE.Id.name = frame.name;
                            newKeyFrame.Channels.ZEROREPLACE.Id.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                            seqFrames.Add(newKeyFrame);
                            fi++;
                        }
                        newSequence.tracks[0].keyframes.Keyframes = seqFrames.ToArray();

                        newSprite.sequence = newSequence;
                        newSprite.layers[0].name = Guid.NewGuid().ToString();
                        Sprites.Add(newSprite);

                        var newSpriteRes = new ProjectYYP.Resource();
                        var newSpriteResID = new ProjectYYP.ResourceID();

                        newSpriteResID.name = newSprite.name;
                        newSpriteResID.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                        newSpriteRes.id = newSpriteResID;
                        newSpriteRes.order = SpriteOrder;
                        SpriteOrder++;
                        Resources.Add(newSpriteRes);
                    }
                }

                // Actives/Common Objs sprites
                if (obj.properties is ObjectCommon common)
                {
                    if (common.Animations == null ||
                        common.Animations.AnimationDict == null ||
                        common.Animations.AnimationDict.Count == 0)
                        continue;
                    for (int ad = 0; ad < common.Animations.AnimationDict.Count; ad++)
                    {
                        if (common.Animations.AnimationDict[ad].DirectionDict == null ||
                            common.Animations.AnimationDict[ad].DirectionDict.Count == 0)
                            continue;
                        for (int dd = 0; dd < common.Animations.AnimationDict[ad].DirectionDict.Count; dd++)
                        {
                            if (common.Animations.AnimationDict[ad].DirectionDict[dd].Frames.Count == 0) continue;
                            var newSprite = new SpriteYY.RootObject();
                            var imgs = common.Animations.AnimationDict[ad].DirectionDict[dd].Frames;
                            var baseimg = gameData.Images.Items[imgs[0]];
                            newSprite.name = $"Sprite {imgs[0]} {ad}_{dd}";

                            newSprite.bbox_right = baseimg.width - 1;
                            newSprite.bbox_bottom = baseimg.height - 1;
                            newSprite.width = baseimg.width;
                            newSprite.height = baseimg.height;

                            newSprite.frames = new SpriteYY.Frame[imgs.Count];

                            for (int i = 0; i < imgs.Count; i++)
                            {
                                newSprite.frames[i] = new SpriteYY.Frame();
                                newSprite.frames[i].name = Guid.NewGuid().ToString();
                                newSprite.frames[i].ctfhandle = imgs[i];
                            }

                            var newSequence = new SpriteYY.Sequence();
                            newSequence.name = $"Sprite {obj.handle} {ad}_{dd}";
                            newSequence.playbackSpeed = common.Animations.AnimationDict[ad].DirectionDict[dd].MinSpeed;
                            newSequence.playbackSpeed = newSequence.playbackSpeed / 100.0f * 60.0f;
                            newSequence.length = imgs.Count;
                            newSequence.backdropWidth = gameData.header.WindowWidth;
                            newSequence.backdropHeight = gameData.header.WindowHeight;
                            newSequence.xorigin = gameData.Images.Items[imgs[0]].HotspotX;
                            newSequence.yorigin = gameData.Images.Items[imgs[0]].HotspotY;

                            var seqFrames = new List<SpriteYY.KeyFrame>();
                            int fi = 0;
                            foreach (var frame in newSprite.frames)
                            {
                                var newKeyFrame = new SpriteYY.KeyFrame();
                                newKeyFrame.id = Guid.NewGuid().ToString();
                                newKeyFrame.Key = fi;
                                newKeyFrame.Channels.ZEROREPLACE.Id.name = frame.name;
                                newKeyFrame.Channels.ZEROREPLACE.Id.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                                seqFrames.Add(newKeyFrame);
                                fi++;
                            }
                            newSequence.tracks[0].keyframes.Keyframes = seqFrames.ToArray();

                            newSprite.sequence = newSequence;
                            newSprite.layers[0].name = Guid.NewGuid().ToString();
                            Sprites.Add(newSprite);

                            var newSpriteRes = new ProjectYYP.Resource();
                            var newSpriteResID = new ProjectYYP.ResourceID();

                            newSpriteResID.name = newSprite.name;
                            newSpriteResID.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                            newSpriteRes.id = newSpriteResID;
                            newSpriteRes.order = SpriteOrder;
                            SpriteOrder++;
                            Resources.Add(newSpriteRes);
                        }
                    }
                }

                // Backdrops sprites (come back to this)
                if (obj.properties is Backdrop backdrop)
                {
                    var newSprite = new SpriteYY.RootObject();
                    var imgs = new List<int>() { backdrop.Image };
                    var baseimg = gameData.Images.Items[imgs[0]];
                    newSprite.name = $"Backdrop_{imgs[0]}";

                    newSprite.bbox_right = baseimg.width - 1;
                    newSprite.bbox_bottom = baseimg.height - 1;
                    newSprite.width = baseimg.width;
                    newSprite.height = baseimg.height;

                    newSprite.frames = new SpriteYY.Frame[imgs.Count];

                    for (int i = 0; i < imgs.Count; i++)
                    {
                        newSprite.frames[i] = new SpriteYY.Frame();
                        newSprite.frames[i].name = Guid.NewGuid().ToString();
                        newSprite.frames[i].ctfhandle = imgs[i];
                    }

                    var newSequence = new SpriteYY.Sequence();
                    newSequence.name = $"Backdrop-{obj.handle}";
                    newSequence.playbackSpeed = 0;
                    newSequence.length = 1;
                    newSequence.backdropWidth = gameData.header.WindowWidth;
                    newSequence.backdropHeight = gameData.header.WindowHeight;
                    newSequence.xorigin = 0;
                    newSequence.yorigin = 0;

                    var seqFrames = new List<SpriteYY.KeyFrame>();
                    int fi = 0;
                    foreach (var frame in newSprite.frames)
                    {
                        var newKeyFrame = new SpriteYY.KeyFrame();
                        newKeyFrame.id = Guid.NewGuid().ToString();
                        newKeyFrame.Key = fi;
                        newKeyFrame.Channels.ZEROREPLACE.Id.name = frame.name;
                        newKeyFrame.Channels.ZEROREPLACE.Id.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                        seqFrames.Add(newKeyFrame);
                        fi++;
                    }
                    newSequence.tracks[0].keyframes.Keyframes = seqFrames.ToArray();

                    newSprite.sequence = newSequence;
                    newSprite.layers[0].name = Guid.NewGuid().ToString();
                    Sprites.Add(newSprite);

                    var newSpriteRes = new ProjectYYP.Resource();
                    var newSpriteResID = new ProjectYYP.ResourceID();

                    newSpriteResID.name = newSprite.name;
                    newSpriteResID.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                    newSpriteRes.id = newSpriteResID;
                    newSpriteRes.order = SpriteOrder;
                    SpriteOrder++;
                    Resources.Add(newSpriteRes);
                }
                // Quick Backdrop sprites (functions fine, but needs a rework)
                if (obj.properties is Quickbackdrop quickbackdrop)
                {
                    Logger.Log($"QUICK BACKDROP Found - ShapeType: {quickbackdrop.Shape.ShapeType} | FillType: {quickbackdrop.Shape.FillType}");

                    if (quickbackdrop.Shape.FillType == 3)
                    {
                        var newSprite = new SpriteYY.RootObject();
                        var imgs = new List<int>() { quickbackdrop.Shape.Image };
                        var baseimg = gameData.Images.Items[imgs[0]];  

                        newSprite.name = $"QuickBackdrop_{imgs[0]}";
                        newSprite.bbox_right = baseimg.width - 1;
                        newSprite.bbox_bottom = baseimg.height - 1;
                        newSprite.width = baseimg.width;
                        newSprite.height = baseimg.height;

                        newSprite.frames = new SpriteYY.Frame[imgs.Count];

                        newSprite.nineSlice.enabled = true;
                        newSprite.nineSlice.tileMode = new int[5] { 0, 0, 0, 0, 1 };

                        for (int i = 0; i < imgs.Count; i++)
                        {
                            newSprite.frames[i] = new SpriteYY.Frame();
                            newSprite.frames[i].name = Guid.NewGuid().ToString();
                            newSprite.frames[i].ctfhandle = imgs[i];
                        }

                        var newSequence = new SpriteYY.Sequence();
                        newSequence.name = $"QuickBackdrop-{obj.handle}";
                        newSequence.playbackSpeed = 0;
                        newSequence.length = 1;
                        newSequence.backdropWidth = gameData.header.WindowWidth;
                        newSequence.backdropHeight = gameData.header.WindowHeight;
                        newSequence.xorigin = 0;
                        newSequence.yorigin = 0;

                        var seqFrames = new List<SpriteYY.KeyFrame>();
                        int fi = 0;
                        foreach (var frame in newSprite.frames)
                        {
                            var newKeyFrame = new SpriteYY.KeyFrame();
                            newKeyFrame.id = Guid.NewGuid().ToString();
                            newKeyFrame.Key = fi;
                            newKeyFrame.Channels.ZEROREPLACE.Id.name = frame.name;
                            newKeyFrame.Channels.ZEROREPLACE.Id.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                            seqFrames.Add(newKeyFrame);
                            fi++;
                        }
                        newSequence.tracks[0].keyframes.Keyframes = seqFrames.ToArray();

                        newSprite.sequence = newSequence;
                        newSprite.layers[0].name = Guid.NewGuid().ToString();
                        Sprites.Add(newSprite);

                        var newSpriteRes = new ProjectYYP.Resource();
                        var newSpriteResID = new ProjectYYP.ResourceID();

                        newSpriteResID.name = newSprite.name;
                        newSpriteResID.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                        newSpriteRes.id = newSpriteResID;
                        newSpriteRes.order = SpriteOrder;
                        SpriteOrder++;
                        Resources.Add(newSpriteRes);
                    }
                    if (quickbackdrop.Shape.FillType == 1)
                    {
                        var newSprite = new SpriteYY.RootObject();
                        var imgs = new List<int>() { quickbackdrop.Shape.Image };
                        var baseimg = gameData.Images.Items[imgs[0]];

                        newSprite.name = $"QuickBackdrop_{imgs[0]}";
                        newSprite.bbox_right = baseimg.width - 1;
                        newSprite.bbox_bottom = baseimg.height - 1;
                        newSprite.width = baseimg.width;
                        newSprite.height = baseimg.height;

                        newSprite.frames = new SpriteYY.Frame[imgs.Count];

                        newSprite.nineSlice.enabled = true;
                        newSprite.nineSlice.tileMode = new int[5] { 0, 0, 0, 0, 1 };

                        for (int i = 0; i < imgs.Count; i++)
                        {
                            newSprite.frames[i] = new SpriteYY.Frame();
                            newSprite.frames[i].name = Guid.NewGuid().ToString();
                            newSprite.frames[i].ctfhandle = imgs[i];
                            newSprite.frames[i].FillType = 1;
                            newSprite.frames[i].solidColor = quickbackdrop.Shape.Color1;
                        }

                        var newSequence = new SpriteYY.Sequence();
                        newSequence.name = $"QuickBackdrop-{obj.handle}";
                        newSequence.playbackSpeed = 0;
                        newSequence.length = 1;
                        newSequence.backdropWidth = gameData.header.WindowWidth;
                        newSequence.backdropHeight = gameData.header.WindowHeight;
                        newSequence.xorigin = 0;
                        newSequence.yorigin = 0;

                        var seqFrames = new List<SpriteYY.KeyFrame>();
                        int fi = 0;
                        foreach (var frame in newSprite.frames)
                        {
                            var newKeyFrame = new SpriteYY.KeyFrame();
                            newKeyFrame.id = Guid.NewGuid().ToString();
                            newKeyFrame.Key = fi;
                            newKeyFrame.Channels.ZEROREPLACE.Id.name = frame.name;
                            newKeyFrame.Channels.ZEROREPLACE.Id.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                            seqFrames.Add(newKeyFrame);
                            fi++;
                        }
                        newSequence.tracks[0].keyframes.Keyframes = seqFrames.ToArray();

                        newSprite.sequence = newSequence;
                        newSprite.layers[0].name = Guid.NewGuid().ToString();
                        Sprites.Add(newSprite);

                        var newSpriteRes = new ProjectYYP.Resource();
                        var newSpriteResID = new ProjectYYP.ResourceID();

                        newSpriteResID.name = newSprite.name;
                        newSpriteResID.path = $"sprites/{newSprite.name}/{newSprite.name}.yy";
                        newSpriteRes.id = newSpriteResID;
                        newSpriteRes.order = SpriteOrder;
                        SpriteOrder++;
                        Resources.Add(newSpriteRes);
                    }
                }
            }


            var Objects = new List<ObjectYY.RootObject>();
            List<ObjectYY.GMLFile> GMLFiles = new List<ObjectYY.GMLFile>();
            foreach (var obj in gameData.frameitems.Values)
            {
                //Counters objects
                if (obj.properties is ObjectCommon commonCntr)
                {
                    
                    if (commonCntr.Identifier == "CNTR" || commonCntr.Identifier == "CN" || !Settings.TwoFivePlus && commonCntr.Parent.ObjectType == 7)
                    {
                        Logger.Log($"Counter Object: {commonCntr.Identifier}");
                        var counters = commonCntr.Counters;
                        var counter = commonCntr.Counter;
                        var imgs = counters.Frames;

                        var newObj = new ObjectYY.RootObject();
                        newObj.name = CleanString(obj.name).Replace(" ", "_") + "_" + obj.handle;
                        newObj.visible = commonCntr.NewFlags["VisibleAtStart"];
                        newObj.spriteId.name = $"CounterSprite_{imgs[0]}";
                        newObj.spriteId.path = $"sprites/{newObj.spriteId.name}/{newObj.spriteId.name}.yy";

                        var events = new List<ObjectYY.Event>();

                        var createEv = new ObjectYY.Event();
                        createEv.eventNum = 0;
                        createEv.eventType = 0;
                        events.Add(createEv);

                        var createEvFile = new ObjectYY.GMLFile();
                        createEvFile.name = "Create_0";
                        createEvFile.path = $"objects\\{newObj.name}";
                        createEvFile.code = $"value = {counter.Initial};\nminval = {counter.Minimum};\nmaxval = {counter.Maximum};\nspriteFont = font_add_sprite_ext(COUNTERSPR, \"0123456789-+.e\", false, false);".Replace("COUNTERSPR", newObj.spriteId.name);
                        GMLFiles.Add(createEvFile);

                        var drawEv = new ObjectYY.Event();
                        drawEv.eventNum = 0;
                        drawEv.eventType = 8;
                        events.Add(drawEv);

                        var drawEvFile = new ObjectYY.GMLFile();
                        drawEvFile.name = "Draw_0";
                        drawEvFile.path = $"objects\\{newObj.name}";
                        drawEvFile.code = "draw_set_font(spriteFont);\ndraw_set_halign(fa_right);\ndraw_set_valign(fa_bottom);\ndraw_text(x, y, string(value));";
                        GMLFiles.Add(drawEvFile);

                        newObj.eventList = events.ToArray();

                        Objects.Add(newObj);

                        var newObjectRes = new ProjectYYP.Resource();
                        var newObjectResID = new ProjectYYP.ResourceID();

                        newObjectResID.name = newObj.name;
                        newObjectResID.path = $"objects/{newObj.name}/{newObj.name}.yy";
                        newObjectRes.id = newObjectResID;
                        newObjectRes.order = ObjectOrder;
                        ObjectOrder++;
                        Resources.Add(newObjectRes);
                        continue;
                    }
                }

                // Actives/Common Objs objects
                if (obj.properties is ObjectCommon common)
                {
                    Logger.Log($"Common Object: {common.Identifier}");
                    if (common.Animations == null ||
                        common.Animations.AnimationDict == null ||
                        common.Animations.AnimationDict.Count == 0 ||
                        common.Animations.AnimationDict[0].DirectionDict == null ||
                        common.Animations.AnimationDict[0].DirectionDict.Count == 0 ||
                        common.Animations.AnimationDict[0].DirectionDict[0].Frames.Count == 0 ||
                        (common.Identifier != "SPRI" && common.Identifier != "SP"))
                    { // VVVVV For any CommonObjects that are unimplemented (i.e. Physics - Engine) pretty much only to stop missing resource complaints from GMS2
                        var newCommonObj = new ObjectYY.RootObject();
                        newCommonObj.name = CleanString(obj.name).Replace(" ", "_") + "_" + obj.handle;
                        newCommonObj.visible = false;
                        Objects.Add(newCommonObj);

                        var newCommonObjectRes = new ProjectYYP.Resource();
                        var newCommonObjectResID = new ProjectYYP.ResourceID();

                        newCommonObjectResID.name = newCommonObj.name;
                        newCommonObjectResID.path = $"objects/{newCommonObj.name}/{newCommonObj.name}.yy";
                        newCommonObjectRes.id = newCommonObjectResID;
                        newCommonObjectRes.order = ObjectOrder;
                        ObjectOrder++;
                        Resources.Add(newCommonObjectRes);
                        continue;
                    }
                    var imgs = common.Animations.AnimationDict[0].DirectionDict[0].Frames;

                    var newObj = new ObjectYY.RootObject();
                    newObj.name = CleanString(obj.name).Replace(" ", "_") + "_" + obj.handle;
                    newObj.visible = common.NewFlags["VisibleAtStart"];
                    newObj.spriteId.name = $"Sprite {imgs[0]} 0_0";
                    newObj.spriteId.path = $"sprites/{newObj.spriteId.name}/{newObj.spriteId.name}.yy";
                    Objects.Add(newObj);

                    var newObjectRes = new ProjectYYP.Resource();
                    var newObjectResID = new ProjectYYP.ResourceID();

                    newObjectResID.name = newObj.name;
                    newObjectResID.path = $"objects/{newObj.name}/{newObj.name}.yy";
                    newObjectRes.id = newObjectResID;
                    newObjectRes.order = ObjectOrder;
                    ObjectOrder++;
                    Resources.Add(newObjectRes);
                    continue;
                }

                // Backdrops objects
                if (obj.properties is Backdrop backdrop)
                {

                    var imgs = new List<int>() { backdrop.Image };

                    var newObj = new ObjectYY.RootObject();
                    newObj.name = CleanString(obj.name).Replace(" ", "_") + "_" + obj.handle;
                    newObj.visible = true;
                    newObj.spriteId.name = $"Backdrop_{imgs[0]}";
                    newObj.spriteId.path = $"sprites/{newObj.spriteId.name}/{newObj.spriteId.name}.yy";

                    Objects.Add(newObj);

                    var newObjectRes = new ProjectYYP.Resource();
                    var newObjectResID = new ProjectYYP.ResourceID();

                    newObjectResID.name = newObj.name;
                    newObjectResID.path = $"objects/{newObj.name}/{newObj.name}.yy";
                    newObjectRes.id = newObjectResID;
                    newObjectRes.order = ObjectOrder;
                    ObjectOrder++;
                    Resources.Add(newObjectRes);
                    continue;
                }
                // Quick Backdrop objects
                if (obj.properties is Quickbackdrop quickbackdrop)
                {

                    var imgs = new List<int>() { quickbackdrop.Shape.Image };

                    var newObj = new ObjectYY.RootObject();
                    newObj.name = CleanString(obj.name).Replace(" ", "_") + "_" + obj.handle;
                    newObj.visible = true;
                    newObj.spriteId.name = $"QuickBackdrop_{imgs[0]}";
                    newObj.spriteId.path = $"sprites/{newObj.spriteId.name}/{newObj.spriteId.name}.yy";

                    if (quickbackdrop.Shape.FillType == 1)
                    {
                        var events = new List<ObjectYY.Event>();

                        var createEv = new ObjectYY.Event();
                        createEv.eventNum = 0;
                        createEv.eventType = 0;
                        events.Add(createEv);

                        var createEvFile = new ObjectYY.GMLFile();
                        createEvFile.name = "Create_0";
                        createEvFile.path = $"objects\\{newObj.name}";
                        createEvFile.code = $"qbdCol = make_colour_rgb({quickbackdrop.Shape.Color1.R}, {quickbackdrop.Shape.Color1.G}, {quickbackdrop.Shape.Color1.B});";
                        GMLFiles.Add(createEvFile);

                        var drawEv = new ObjectYY.Event();
                        drawEv.eventNum = 0;
                        drawEv.eventType = 8;
                        events.Add(drawEv);

                        var drawEvFile = new ObjectYY.GMLFile();
                        drawEvFile.name = "Draw_0";
                        drawEvFile.path = $"objects\\{newObj.name}";
                        drawEvFile.code = $"draw_rectangle_colour(x, y, x + {quickbackdrop.Width} - 1, y + {quickbackdrop.Height} - 1, qbdCol, qbdCol, qbdCol, qbdCol, false);";
                        GMLFiles.Add(drawEvFile);

                        newObj.eventList = events.ToArray();
                    }

                    Objects.Add(newObj);

                    var newObjectRes = new ProjectYYP.Resource();
                    var newObjectResID = new ProjectYYP.ResourceID();

                    newObjectResID.name = newObj.name;
                    newObjectResID.path = $"objects/{newObj.name}/{newObj.name}.yy";
                    newObjectRes.id = newObjectResID;
                    newObjectRes.order = ObjectOrder;
                    ObjectOrder++;
                    Resources.Add(newObjectRes);
                    continue;
                }
            }

            // Sounds (come back to this and rework)
            var Sounds = new List<SoundYY.RootObject>();
            foreach (var snd in gameData.Sounds.Items)
            {
                if (snd is SoundItem soundItem)
                {
                    Logger.Log($"Sound: {soundItem.Name}");

                    // Determine audio file type
                    var ext = ".wav";
                    if (soundItem.Data[0] == 0xff || soundItem.Data[0] == 0x49)
                        ext = ".mp3";
                    else if (soundItem.Data[0] == 0x4F)
                        ext = ".ogg";


                    var newSound = new SoundYY.RootObject();
                    newSound.name = "snd_" + CleanStringFull(soundItem.Name);
                    newSound.soundFile = newSound.name + ext;
                    
                    newSound.volume = 1.0f;

                    Sounds.Add(newSound);

                    var newSoundRes = new ProjectYYP.Resource();
                    var newSoundResID = new ProjectYYP.ResourceID();

                    newSoundResID.name = newSound.name;
                    newSoundResID.path = $"sounds/{newSound.name}/{newSound.name}.yy";
                    newSoundRes.id = newSoundResID;
                    newSoundRes.order = SoundOrder;
                    SoundOrder++;
                    Resources.Add(newSoundRes);
                }
            }

            Logger.Log("Writing Frame Events (unfinished)");
            try
            {
                FrameEvents.Add(GMLFiles, Resources, Objects, gameData, Rooms);
            }
            catch (Exception ex)
            {
                Logger.Log($"Problem trying to write Frame Events: {ex}");
            }

            ProjectJSON.AudioGroups = new ProjectYYP.AudioGroup[1];
            ProjectJSON.TextureGroups = new ProjectYYP.TextureGroup[1];
            ProjectJSON.AudioGroups[0] = new ProjectYYP.AudioGroup();
            ProjectJSON.TextureGroups[0] = new ProjectYYP.TextureGroup();

            ProjectJSON.RoomOrderNodes = RoomOrderNodes.ToArray();
            ProjectJSON.resources = Resources.ToArray();
            RoomOrderNodes.Clear();
            Resources.Clear();
            try
            {
                WriteToFile(outPath, outName, gameData, ProjectJSON, Rooms, Sprites, Objects, Sounds, GMLFiles);
            }
            catch (Exception ex)
            {
                Logger.Log($"Problem trying to write Project File: {ex}");
            }
        }

        public static void WriteToFile  (string outPath,
                                         string outName,
                                       GameData gameData,
                          ProjectYYP.RootObject ProjectJSON,
                        List<RoomYY.RootObject> RoomJSONs,
                      List<SpriteYY.RootObject> SpriteJSONs,
                      List<ObjectYY.RootObject> ObjectJSONs,
                      List<SoundYY.RootObject> SoundJSONs,
                      List<ObjectYY.GMLFile> gmlFiles)
        {
            if (Directory.Exists(outPath))
                Directory.Delete(outPath, true);

            Logger.Log("Writing YYP/Project File");
            var WriteProjectJSON = JsonConvert.SerializeObject(ProjectJSON);
            Directory.CreateDirectory(outPath);
            File.WriteAllText($"{outPath}\\{outName}.yyp", WriteProjectJSON);

            Task[] tasks = new Task[RoomJSONs.Count];
            int i = 0;
            Logger.Log("Writing rooms");
            foreach (var room in RoomJSONs)
            {
                var newTask = new Task(() =>
                {
                    var WriteRoomJSON = JsonConvert.SerializeObject(room);
                    Directory.CreateDirectory($"{outPath}\\rooms\\{room.name}");
                    File.WriteAllText($"{outPath}\\rooms\\{room.name}\\{room.name}.yy", WriteRoomJSON);
                });
                tasks[i] = newTask;
                newTask.Start();
                i++;
            }
            foreach (var item in tasks)
            {
                item.Wait();
            }

            i = 0;
            Logger.Log("Writing sprites (may take a while)");
            foreach (var spr in SpriteJSONs)
            {
                foreach (var frame in spr.frames)
                {
                RETRY_SAVE:
                    Directory.CreateDirectory($"{outPath}\\sprites\\{spr.name}\\layers\\{frame.name}");
                    try
                    {
                        gameData.Images.Items[frame.ctfhandle].bitmap.Save($"{outPath}\\sprites\\{spr.name}\\layers\\{frame.name}\\{spr.layers[0].name}.png");
                        gameData.Images.Items[frame.ctfhandle].bitmap.Save($"{outPath}\\sprites\\{spr.name}\\{frame.name}.png");

                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Problem saving image: {ex}");
                        goto RETRY_SAVE;
                    }
                }
            RETRY_SAVE_YY:
                try
                {
                    var WriteSpriteJSON = JsonConvert.SerializeObject(spr).Replace("ZEROREPLACE", "0");
                    File.WriteAllText($"{outPath}\\sprites\\{spr.name}\\{spr.name}.yy", WriteSpriteJSON);
                }
                catch
                {
                    goto RETRY_SAVE_YY;
                }
                i++;
            }

            // Write sounds
            tasks = new Task[SoundJSONs.Count];
            i = 0;
            Logger.Log("Writing sounds (may take a while)");
            foreach (var snd in SoundJSONs)
            {
                var newTask = new Task(() =>
                {
                RETRY_SAVE:
                    Directory.CreateDirectory($"{outPath}\\sounds\\{snd.name}");
                    try
                    {
                        byte[] WriteSoundData = new byte[0];
                        foreach (var item in gameData.Sounds.Items)
                        {
                            if (item.Name == snd.name.Substring(4, snd.name.Length - 4))
                            {
                                WriteSoundData = item.Data;
                            }
                        }
                        File.WriteAllBytes($"{outPath}\\sounds\\{snd.name}\\{snd.soundFile}", WriteSoundData);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Problem writing sound file: {ex}");
                        //goto RETRY_SAVE;
                    }
                RETRY_SAVE_YY:
                    try
                    {
                        var WriteSoundJSON = JsonConvert.SerializeObject(snd);
                        File.WriteAllText($"{outPath}\\sounds\\{snd.name}\\{snd.name}.yy", WriteSoundJSON);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Problem writing sound YY: {ex}");
                        //goto RETRY_SAVE_YY;
                    }
                });
                tasks[i] = newTask;
                newTask.Start();
                i++;
            }
            foreach (var item in tasks)
            {
                item.Wait();
            }

            // Write Objects
            tasks = new Task[ObjectJSONs.Count];
            i = 0;
            Logger.Log("Writing objects");
            foreach (var obj in ObjectJSONs)
            {
                var newTask = new Task(() =>
                {
                    var WriteObjJSON = JsonConvert.SerializeObject(obj);
                    Directory.CreateDirectory($"{outPath}\\objects\\{obj.name}");
                    File.WriteAllText($"{outPath}\\objects\\{obj.name}\\{obj.name}.yy", WriteObjJSON);
                });
                tasks[i] = newTask;
                newTask.Start();
                i++;
            }
            foreach (var item in tasks)
            {
                item.Wait();
            }
            
            tasks = new Task[gmlFiles.Count];
            i = 0;
            Logger.Log("Writing GML files");
            foreach (var obj in gmlFiles)
            {
                var newTask = new Task(() =>
                {
                    RETRY_SAVE_GML:
                    Directory.CreateDirectory($"{outPath}\\{obj.path}");
                    try
                    {
                        File.WriteAllText($"{outPath}\\{obj.path}\\{obj.name}.gml", obj.code);
                        Logger.Log($"Wrote GML to {outPath}\\{obj.path}\\{obj.name}.gml");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Problem writing GML: {ex}");
                        goto RETRY_SAVE_GML;
                    }
                    
                });
                tasks[i] = newTask;
                newTask.Start();
                i++;
            }
            foreach (var item in tasks)
            {
                item.Wait();
            }
            
        }

        public static uint ToUIntColour(Color colour)
        {
            uint UIntCol = (UInt32)255 << 24; // Alpha (no bg transparency in fusion)
            UIntCol += (UInt32)colour.B << 16;
            UIntCol += (UInt32)colour.G << 8;
            UIntCol += colour.R;
            return UIntCol;
        }

        public static string NewInstanceID()
        {
            var str = "";
            for (int i = 0; i < 8; i++)
                str += RandomChar();
            return str;
        }

    }
}
