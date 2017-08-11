﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenZH.Data.Ini.Parser
{
    internal sealed partial class IniParser
    {
        private static readonly Dictionary<string, Action<IniParser, IniDataContext>> BlockParsers = new Dictionary<string, Action<IniParser, IniDataContext>>
        {
            { "AIData", (parser, context) => context.AIData = AIData.Parse(parser) },
            { "Animation", (parser, context) => context.Animations.Add(Animation.Parse(parser)) },
            { "Armor", (parser, context) => context.Armors.Add(Armor.Parse(parser)) },
            { "AudioSettings", (parser, context) => context.AudioSettings = AudioSettings.Parse(parser) },
            { "Campaign", (parser, context) => context.Campaigns.Add(Campaign.Parse(parser)) },
            { "CommandButton", (parser, context) => context.CommandButtons.Add(CommandButton.Parse(parser)) },
            { "CommandMap", (parser, context) => context.CommandMaps.Add(CommandMap.Parse(parser)) },
            { "CommandSet", (parser, context) => context.CommandSets.Add(CommandSet.Parse(parser)) },
            { "ControlBarResizer", (parser, context) => context.ControlBarResizers.Add(ControlBarResizer.Parse(parser)) },
            { "ControlBarScheme", (parser, context) => context.ControlBarSchemes.Add(ControlBarScheme.Parse(parser)) },
            { "CrateData", (parser, context) => context.CrateDatas.Add(CrateData.Parse(parser)) },
            { "Credits", (parser, context) => context.Credits = Credits.Parse(parser) },
            { "DamageFX", (parser, context) => context.DamageFXs.Add(DamageFX.Parse(parser)) },
            { "DrawGroupInfo", (parser, context) => context.DrawGroupInfo = DrawGroupInfo.Parse(parser) },
            { "EvaEvent", (parser, context) => context.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "FXList", (parser, context) => context.FXLists.Add(FXList.Parse(parser)) },
            { "GameData", (parser, context) => context.GameData = GameData.Parse(parser) },
            { "HeaderTemplate", (parser, context) => context.HeaderTemplates.Add(HeaderTemplate.Parse(parser)) },
            { "Language", (parser, context) => context.Language = Language.Parse(parser) },
            { "Object", (parser, context) => context.Objects.Add(ObjectDefinition.Parse(parser)) },
        };

        private readonly List<IniToken> _tokens;
        private int _tokenIndex;

        private readonly Stack<string> _currentBlockOrFieldStack;

        public IniParser(string source, string fileName)
        {
            var lexer = new IniLexer(source, fileName);

            _tokens = new List<IniToken>();

            IniToken token;
            do
            {
                _tokens.Add(token = lexer.Lex());
            } while (token.TokenType != IniTokenType.EndOfFile);

            _currentBlockOrFieldStack = new Stack<string>();
        }

        private IniToken Current => _tokens[_tokenIndex];

        public IniTokenPosition CurrentPosition => Current.Position;

        private IniToken NextToken(params IniTokenType[] tokenTypes)
        {
            var token = Current;
            if (tokenTypes.Length > 0 && !tokenTypes.Contains(token.TokenType))
            {
                throw new IniParseException($"Expected token of type '{string.Join(",", tokenTypes)}', but got token of type '{token.TokenType}'.", token.Position);
            }
            _tokenIndex++;
            return token;
        }

        private IniToken? NextTokenIf(IniTokenType tokenType)
        {
            var token = Current;
            if (token.TokenType == tokenType)
            {
                _tokenIndex++;
                return token;
            }
            return null;
        }

        private IniToken NextIdentifierToken(string expectedStringValue)
        {
            var token = NextToken(IniTokenType.Identifier);
            if (token.StringValue != expectedStringValue)
            {
                throw new IniParseException($"Expected an identifier with name '{expectedStringValue}', but got '{token.StringValue}'.", token.Position);
            }
            return token;
        }

        private int NextIntegerLiteralTokenValue()
        {
            return NextToken(IniTokenType.IntegerLiteral).IntegerValue;
        }

        private string NextStringLiteralTokenValue()
        {
            return NextToken(IniTokenType.StringLiteral).StringValue;
        }

        private void UnexpectedToken(IniToken token)
        {
            throw new IniParseException($"Unexpected token: {token}", token.Position);
        }

        public string ParseAsciiString() => NextToken(IniTokenType.Identifier, IniTokenType.StringLiteral).StringValue;

        public string[] ParseAsciiStringArray()
        {
            var result = new List<string>();

            do
            {
                result.Add(ParseAsciiString());
            } while (Current.TokenType == IniTokenType.Identifier);

            return result.ToArray();
        }

        public int ParseInteger() => NextIntegerLiteralTokenValue();

        public byte ParseByte() => (byte) NextIntegerLiteralTokenValue();

        public float ParseFloat()
        {
            var token = NextToken(IniTokenType.FloatLiteral, IniTokenType.IntegerLiteral);
            return (token.TokenType == IniTokenType.FloatLiteral)
                ? token.FloatValue
                : token.IntegerValue;
        }

        public float ParsePercentage() => NextToken(IniTokenType.PercentLiteral).FloatValue;

        public bool ParseBoolean()
        {
            var token = NextToken(IniTokenType.Identifier);

            switch (token.StringValue.ToUpper())
            {
                case "YES":
                    return true;

                case "NO":
                    return false;

                default:
                    throw new IniParseException($"Invalid value for boolean: '{token.StringValue}'", token.Position);
            }
        }

        public string ParseIdentifier()
        {
            return NextToken(IniTokenType.Identifier).StringValue;
        }

        private T ParseEnum<T>(Dictionary<string, T> stringToValueMap)
            where T : struct
        {
            var token = NextToken(IniTokenType.Identifier);

            if (stringToValueMap.TryGetValue(token.StringValue.ToUpper(), out var enumValue))
                return enumValue;

            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.StringValue}'", token.Position);
        }

        private T ParseEnumFlags<T>(T noneValue, Dictionary<string, T> stringToValueMap)
            where T : struct
        {
            var result = noneValue;

            do
            {
                var token = NextToken(IniTokenType.Identifier);

                if (!stringToValueMap.TryGetValue(token.StringValue.ToUpper(), out var enumValue))
                    throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.StringValue}'", token.Position);

                // Ugh.
                result = (T) (object) ((int) (object) result | (int) (object) enumValue);
            } while (Current.TokenType != IniTokenType.EndOfLine);

            return result;
        }

        public T ParseTopLevelNamedBlock<T>(
            Action<T, string> setNameCallback,
            IniParseTable<T> fieldParseTable)
            where T : class, new()
        {
            NextToken();

            var result = ParseNamedBlock(setNameCallback, fieldParseTable);

            NextTokenIf(IniTokenType.EndOfLine);

            return result;
        }

        public T ParseNamedBlock<T>(
            Action<T, string> setNameCallback,
            IniParseTable<T> fieldParseTable)
            where T : class, new()
        {
            var result = new T();

            var name = ParseIdentifier();
            setNameCallback(result, name);

            NextToken(IniTokenType.EndOfLine);

            ParseBlockContent(result, fieldParseTable);

            return result;
        }

        public T ParseTopLevelBlock<T>(
            IniParseTable<T> fieldParseTable)
            where T : class, new()
        {
            NextToken();

            var result = ParseBlock(fieldParseTable);

            NextTokenIf(IniTokenType.EndOfLine);

            return result;
        }

        public T ParseBlock<T>(
           IniParseTable<T> fieldParseTable)
           where T : class, new()
        {
            var result = new T();

            NextToken(IniTokenType.EndOfLine);

            ParseBlockContent(result, fieldParseTable);

            return result;
        }

        private void ParseBlockContent<T>(
            T result,
            IniParseTable<T> fieldParseTable)
            where T : class, new()
        {
            while (Current.TokenType == IniTokenType.Identifier || Current.TokenType == IniTokenType.IntegerLiteral)
            {
                if (Current.TokenType == IniTokenType.Identifier && Current.StringValue.ToUpper() == "END")
                {
                    NextToken();
                    break;
                }
                else
                {
                    var fieldName = Current.TokenType == IniTokenType.Identifier
                        ? Current.StringValue
                        : Current.IntegerValue.ToString();

                    if (fieldParseTable.TryGetValue(fieldName, out var fieldParser))
                    {
                        _currentBlockOrFieldStack.Push(fieldName);

                        NextToken();
                        NextTokenIf(IniTokenType.Equals);

                        fieldParser(this, result);

                        NextToken(IniTokenType.EndOfLine);

                        _currentBlockOrFieldStack.Pop();
                    }
                    else
                    {
                        throw new IniParseException($"Unexpected field '{fieldName}' in block '{_currentBlockOrFieldStack.Peek()}'.", Current.Position);
                    }
                }
            }
        }

        public void ParseFile(IniDataContext dataContext)
        {
            while (Current.TokenType != IniTokenType.EndOfFile)
            {
                switch (Current.TokenType)
                {
                    case IniTokenType.Identifier:
                        if (BlockParsers.TryGetValue(Current.StringValue, out var blockParser))
                        {
                            _currentBlockOrFieldStack.Push(Current.StringValue);
                            blockParser(this, dataContext);
                            _currentBlockOrFieldStack.Pop();
                        }
                        else
                        {
                            throw new IniParseException($"Unexpected block '{Current.StringValue}'.", Current.Position);
                        }
                        break;

                    default:
                        UnexpectedToken(Current);
                        break;
                }
            }
        }
    }
}