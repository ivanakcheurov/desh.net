﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Desh.Core.Parsing.Ast;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Desh.Core.Parsing
{
    public class DeshParser
    {
        public ExpressionBlock Parse (string desh)
        {
            var deserializers = new INodeDeserializer[]
            {
                new ExpressionBlockDeserializer(),
                new Expression_AND_Mapping_Deserializer(),
                new ComparatorDeserializer(),
                new Operator_AND_Mapping_Deserializer(),
                new ValueExpressionTreeDeserializer(),
                new DecisionLeafDeserializer(),
                new OperatorDeserializer(),
            };
            var builder = new DeserializerBuilder();
            foreach (var nodeDeserializer in deserializers)
            {
                builder = builder.WithNodeDeserializer(nodeDeserializer);
            }
            var deserializer =
                builder.WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();

            //Dictionary<string, string[]> map;
            using (var reader = new StringReader(desh))
            {
                var eventReader = new YamlDotNet.Core.Parser(reader);
                // Consume the stream start event "manually"
                // https://stackoverflow.com/questions/27490434/does-yamldotnet-library-support-the-document-separator
                eventReader.Expect<StreamStart>();
                //map = deserializer.Deserialize<Dictionary<string, string[]>>(eventReader);
                var decisionTree = deserializer.Deserialize<ExpressionBlock>(eventReader);
                return decisionTree;
            }

        }
    }

    public abstract class NodeDeserializer<T> : INodeDeserializer
    {
        public bool Deserialize(YamlDotNet.Core.IParser reader, Type expectedType, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer,
            out object value)
        {
            if (expectedType != typeof(T))
            {
                value = null;
                return false;
            }

            var result = Deserialize(reader, nestedObjectDeserializer, out T val);
            value = val;
            return result;
        }

        public abstract bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out T value);
    }

    public class ExpressionBlockDeserializer : NodeDeserializer<ExpressionBlock>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out ExpressionBlock value)
        {
            var start = reader.Current.Start;
            var seq = reader.Peek<SequenceStart>();
            if (seq != null)
            {
                // OR list of Expression_AND_Mapping
                var expressionAndMappings = (Expression_AND_Mapping[])nestedObjectDeserializer(reader, typeof(Expression_AND_Mapping[]));
                value = new Expression_OR_List { ExpressionAndMappings = expressionAndMappings };
                return true;
            }


            value = (Expression_AND_Mapping)nestedObjectDeserializer(reader, typeof(Expression_AND_Mapping));
            return true;
        }
    }

    // ReSharper disable once InconsistentNaming
    public class Expression_AND_Mapping_Deserializer : NodeDeserializer<Expression_AND_Mapping>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out Expression_AND_Mapping value)
        {
            var start = reader.Current.Start;
            // ReSharper disable once UnusedVariable, used for debugging purposes
            var map = reader.Expect<MappingStart>();

            var pairs = new Dictionary<string, Comparator>();
            DecisionLeaf decision = null;
            do
            {
                var variableScalar = reader.Expect<Scalar>();
                if (variableScalar.Value == "decide")
                {
                    if (decision != null)
                        throw new ParseException("Cannot have multiple DECIDE blocks");
                    decision = (DecisionLeaf)nestedObjectDeserializer(reader, typeof(DecisionLeaf));
                }
                else
                {
                    if (decision != null)
                        throw new ParseException("Cannot have more pairs after a DECIDE block in one Expression_AND_Mapping");
                    var variable = variableScalar.Value /*.TrimEnd('?')*/;
                    var comparator = (Comparator)nestedObjectDeserializer(reader, typeof(Comparator));
                    pairs.Add(variable, comparator);
                }

            } while (reader.Allow<MappingEnd>() == null);

            //var check = new Check(variableName, comparators);
            value = new Expression_AND_Mapping { NormalPairs = pairs, DecisionLeaf = decision };
            return true;
        }
    }

    public class ComparatorDeserializer : NodeDeserializer<Comparator>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out Comparator value)
        {
            var start = reader.Current.Start;
            var seq = reader.Allow<SequenceStart>();
            if (seq != null)
            {
                // OR list of Expression_AND_Mapping
                var comparators = new List<Comparator>();
                // ReSharper disable once NotAccessedVariable
                object endCond = null;
                do
                {
                    var comp = (Comparator)nestedObjectDeserializer(reader, typeof(Comparator));
                    comparators.Add(comp);
                    // ReSharper disable once RedundantAssignment
                } while ((endCond = reader.Allow<SequenceEnd>()) == null);

                value = new ComparatorOrList() { Comparators = comparators.ToArray() };
                return true;
            }

            var scalar = reader.Allow<Scalar>();
            if (scalar != null)
            {
                if (scalar.Value.StartsWith("."))
                    value = new UnaryOperator { Name = scalar.Value };
                else
                    value = new ScalarValue { Value = scalar.Value };
                return true;
            }

            var map = reader.Expect<MappingStart>();
            var variableOrOperatorScalar = reader.Peek<Scalar>();
            if (variableOrOperatorScalar?.Value != null && variableOrOperatorScalar.Value.StartsWith("."))
                value = (Operator_AND_Mapping)nestedObjectDeserializer(reader, typeof(Operator_AND_Mapping));
            else
                value = (ValueExpressionTree)nestedObjectDeserializer(reader, typeof(ValueExpressionTree));

            var mapEnd = reader.Expect<MappingEnd>();
            return true;
        }
    }

    // ReSharper disable once InconsistentNaming
    public class Operator_AND_Mapping_Deserializer : NodeDeserializer<Operator_AND_Mapping>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer,
            out Operator_AND_Mapping value)
        {
            var start = reader.Current.Start;
            List<Operator> operators = new List<Operator>();
            DecisionLeaf decision = null;
            ExpressionBlock thenBlock = null;
            do
            {
                var scalar = reader.Peek<Scalar>();
                switch (scalar.Value)
                {
                    case "then":
                        if (thenBlock != null)
                            throw new ParseException("Cannot have two THEN blocks");
                        scalar = reader.Expect<Scalar>();
                        thenBlock = (ExpressionBlock)nestedObjectDeserializer(reader, typeof(ExpressionBlock));
                        break;
                    case "decide":
                        if (decision != null)
                            throw new ParseException("Cannot have multiple DECIDE blocks");
                        scalar = reader.Expect<Scalar>();
                        decision = (DecisionLeaf)nestedObjectDeserializer(reader, typeof(DecisionLeaf));
                        break;
                    default:
                        var @operator = (Operator)nestedObjectDeserializer(reader, typeof(Operator));
                        operators.Add(@operator);
                        break;
                }
            } while (reader.Peek<MappingEnd>() == null);

            value = new Operator_AND_Mapping(operators.ToArray(), thenBlock, decision);
            return true;
        }
    }

    public class ValueExpressionTreeDeserializer : NodeDeserializer<ValueExpressionTree>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer,
            out ValueExpressionTree value)
        {
            var start = reader.Current.Start;
            string[] strings = null;
            var seq = reader.Peek<SequenceStart>();
            if (seq != null)
            {
                strings = (string[])nestedObjectDeserializer(reader, typeof(string[]));
            }
            else
            {
                var scalar = reader.Expect<Scalar>();
                strings = new[] { scalar.Value };
            }

            var thenExpressionBlock = (ExpressionBlock)nestedObjectDeserializer(reader, typeof(ExpressionBlock));
            value = new ValueExpressionTree { ScalarValues = strings.Select(s => new ScalarValue { Value = s }).ToArray(), ThenExpressionBlock = thenExpressionBlock };
            return true;
        }
    }

    public class DecisionLeafDeserializer : NodeDeserializer<DecisionLeaf>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out DecisionLeaf value)
        {
            var start = reader.Current.Start;
            value = new DecisionLeaf { Decision = reader.Expect<Scalar>().Value };
            return true;
        }
    }

    public class OperatorDeserializer : NodeDeserializer<Operator>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out Operator value)
        {
            var start = reader.Current.Start;
            var scalar = reader.Expect<Scalar>();
            var operatorName = scalar.Value;
            if (operatorName.StartsWith(".") == false)
                throw new ParseException("Must be an operator name");
            string[] args = null;
            if (reader.Peek<SequenceStart>() != null)
            {
                args = (string[])nestedObjectDeserializer(reader, typeof(string[]));
            }
            else
            {
                var arg = reader.Expect<Scalar>().Value;
                if (arg != null)
                    args = new[] { arg };
            }
            value = new Operator { Name = operatorName, Arguments = args };
            return true;
        }
    }
}