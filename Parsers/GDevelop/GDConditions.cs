using CTFAK.CCN;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZelCTFTranslator.Utils;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class GDConditions
    {
        public static GDJSON.Condition CompareAltVal(Condition condition, GameData gameData)
        {
            var Parameter1 = condition.Items[0].Loader as AlterableValue;
            var Parameter2 = condition.Items[1].Loader as ExpressionParameter;

            var newCondition = new GDJSON.Condition();
            newCondition.type.value = "VarObjet";

            string ObjectName = $"Qualifier {condition.ObjectInfo}";
            if (condition.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[condition.ObjectInfo].name;

            string ComparisonStr;
            switch (Parameter2.Comparsion)
            {
                default:
                case 0:
                    ComparisonStr = "=";
                    break;
                case 1:
                    ComparisonStr = "!=";
                    break;
                case 2:
                    ComparisonStr = "<=";
                    break;
                case 3:
                    ComparisonStr = "<";
                    break;
                case 4:
                    ComparisonStr = ">=";
                    break;
                case 5:
                    ComparisonStr = ">";
                    break;
            }

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "AlterableValue" + GDWriter.AltCharacter(Parameter1.Value), //Alterable Value
                ComparisonStr, //Comparison
                ((int)(Parameter2.Items[0].Loader as LongExp).Value).ToString() //Compare To
            };

            newCondition.parameters = Parameters.ToArray();
            return newCondition;
        }

        public static GDJSON.Condition DefaultType(Condition condition, GameData gameData, bool Object, bool Inverted, string Type)
        {
            var a = ((32767 - 14000) / 18767) * 800;
            var newCondition = new GDJSON.Condition();
            newCondition.type.value = Type;
            newCondition.type.inverted = Inverted;

            string ObjectName = $"Qualifier {condition.ObjectInfo}";
            if (condition.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[condition.ObjectInfo].name;

            var Parameters = new List<string>();
            if (Object) Parameters.Add(ObjectName);
            else Parameters.Add("");
            Parameters.Clear();

            newCondition.parameters = Parameters.ToArray();
            return newCondition;
        }

        public static GDJSON.Condition KeyPressed(Condition condition, GameData gameData)
        {
            var Parameter1 = condition.Items[0].Loader as KeyParameter;

            var newCondition = new GDJSON.Condition();
            newCondition.type.value = "KeyPressed";

            var Parameters = new List<string>
            {
                "",
                GDKeyCodes.ShortKeyCodes[(short)Parameter1.Key],
            };

            newCondition.parameters = Parameters.ToArray();
            return newCondition;
        }

        public static GDJSON.Condition EveryTimer(Condition condition, GameData gameData, int index)
        {
            var Parameter1 = condition.Items[0].Loader as Time;

            var newCondition = new GDJSON.Condition();
            newCondition.type.value = "RepeatEveryXSeconds::Repeat";

            var Parameters = new List<string>
            {
                "", //?
                $"\"{index}\"",
                (Parameter1.Timer / 1000.0).ToString(),
                ""
            };

            newCondition.parameters = Parameters.ToArray();
            return newCondition;
        }
    }
}
