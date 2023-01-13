using static ZelCTFTranslator.Parsers.Game_Maker_Studio_2.YYs.ObjectYY;

namespace ZelCTFTranslator.Parsers.Game_Maker_Studio_2.YYs
{
    public class ObjectYY
    {
        public class RootObject
        {
            public string resourceType = "GMObject";
            public string resourceVersion = "1.0";
            public string name { get; set; }
            public SpriteID spriteId = new SpriteID();
            public bool solid = false;
            public bool visible { get; set; }
            public bool managed = true;
            public object spriteMaskId = null;
            public bool persistent = false;
            public object parentObjectId = null;
            public bool physicsObject = false;
            public bool physicsSensor = false;
            public int physicsShape = 1;
            public int physicsGroup = 1;
            public float physicsDensity = 0.5f;
            public float physicsRestitution = 0.1f;
            public float physicsLinearDamping = 0.1f;
            public float physicsAngularDamping = 0.1f;
            public float physicsFriction = 0.2f;
            public bool physicsStartAwake = true;
            public bool physicsKinematic = false;
            public object[] physicsShapePoints = new object[0];
            public object[] eventList = new object[0];
            public object[] properties = new object[0];
            public object[] overriddenProperties = new object[0];
            public Parent parent = new Parent();
        }

        public class SpriteID
        {
            public string name { get; set; }
            public string path { get; set; }
        }

        public class Parent
        {
            public string name = "Objects";
            public string path = "folders/Objects.yy";
        }

    }
}
