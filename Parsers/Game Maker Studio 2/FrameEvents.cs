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
using CTFAK.MFA.MFAObjectLoaders;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class FrameEvents
    {
        public static void Add(List<GMLFile> gmlFiles, List<ProjectYYP.Resource> projResources, List<ObjectYY.RootObject> objects, GameData gameData, List<RoomYY.RootObject> rooms)
        {
            
            int frameCount = 0;
            foreach (var frame in gameData.frames)
            {
                // Add the built-in Fusion objects (i.e. Timer, Storyboard Controls, etc.)
                //string NewObjName = "";
                List<int> CTF_objectIDs = new List<int>();
                /*
                for (var i = 0; i < 10; i++)
                {
                    switch (i)
                    {
                        case 0:
                            NewObjName = "Special";
                            break;
                        case 1:
                            NewObjName = "Sound";
                            break;
                        case 2:
                            NewObjName = "Storyboard";
                            break;
                        case 3:
                            NewObjName = "Timer";
                            break;
                        case 4:
                            NewObjName = "Create";
                            break;
                        case 5:
                            NewObjName = "KeyboardMouse";
                            break;
                        case 6:
                            NewObjName = "Player1";
                            break;
                        case 7:
                            NewObjName = "Player2";
                            break;
                        case 8:
                            NewObjName = "Player3";
                            break;
                        case 9:
                            NewObjName = "Player4";
                            break;


                    }
                */
                var newObj = new ObjectYY.RootObject();
                newObj.name = $"CTF_Events_{frameCount}";
                newObj.visible = false;
                objects.Add(newObj);
                CTF_objectIDs.Add(projResources.Count); // Save object ID

                var newObjectRes = new ProjectYYP.Resource();
                var newObjectResID = new ProjectYYP.ResourceID();

                newObjectResID.name = newObj.name;
                newObjectResID.path = $"objects/{newObj.name}/{newObj.name}.yy";
                newObjectRes.id = newObjectResID;
                newObjectRes.order = GMS2Writer.ObjectOrder;
                GMS2Writer.ObjectOrder++;
                projResources.Add(newObjectRes);
                    



                List<RoomYY.Instance> instancesList = rooms[frameCount].layers[0].instances.ToList<RoomYY.Instance>();
                List<RoomYY.InstanceCreationOrder> instancesOrderList = rooms[frameCount].instanceCreationOrder.ToList<RoomYY.InstanceCreationOrder>();

                var newInstance = new RoomYY.Instance();
                newInstance.name = $"inst_{GMS2Writer.NewInstanceID()}";
                newInstance.x = 0;
                newInstance.y = -64;
                newInstance.colour = ((long)0 * 16777216) + 16777215;

                var objectID = new RoomYY.ObjectID();
                objectID.name = newObj.name;
                objectID.path = newObjectResID.path;
                newInstance.objectId = objectID;
                instancesList.Add(newInstance);

                var instanceCreation = new RoomYY.InstanceCreationOrder();
                instanceCreation.name = newInstance.name;
                instanceCreation.path = $"rooms/{rooms[frameCount].name}/{rooms[frameCount].name}.yy";
                instancesOrderList.Add(instanceCreation);


                rooms[frameCount].layers[0].instances = instancesList.ToArray();
                rooms[frameCount].instanceCreationOrder = instancesOrderList.ToArray();

                foreach (var evnt in frame.events.Items)
                {
                    foreach (var cond in evnt.Conditions)
                    {
                        ObjectYY.Event newCond = null;
                        switch (cond.ObjectType)
                        {
                            case -4: // Timer
                                {
                                    bool alreadyDone = false;
                                    if (!alreadyDone)
                                    {
                                        // Initialize new create event 
                                        newCond = new ObjectYY.Event();
                                        newCond.eventNum = 0;
                                        newCond.eventType = 0;
                                        GMLFile createGML = null;
                                        createGML.name = "Step_0";
                                        createGML.path = $"objects\\{objects[CTF_objectIDs[3]].name}";
                                        createGML.code = $"timer = 0;";
                                        gmlFiles.Add(createGML);
                                    }
                                    switch (cond.Num)
                                    {
                                        case -8: // Every XX"-XX
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                    
                                }

                        }
                    }
                }



                frameCount++;
            }
            
        }
    }
}