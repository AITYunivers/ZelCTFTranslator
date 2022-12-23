using CTFAK.CCN;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ZelCTFTranslator.Parsers.GDevelop
{
    public class GDConditions
    {
        public static GDJSON.Condition CompareAltVal(Condition condition, GameData gameData)
        {
            ByteWriter Parameter1 = new ByteWriter(new MemoryStream());
            condition.Items[0].Write(Parameter1);
            ByteWriter Parameter2 = new ByteWriter(new MemoryStream());
            condition.Items[1].Write(Parameter2);

            ByteReader reader = new ByteReader(Parameter1.GetBuffer());
            reader.Skip(4);
            short AlterableValue = reader.ReadInt16();

            reader = new ByteReader(Parameter2.GetBuffer());
            reader.Skip(4);
            short Comparison = reader.ReadInt16();
            reader.Skip(6);
            int CompareTo = reader.ReadInt32();

            var newCondition = new GDJSON.Condition();
            newCondition.type.value = "VarObjet";

            string ComparisonStr;
            switch (Comparison)
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

            string ObjectName = $"Qualifier {condition.ObjectInfo}";
            if (condition.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[condition.ObjectInfo].name;

            var Parameters = new List<string>
            {
                ObjectName, //Object Name
                "AlterableValue" + GDevelopTranslator.AltCharacter(AlterableValue), //Alterable Value
                ComparisonStr, //Comparison
                CompareTo.ToString() //Compare To
            };

            newCondition.parameters = Parameters.ToArray();
            return newCondition;
        }

        public static GDJSON.Condition DefaultType(Condition condition, GameData gameData, bool Object, bool Inverted, string Type)
        {
            var newCondition = new GDJSON.Condition();
            newCondition.type.value = Type;
            newCondition.type.inverted = Inverted;

            string ObjectName = $"Qualifier {condition.ObjectInfo}";
            if (condition.ObjectInfo <= gameData.frameitems.Count)
                ObjectName = gameData.frameitems[condition.ObjectInfo].name;

            var Parameters = new List<string>();
            if (Object) Parameters.Add(ObjectName);
            else Parameters.Add("");

            newCondition.parameters = Parameters.ToArray();
            return newCondition;
        }
    }
}
