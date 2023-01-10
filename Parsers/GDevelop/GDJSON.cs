using System;
using System.Security.Cryptography;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class GDJSON
    {
        public class Rootobject
        {
            public string firstLayout { get; set; }
            public Gdversion gdVersion = new();
            public Properties properties { get; set; }
            public Resources resources { get; set; }
            public object[] objects { get; set; }
            public object[] objectsGroups { get; set; }
            public Variable[] variables { get; set; }
            public Layout[] layouts { get; set; }
            public object[] externalEvents { get; set; }
            public Eventsfunctionsextension[] eventsFunctionsExtensions { get; set; }
            public object[] externalLayouts { get; set; }
            public object[] externalSourceFiles { get; set; }
        }

        public class Gdversion
        {
            public int build = 99;
            public int major = 4;
            public int minor = 0;
            public int revision = 0;
        }

        public class Properties
        {
            public bool adaptGameResolutionAtRuntime = false;
            public bool folderProject = false;
            public string orientation = "landscape";
            public string packageName { get; set; }
            public bool pixelsRounding = false;
            public string projectUuid = "";
            public string scaleMode = "linear";
            public string sizeOnStartupMode = "adaptHeight";
            public string templateSlug = "";
            public bool useExternalSourceFiles = false;
            public string version = "1.0.0";
            public string name { get; set; }
            public string description = "Decompiled using CTFAK 2.0 and ZelTranslator";
            public string author { get; set; }
            public int windowWidth { get; set; }
            public int windowHeight { get; set; }
            public string latestCompilationDirectory = "";
            public int maxFPS { get; set; }
            public int minFPS = 20;
            public bool verticalSync { get; set; }
            public Platformspecificassets platformSpecificAssets = new();
            public Loadingscreen loadingScreen = new();
            public object[] authorIds = new object[0];
            public object[] categories = new object[0];
            public object[] playableDevices = new object[0];
            public object[] extensionProperties = new object[0];
            public Platform[] platforms { get; set; }
            public string currentPlatform = "GDevelop JS platform";
        }

        public class Platformspecificassets
        {
        }

        public class Loadingscreen
        {
            public int backgroundColor = 0;
            public float backgroundFadeInDuration = 0.2f;
            public string backgroundImageResourceName = "";
            public string gdevelopLogoStyle = "light";
            public float logoAndProgressFadeInDuration = 0.2f;
            public float logoAndProgressLogoFadeInDelay = 0.2f;
            public float minDuration = 1.5f;
            public int progressBarColor = 16777215;
            public int progressBarHeight = 20;
            public int progressBarMaxWidth = 200;
            public int progressBarMinWidth = 40;
            public int progressBarWidthPercent = 30;
            public bool showGDevelopSplash = false;
            public bool showProgressBar = false;
        }

        public class Platform
        {
            public string name = "GDevelop JS platform";
        }

        public class Resources
        {
            public Resource[] resources { get; set; }
            public object[] resourceFolders { get; set; }
        }

        public class Resource
        {
            public bool alwaysLoaded { get; set; }
            public string file { get; set; }
            public string kind { get; set; }
            public string metadata { get; set; }
            public string name { get; set; }
            public bool smoothed { get; set; }
            public bool userAdded { get; set; }
            public bool preloadAsMusic { get; set; }
            public bool preloadAsSound { get; set; }
            public bool preloadInCache { get; set; }
        }

        public class Variable
        {
            public bool folded { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int value { get; set; }
        }

        public class Layout
        {
            public int b = 0;
            public bool disableInputWhenNotFocused = true;
            public string mangledName { get; set; }
            public string name { get; set; }
            public int oglFOV = 90;
            public int oglZFar = 500;
            public int oglZNear = 1;
            public int r = 0;
            public bool standardSortMethod = true;
            public bool stopSoundsOnStartup = false;
            public string title = "";
            public int v = 0;
            public Uisettings uiSettings = new();
            public object[] objectsGroups { get; set; }
            public Variable1[] variables { get; set; }
            public Instance[] instances { get; set; }
            public Object[] objects { get; set; }
            public FrameEvents[] events { get; set; }
            public Layer[] layers { get; set; }
            public object[] behaviorsSharedData { get; set; }
        }

        public class Uisettings
        {
            public bool grid = false;
            public string gridType = "rectangular";
            public int gridWidth = 32;
            public int gridHeight = 32;
            public int gridOffsetX = 0;
            public int gridOffsetY = 0;
            public int gridColor = 10401023;
            public float gridAlpha = 0.8f;
            public bool snap = false;
            public float zoomFactor = 0.5f;
            public bool windowMask = false;
        }

        public class Variable1
        {
            public bool folded { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int value { get; set; }
        }

        public class Instance
        {
            public int angle = 0;
            public bool customSize = false;
            public int height = 0;
            public string layer { get; set; }
            public string name { get; set; }
            public string persistentUuid = $"{RandomNumberGenerator.GetInt32(10000000, 99999999)}-" +
                                           $"{RandomNumberGenerator.GetInt32(1000, 9999)}-" +
                                           $"{RandomNumberGenerator.GetInt32(1000, 9999)}-" +
                                           $"{RandomNumberGenerator.GetInt32(100000, 999999)}" +
                                           $"{RandomNumberGenerator.GetInt32(100000, 999999)}";
            public int width = 0;
            public int x { get; set; }
            public int y { get; set; }
            public int zOrder { get; set; }
            public object[] numberProperties = new object[0];
            public object[] stringProperties = new object[0];
            public object[] initialVariables = new object[0];
        }

        public class Object
        {
            public string assetStoreId = "";
            public string name { get; set; }
            public string tags = "";
            public string type { get; set; }
            public bool updateIfNotVisible = false;
            public ObjectVariable[] variables = new ObjectVariable[0];
            public ObjectEffect[] effects = new ObjectEffect[0];
            public object[] behaviors = new object[0];
            public Animation[] animations { get; set; }
            public bool bold { get; set; }
            public bool italic { get; set; }
            public bool smoothed { get; set; }
            public bool underlined { get; set; }
            public string _string { get; set; }
            public string font { get; set; }
            public int characterSize { get; set; }
            public Color color { get; set; }
        }

        public class Color
        {
            public int b { get; set; }
            public int g { get; set; }
            public int r { get; set; }
        }

        public class ObjectVariable
        {
            public bool folded = true;
            public string name { get; set; }
            public string type { get; set; }
            public object value { get; set; }
        }

        public class ObjectEffect
        {
            public string effectType { get; set; }
            public string name { get; set; }
            public Doubleparameters doubleParameters { get; set; }
            public Stringparameters stringParameters { get; set; }
            public Booleanparameters booleanParameters { get; set; }
        }

        public class Doubleparameters
        {
            public int blendmode { get; set; }
            public int opacity { get; set; }
        }

        public class Stringparameters
        {
        }

        public class Booleanparameters
        {
        }

        public class Animation
        {
            public string name { get; set; }
            public bool useMultipleDirections { get; set; }
            public Direction[] directions { get; set; }
        }

        public class Direction
        {
            public bool looping { get; set; }
            public float timeBetweenFrames { get; set; }
            public Sprite[] sprites { get; set; }
        }

        public class Sprite
        {
            public bool hasCustomCollisionMask = false;
            public string image { get; set; }
            public object[] points = new object[0];
            public Originpoint originPoint { get; set; }
            public Centerpoint centerPoint = new Centerpoint();
            public object[] customCollisionMask = new object[0];
        }

        public class Originpoint
        {
            public string name = "origine";
            public int x { get; set; }
            public int y { get; set; }
        }

        public class Centerpoint
        {
            public bool automatic = true;
            public string name = "centre";
            public int x = 0;
            public int y = 0;
        }

        public class FrameEvents
        {
            public string type = "BuiltinCommonInstructions::Standard";
            public Condition[] conditions { get; set; }
            public Action[] actions { get; set; }
            public bool folded = true;
            //public Event[] events { get; set; }
        }

        public class Condition
        {
            public ConditionType type = new ConditionType();
            public string[] parameters { get; set; }
            public Subinstruction[] subInstructions { get; set; }
        }

        public class ConditionType
        {
            public string value { get; set; }
            public bool inverted = false;
        }

        public class Subinstruction
        {
            public Type type { get; set; }
            public string[] parameters { get; set; }
        }

        public class Type
        {
            public string value { get; set; }
        }

        public class Action
        {
            public Type type = new();
            public string[] parameters { get; set; }
        }

        public class Event
        {
            public string type { get; set; }
            public Condition[] conditions { get; set; }
            public Action[] actions { get; set; }
        }

        public class Layer
        {
            public int ambientLightColorB = 32;
            public int ambientLightColorG = 0;
            public int ambientLightColorR = 0;
            public bool followBaseLayerCamera { get; set; }
            public bool isLightingLayer = false;
            public string name { get; set; }
            public bool visibility { get; set; }
            public Camera[] cameras { get; set; }
            public LayerEffect[] effects = new LayerEffect[0];
        }

        public class Camera
        {
            public bool defaultSize = true;
            public bool defaultViewport = true;
            public int height = 0;
            public int viewportBottom = 1;
            public int viewportLeft = 0;
            public int viewportRight = 1;
            public int viewportTop = 0;
            public int width = 0;
        }

        public class LayerEffect
        {
            public string effectType { get; set; }
            public string name { get; set; }
            public Doubleparameters1 doubleParameters { get; set; }
            public Stringparameters1 stringParameters { get; set; }
            public Booleanparameters1 booleanParameters { get; set; }
        }

        public class Doubleparameters1
        {
            public int scaleX { get; set; }
            public int scaleY { get; set; }
        }

        public class Stringparameters1
        {
            public string displacementMapTexture { get; set; }
        }

        public class Booleanparameters1
        {
        }

        public class Eventsfunctionsextension
        {
            public string author { get; set; }
            public string category { get; set; }
            public string description { get; set; }
            public string extensionNamespace { get; set; }
            public string fullName { get; set; }
            public string helpPath { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string previewIconUrl { get; set; }
            public string shortDescription { get; set; }
            public string version { get; set; }
            public Origin origin { get; set; }
            public string[] tags { get; set; }
            public string[] authorIds { get; set; }
            public object[] dependencies { get; set; }
            public Eventsfunction[] eventsFunctions { get; set; }
            public Eventsbasedbehavior[] eventsBasedBehaviors { get; set; }
        }

        public class Origin
        {
            public string identifier { get; set; }
            public string name { get; set; }
        }

        public class Eventsfunction
        {
            public string description { get; set; }
            public string fullName { get; set; }
            public string functionType { get; set; }
            public string group { get; set; }
            public string name { get; set; }
            public bool _private { get; set; }
            public string sentence { get; set; }
            public Event2[] events { get; set; }
            public Parameter[] parameters { get; set; }
            public object[] objectGroups { get; set; }
        }

        public class Event2
        {
            public string type { get; set; }
            public string inlineCode { get; set; }
            public string parameterObjects { get; set; }
            public bool useStrict { get; set; }
            public bool eventsSheetExpanded { get; set; }
            public Condition2[] conditions { get; set; }
            public Action2[] actions { get; set; }
        }

        public class Condition2
        {
            public Type6 type { get; set; }
            public string[] parameters { get; set; }
            public Subinstruction2[] subInstructions { get; set; }
        }

        public class Type6
        {
            public string value { get; set; }
        }

        public class Subinstruction2
        {
            public Type7 type { get; set; }
            public string[] parameters { get; set; }
        }

        public class Type7
        {
            public string value { get; set; }
        }

        public class Action2
        {
            public Type8 type { get; set; }
            public string[] parameters { get; set; }
        }

        public class Type8
        {
            public string value { get; set; }
        }

        public class Parameter
        {
            public bool codeOnly { get; set; }
            public string defaultValue { get; set; }
            public string description { get; set; }
            public string longDescription { get; set; }
            public string name { get; set; }
            public bool optional { get; set; }
            public string supplementaryInformation { get; set; }
            public string type { get; set; }
        }

        public class Eventsbasedbehavior
        {
            public string description { get; set; }
            public string fullName { get; set; }
            public string name { get; set; }
            public string objectType { get; set; }
            public Eventsfunction1[] eventsFunctions { get; set; }
            public Propertydescriptor[] propertyDescriptors { get; set; }
        }

        public class Eventsfunction1
        {
            public string description { get; set; }
            public string fullName { get; set; }
            public string functionType { get; set; }
            public string group { get; set; }
            public string name { get; set; }
            public bool _private { get; set; }
            public string sentence { get; set; }
            public Event3[] events { get; set; }
            public Parameter1[] parameters { get; set; }
            public object[] objectGroups { get; set; }
        }

        public class Event3
        {
            public string type { get; set; }
            public Condition3[] conditions { get; set; }
            public Action3[] actions { get; set; }
            public Color1 color { get; set; }
            public string comment { get; set; }
            public string comment2 { get; set; }
            public string repeatExpression { get; set; }
            public Event4[] events { get; set; }
        }

        public class Color1
        {
            public int b { get; set; }
            public int g { get; set; }
            public int r { get; set; }
            public int textB { get; set; }
            public int textG { get; set; }
            public int textR { get; set; }
        }

        public class Condition3
        {
            public Type9 type { get; set; }
            public string[] parameters { get; set; }
            public Subinstruction3[] subInstructions { get; set; }
        }

        public class Type9
        {
            public string value { get; set; }
        }

        public class Subinstruction3
        {
            public Type10 type { get; set; }
            public string[] parameters { get; set; }
        }

        public class Type10
        {
            public string value { get; set; }
        }

        public class Action3
        {
            public Type11 type { get; set; }
            public string[] parameters { get; set; }
        }

        public class Type11
        {
            public string value { get; set; }
        }

        public class Event4
        {
            public string type { get; set; }
            public Color2 color { get; set; }
            public string comment { get; set; }
            public string comment2 { get; set; }
            public Condition4[] conditions { get; set; }
            public Action4[] actions { get; set; }
        }

        public class Color2
        {
            public int b { get; set; }
            public int g { get; set; }
            public int r { get; set; }
            public int textB { get; set; }
            public int textG { get; set; }
            public int textR { get; set; }
        }

        public class Condition4
        {
            public Type12 type { get; set; }
            public string[] parameters { get; set; }
        }

        public class Type12
        {
            public string value { get; set; }
        }

        public class Action4
        {
            public Type13 type { get; set; }
            public string[] parameters { get; set; }
        }

        public class Type13
        {
            public string value { get; set; }
        }

        public class Parameter1
        {
            public bool codeOnly { get; set; }
            public string defaultValue { get; set; }
            public string description { get; set; }
            public string longDescription { get; set; }
            public string name { get; set; }
            public bool optional { get; set; }
            public string supplementaryInformation { get; set; }
            public string type { get; set; }
        }

        public class Propertydescriptor
        {
            public string value { get; set; }
            public string type { get; set; }
            public string label { get; set; }
            public string description { get; set; }
            public string group { get; set; }
            public object[] extraInformation { get; set; }
            public bool hidden { get; set; }
            public string name { get; set; }
        }

    }
}
