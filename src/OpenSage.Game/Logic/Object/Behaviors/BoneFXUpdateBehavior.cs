﻿using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class BoneFXUpdateBehavior : ObjectBehavior
    {
        internal static BoneFXUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoneFXUpdateBehavior> FieldParseTable = new IniParseTable<BoneFXUpdateBehavior>
        {
            { "DamageFXTypes", (parser, x) => x.DamageFXTypes = parser.ParseEnumBitArray<DamageType>() },
            { "RubbleFXList1", (parser, x) => x.RubbleFXList1 = BoneFXUpdateFXList.Parse(parser) },

            { "DamageParticleTypes", (parser, x) => x.DamageParticleTypes = parser.ParseEnumBitArray<DamageType>() },
            { "RubbleParticleSystem1", (parser, x) => x.RubbleParticleSystem1 = BoneFXUpdateParticleSystem.Parse(parser) },
        };

        public BitArray<DamageType> DamageFXTypes { get; private set; }
        public BoneFXUpdateFXList RubbleFXList1 { get; private set; }

        public BitArray<DamageType> DamageParticleTypes { get; private set; }
        public BoneFXUpdateParticleSystem RubbleParticleSystem1 { get; private set; }
    }

    public sealed class BoneFXUpdateFXList
    {
        internal static BoneFXUpdateFXList Parse(IniParser parser)
        {
            return new BoneFXUpdateFXList
            {
                Bone = parser.ParseAttribute("Bone", () => parser.ParseBoneName()),
                OnlyOnce = parser.ParseAttributeBoolean("OnlyOnce"),
                Min = parser.ParseInteger(),
                Max = parser.ParseInteger(),
                FXList = parser.ParseAttribute("FXList", () => parser.ParseAssetReference())
            };
        }

        public string Bone { get; private set; }
        public bool OnlyOnce { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public string FXList { get; private set; }
    }

    public sealed class BoneFXUpdateParticleSystem
    {
        internal static BoneFXUpdateParticleSystem Parse(IniParser parser)
        {
            return new BoneFXUpdateParticleSystem
            {
                Bone = parser.ParseAttribute("Bone", () => parser.ParseBoneName()),
                OnlyOnce = parser.ParseAttributeBoolean("OnlyOnce"),
                Min = parser.ParseInteger(),
                Max = parser.ParseInteger(),
                ParticleSystem = parser.ParseAttribute("PSys", () => parser.ParseAssetReference())
            };
        }

        public string Bone { get; private set; }
        public bool OnlyOnce { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        public string ParticleSystem { get; private set; }
    }
}
