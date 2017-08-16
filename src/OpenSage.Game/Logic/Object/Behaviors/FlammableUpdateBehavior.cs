﻿using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of the AFLAME, SMOLDERING, and BURNED condition states.
    /// </summary>
    public sealed class FlammableUpdateBehavior : ObjectBehavior
    {
        internal static FlammableUpdateBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<FlammableUpdateBehavior> FieldParseTable = new IniParseTable<FlammableUpdateBehavior>
        {
            { "FlameDamageLimit", (parser, x) => x.FlameDamageLimit = parser.ParseInteger() },
            { "FlameDamageExpiration", (parser, x) => x.FlameDamageExpiration = parser.ParseInteger() },
            { "AflameDuration", (parser, x) => x.AflameDuration = parser.ParseInteger() },
            { "AflameDamageAmount", (parser, x) => x.AflameDamageAmount = parser.ParseInteger() },
            { "AflameDamageDelay", (parser, x) => x.AflameDamageDelay = parser.ParseInteger() },
        };

        /// <summary>
        /// How much flame damage to receive before catching fire.
        /// </summary>
        public int FlameDamageLimit { get; private set; }

        /// <summary>
        /// Time within which <see cref="FlameDamageLimit"/> must be received in order to catch fire.
        /// </summary>
        public int FlameDamageExpiration { get; private set; }

        /// <summary>
        /// How long to burn for after catching fire.
        /// </summary>
        public int AflameDuration { get; private set; }

        /// <summary>
        /// Amount of damage inflicted.
        /// </summary>
        public int AflameDamageAmount { get; private set; }

        /// <summary>
        /// Delay between each time that <see cref="AflameDamageAmount"/> is inflicted.
        /// </summary>
        public int AflameDamageDelay { get; private set; }
    }
}
