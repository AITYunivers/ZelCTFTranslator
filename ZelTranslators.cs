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
using ZelCTFTranslator.Parsers.GDevelop;

namespace ZelCTFTranslator
{
    public class GDevelopTranslator : IFusionTool
    {
        public int[] Progress = new int[] { };
        int[] IFusionTool.Progress => Progress;
        public string Name => "GDevelop Translator";

        public static string AltCharacter(int index)
        {
            if (index >= 26)
                return ((char)('A' + ((index - index % 26) / 26 - 1))).ToString() + ((char)('A' + index % 26)).ToString();
            else
                return ((char)('A' + index)).ToString();
        }

        public void Execute(IFileReader reader)
        {
            var GameData = reader.getGameData();

            var outPath = GameData.name ?? "Unknown Game";
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            outPath = rgx.Replace(outPath, "").Trim(' ');
            outPath = $"Dumps\\{outPath}\\GDevelop";
            Directory.CreateDirectory(outPath);

            var JSONtoWrite = new GDJSON.Rootobject();
            JSONtoWrite.firstLayout = GameData.frames[0].name;

            var Properties = new GDJSON.Properties();
            Properties.packageName = $"com.CTFAK.{rgx.Replace(GameData.name, "").Replace(" ", "")}";
            Properties.name = GameData.name;
            Properties.author = GameData.author;
            Properties.windowWidth = GameData.header.WindowWidth;
            Properties.windowHeight = GameData.header.WindowHeight;
            Properties.maxFPS = GameData.header.FrameRate;
            Properties.verticalSync = GameData.header.NewFlags["VSync"];
            Properties.platforms = new GDJSON.Platform[1];
            Properties.platforms[0] = new();

            var Resources = new GDJSON.Resources();
            var ListResources = new List<GDJSON.Resource>();
            if (GameData.Images != null)
                foreach (Image img in GameData.Images.Items.Values)
                {
                    var bmp = img.bitmap;
                    bmp.Save($"{outPath}\\img{img.Handle}.png");

                    var res = new GDJSON.Resource();
                    res.alwaysLoaded = false;
                    res.file = $"img{img.Handle}.png";
                    res.kind = "image";
                    res.metadata = "";
                    res.name = $"img{img.Handle}.png";
                    res.smoothed = true;
                    res.userAdded = true;
                    ListResources.Add(res);
                }
            if (GameData.Sounds != null)
                foreach (SoundItem sound in GameData.Sounds.Items)
                {
                    var snddata = sound.Data;
                    var sndext = ".wav";
                    if (snddata[0] == 0xff || snddata[0] == 0x49)
                        sndext = ".mp3";
                    File.WriteAllBytes($"{outPath}\\{sound.Name}{sndext}", snddata);

                    var res = new GDJSON.Resource();
                    res.file = sound.Name + sndext;
                    res.kind = "audio";
                    res.metadata = "";
                    res.name = sound.Name + sndext;
                    res.preloadAsMusic = false;
                    res.preloadAsSound = true;
                    res.preloadInCache = false;
                    res.userAdded = true;
                    ListResources.Add(res);
                }
            Resources.resources = ListResources.ToArray();

            var Scenes = new List<GDJSON.Layout>();
            foreach (var frame in GameData.frames)
            {
                var newScene = new GDJSON.Layout();
                if (frame.name == "" || frame.name == null) continue;
                newScene.mangledName = rgx.Replace(frame.name, "").Replace(" ", "");
                newScene.name = frame.name;
                var sceneLayers = new List<GDJSON.Layer>();
                foreach (var layer in frame.layers.Items)
                {
                    var newLayer = new GDJSON.Layer();
                    newLayer.followBaseLayerCamera = layer.XCoeff != 0 && layer.YCoeff != 0;
                    newLayer.name = layer.Name;
                    newLayer.visibility = !layer.Flags["ToHide"];
                    newLayer.cameras = new GDJSON.Camera[1];
                    newLayer.cameras[0] = new();
                    sceneLayers.Add(newLayer);
                }
                var sceneObjects = new List<GDJSON.Object>();
                var sceneInstances = new List<GDJSON.Instance>();
                int z = 0;
                foreach (var obj in frame.objects)
                {
                    var newObj = new GDJSON.Object();
                    var objItem = GameData.frameitems[obj.objectInfo];
                    bool alreadyIn = false;
                    foreach (var oldObj in sceneObjects)
                        if (objItem.name == oldObj.name)
                            alreadyIn = true;
                    if (!alreadyIn)
                    {
                        if (objItem.properties is ObjectCommon objCommon)
                        {
                            //Actives
                            if (Settings.twofiveplus && objCommon.Identifier == "SPRI" || !Settings.twofiveplus && objCommon.Parent.ObjectType == 2)
                            {
                                if (objCommon.Animations.AnimationDict == null ||
                                    objCommon.Animations.AnimationDict[0].DirectionDict == null) continue;
                                newObj.name = objItem.name;
                                newObj.type = "Sprite";
                                var Animations = new List<GDJSON.Animation>();
                                foreach (var anim in objCommon.Animations.AnimationDict)
                                {
                                    if (anim.Value.DirectionDict == null) continue;
                                    var newAnim = new GDJSON.Animation();
                                    newAnim.name = $"Animation {anim.Key}";
                                    newAnim.useMultipleDirections = anim.Value.DirectionDict.Count <= 0;
                                    var Directions = new List<GDJSON.Direction>();
                                    foreach (var dir in anim.Value.DirectionDict.Values)
                                    {
                                        if (dir.Frames.Count == 0) continue;
                                        var newDir = new GDJSON.Direction();
                                        newDir.looping = dir.Repeat == -1;
                                        newDir.timeBetweenFrames = 1 / (60 * ((float)dir.MaxSpeed / 100));
                                        var Images = new List<GDJSON.Sprite>();
                                        foreach (var img in dir.Frames)
                                        {
                                            var newImg = new GDJSON.Sprite();
                                            newImg.image = $"img{img}.png";
                                            var newHotspot = new GDJSON.Originpoint();
                                            newHotspot.x = GameData.Images.Items[img].HotspotX;
                                            newHotspot.y = GameData.Images.Items[img].HotspotY;
                                            newImg.originPoint = newHotspot;
                                            Images.Add(newImg);
                                        }
                                        newDir.sprites = Images.ToArray();
                                        Directions.Add(newDir);
                                    }
                                    newAnim.directions = Directions.ToArray();
                                    Animations.Add(newAnim);
                                }

                                var Variables = new List<GDJSON.ObjectVariable>();
                                var AltFlags = new List<GDJSON.ObjectVariable>();
                                if (objCommon.Values != null)
                                {
                                    for (int j = 0; j < objCommon.Values.Items.Count; j++)
                                    {
                                        var newValue = new GDJSON.ObjectVariable();
                                        newValue.name = $"AlterableValue{AltCharacter(j)}";
                                        newValue.value = objCommon.Values.Items[j];
                                        newValue.type = "number";
                                        Variables.Add(newValue);
                                    }

                                    for (int j = 0; j < 32; j++)
                                    {
                                        var newValue = new GDJSON.ObjectVariable();
                                        newValue.name = $"AlterableFlag{AltCharacter(j)}";
                                        newValue.value = ByteFlag.GetFlag((uint)objCommon.Values.Flags, j);
                                        newValue.type = "boolean";
                                        AltFlags.Add(newValue);
                                    }

                                    for (int j = 31; j >= 0; j--)
                                        if (AltFlags[j].value is bool b && b == false)
                                            AltFlags.Remove(AltFlags[j]);
                                        else break;
                                }

                                if (objCommon.Strings != null)
                                {
                                    for (int j = 0; j < objCommon.Strings.Items.Count; j++)
                                    {
                                        var newValue = new GDJSON.ObjectVariable();
                                        newValue.name = $"AlterableString{AltCharacter(j)}";
                                        newValue.value = objCommon.Strings.Items[j];
                                        newValue.type = "string";
                                        Variables.Add(newValue);
                                    }
                                }

                                foreach (var value in AltFlags)
                                    Variables.Add(value);

                                var Effects = new List<GDJSON.ObjectEffect>();

                                var newAlpha = new GDJSON.ObjectEffect();
                                newAlpha.effectType = "BlendingMode";
                                newAlpha.name = "Alpha Blending Coefficient";
                                var newParameter = new GDJSON.Doubleparameters();
                                newParameter.blendmode = 0;
                                newParameter.opacity = 1 - objItem.blend / 255;
                                newAlpha.doubleParameters = newParameter;
                                newAlpha.booleanParameters = new();
                                newAlpha.stringParameters = new();
                                Effects.Add(newAlpha);

                                newObj.animations = Animations.ToArray();
                                newObj.variables = Variables.ToArray();
                                newObj.effects = Effects.ToArray();
                            }
                        }
                        //Backdrops
                        else if (objItem.properties is Backdrop objBackdrop)
                        {
                            newObj.name = objItem.name;
                            newObj.type = "Sprite";

                            newObj.animations = new GDJSON.Animation[1];
                            newObj.animations[0] = new GDJSON.Animation();
                            newObj.animations[0].directions = new GDJSON.Direction[1];
                            newObj.animations[0].directions[0] = new GDJSON.Direction();
                            newObj.animations[0].directions[0].sprites = new GDJSON.Sprite[1];
                            newObj.animations[0].directions[0].sprites[0] = new GDJSON.Sprite();
                            newObj.animations[0].directions[0].sprites[0].originPoint = new GDJSON.Originpoint();

                            newObj.animations[0].name = "Backdrop";
                            newObj.animations[0].useMultipleDirections = false;

                            newObj.animations[0].directions[0].looping = false;
                            newObj.animations[0].directions[0].timeBetweenFrames = 0.033f;

                            newObj.animations[0].directions[0].sprites[0].image = $"img{objBackdrop.Image}.png";
                            newObj.animations[0].directions[0].sprites[0].originPoint.x = 0;
                            newObj.animations[0].directions[0].sprites[0].originPoint.y = 0;
                        }
                        else continue;
                    }

                    var newInstance = new GDJSON.Instance();
                    newInstance.layer = frame.layers.Items[obj.layer].Name;
                    newInstance.name = objItem.name;
                    newInstance.x = obj.x;
                    newInstance.y = obj.y;
                    newInstance.zOrder = z;
                    z++;

                    sceneInstances.Add(newInstance);
                    sceneObjects.Add(newObj);
                }

                var Events = new List<GDJSON.FrameEvents>();
                foreach (var evnt in frame.events.Items)
                {
                    var newEvnt = new GDJSON.FrameEvents();
                    var Conditions = new List<GDJSON.Condition>();
                    foreach (var cond in evnt.Conditions)
                    {
                        GDJSON.Condition newCond = null;
                        if (CTFAKCore.parameters.Contains("-dumpevents"))
                        {
                            int p = 0;
                            foreach (var parameter in cond.Items)
                            {
                                var paramreader = new ByteWriter(new MemoryStream());
                                parameter.Write(paramreader);
                                File.WriteAllBytes($"Events\\Condition {cond.ObjectType} ~ {cond.Num} ~ Parameter{p}.bin", paramreader.GetBuffer());
                                p++;
                            }
                        }
                        switch (cond.ObjectType)
                        {
                            /* To Add:
                                Upon Pressing Key
                                Only One Action
                                Counter !=<>
                                Every Timer
                                Start of Frame
                                Animation is Over
                                Overlapping
                                Mouse Pointer Over
                                Compare X Position
                                User Clicks on Object
                                Compare Two General Values
                                Repeat while key is pressed
                                Animation is playing
                                Timer is greater than
                                Run this event once
                                Current Frame !=<>
                                Is Visible
                            */
                            case -1:
                                switch (cond.Num)
                                {
                                    case -1: //Always
                                        newCond = GDConditions.DefaultType(cond, GameData, false, false, "BuiltinCommonInstructions::Always");
                                        break;
                                    case -2: //Never
                                        newCond = GDConditions.DefaultType(cond, GameData, false, true, "BuiltinCommonInstructions::Always");
                                        break;
                                }
                                break;
                            case 2:
                                switch (cond.Num)
                                {
                                    case -27: //Compare to Alterable Value
                                        newCond = GDConditions.CompareAltVal(cond, GameData);
                                        break;
                                }
                                break;
                        }
                        if (newCond == null)
                            Logger.Log($"Unknown Condition: {cond.ObjectType} ~ {cond.Num}");

                        Conditions.Add(newCond);
                    }
                    var Actions = new List<GDJSON.Action>();
                    foreach (var action in evnt.Actions)
                    {
                        GDJSON.Action newAction = null;
                        if (CTFAKCore.parameters.Contains("-dumpevents"))
                        {
                            int p = 0;
                            foreach (var parameter in action.Items)
                            {
                                var paramreader = new ByteWriter(new MemoryStream());
                                parameter.Write(paramreader);
                                File.WriteAllBytes($"Events\\Action {action.ObjectType} ~ {action.Num} ~ Parameter{p}.bin", paramreader.GetBuffer());
                                p++;
                            }
                        }
                        switch (action.ObjectType)
                        {
                            /* To Add:
                                Create at Position
                                Create at Position From
                                Set/Add to/Subtract from; Counter
                                Center Display at X
                                Set Ini File
                                Set Ini Group
                                Set Ini Value
                            */
                            case -3:
                                switch (action.Num)
                                {
                                    case 0: //Next Frame
                                        newAction = GDActions.ToFrame(action, GameData, Scenes.Count);
                                        break;
                                    case 2: //Jump to Frame
                                        newAction = GDActions.ToFrame(action, GameData, -1);
                                        break;
                                    case 4: //End Application
                                        newAction = GDActions.DefaultType(action, GameData, false, "Quit");
                                        break;
                                }
                                break;
                            case -2:
                                switch (action.Num)
                                {
                                    case 1: //Stop Any Sample
                                        newAction = GDActions.DefaultType(action, GameData, false, "UnloadAllAudio");
                                        break;
                                    case 11: //Play Sound on Channel
                                        newAction = GDActions.PlaySoundChannel(action, GameData, false);
                                        break;
                                    case 12: //Loop Sound on Channel
                                        newAction = GDActions.PlaySoundChannel(action, GameData, true);
                                        break;
                                    case 17: //Set Volume of Channel
                                        newAction = GDActions.SetChannelVolume(action, GameData);
                                        break;
                                }
                                break;
                            case 2:
                                switch (action.Num)
                                {
                                    case 1: //Set Position
                                        newAction = GDActions.SetPosition(action, GameData);
                                        break;
                                    case 2: //Set X
                                        newAction = GDActions.SetXorY(action, GameData, "X");
                                        break;
                                    case 3: //Set Y
                                        newAction = GDActions.SetXorY(action, GameData, "Y");
                                        break;
                                    case 17: //Set Animation
                                        newAction = GDActions.SetAnimation(action, GameData);
                                        break;
                                    case 24: //Destroy
                                        newAction = GDActions.DefaultType(action, GameData, true, "Delete");
                                        break;
                                    case 26: //Show
                                        newAction = GDActions.DefaultType(action, GameData, true, "Cache");
                                        break;
                                    case 27: //Hide
                                        newAction = GDActions.DefaultType(action, GameData, true, "Montre");
                                        break;
                                    case 31: //Set Alterable Value
                                        newAction = GDActions.SetAltVal(action, GameData, "=");
                                        break;
                                    case 32: //Add to Alterable Value
                                        newAction = GDActions.SetAltVal(action, GameData, "+");
                                        break;
                                    case 33: //Subtract from Alterable Value
                                        newAction = GDActions.SetAltVal(action, GameData, "-");
                                        break;
                                    case 65: //Set Alpha Blending Coefficient
                                        newAction = GDActions.SetOpacity(action, GameData);
                                        break;
                                }
                                break;
                        }
                        if (newAction == null)
                            Logger.Log($"Unknown Action: {action.ObjectType} ~ {action.Num}");

                        Actions.Add(newAction);
                    }
                    if (Conditions.Count == 0 && Actions.Count == 0) continue;

                    newEvnt.conditions = Conditions.ToArray();
                    newEvnt.actions = Actions.ToArray();
                    Events.Add(newEvnt);
                }
                newScene.layers = sceneLayers.ToArray();
                newScene.objects = sceneObjects.ToArray();
                newScene.instances = sceneInstances.ToArray();
                newScene.events = Events.ToArray();
                Scenes.Add(newScene);
            }

            JSONtoWrite.properties = Properties;
            JSONtoWrite.resources = Resources;
            JSONtoWrite.layouts = Scenes.ToArray();

            var JSON = JsonConvert.SerializeObject(JSONtoWrite);
            File.WriteAllText($"{outPath}\\game.json", JSON);
        }
    }
}
