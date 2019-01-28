using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Desh.Parsing.Ast;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Desh.Parsing
{
    public class DeshParser
    {
        public ExpressionBlock Parse (string desh, IContext logger)
        {
            var deserializers = new INodeDeserializer[]
            {
                new ExpressionBlockDeserializer(logger),
                new Expression_AND_Mapping_Deserializer(logger),
                new ComparatorDeserializer(logger),
                new Operator_AND_Mapping_Deserializer(logger),
                new ValueExpressionTreeDeserializer(logger),
                new DecisionLeafDeserializer(logger),
                new OperatorDeserializer(logger),
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
        public IContext Ctx { get; }

        public string ClassName => this.GetType().Name;

        protected void LogStart(YamlDotNet.Core.IParser reader) => Ctx.Logger.LogStart(ClassName, null, reader.Current.Start.Line, reader.Current.Start.Column);
        protected void LogEnd(YamlDotNet.Core.IParser reader) => Ctx.Logger.LogEnd(ClassName, null, reader.Current.End.Line, reader.Current.End.Column);

        protected NodeDeserializer(IContext ctx)
        {
            Ctx = ctx;
        }

        public bool Deserialize(YamlDotNet.Core.IParser reader, Type expectedType, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer,
            out object value)
        {
            if (expectedType != typeof(T))
            {
                value = null;
                return false;
            }

            LogStart(reader);
            var result = Deserialize(reader, nestedObjectDeserializer, out T val);
            LogEnd(reader);
            value = val;
            return result;
        }

        public abstract bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out T value);
    }

    public class ExpressionBlockDeserializer : NodeDeserializer<ExpressionBlock>
    {
        public ExpressionBlockDeserializer(IContext ctx) : base(ctx)
        {
        }

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

            var scalar = reader.Peek<Scalar>();
            if (scalar != null)
            {
                value = (DecisionLeaf)nestedObjectDeserializer(reader, typeof(DecisionLeaf));
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
            ExpressionBlock thenBlock = null;
            DecisionLeaf decision = null;
            do
            {
                var variableScalar = reader.Expect<Scalar>();
                switch (variableScalar.Value)
                {
                    case "then":
                        if (thenBlock != null)
                            throw new ParseException("Cannot have two THEN blocks");
                        thenBlock = (ExpressionBlock)nestedObjectDeserializer(reader, typeof(ExpressionBlock));
                        break;
                    case "decide":
                        if (decision != null)
                            throw new ParseException("Cannot have multiple DECIDE blocks");
                        decision = (DecisionLeaf)nestedObjectDeserializer(reader, typeof(DecisionLeaf));
                        break;
                    default:
                        if (decision != null)
                            throw new ParseException("Cannot have more pairs after a DECIDE block in one Expression_AND_Mapping");
                        if (thenBlock != null)
                            throw new ParseException("Cannot have more pairs after a THEN block in one Expression_AND_Mapping");
                        var variable = variableScalar.Value /*.TrimEnd('?')*/;
                        var comparator = (Comparator)nestedObjectDeserializer(reader, typeof(Comparator));
                        pairs.Add(variable, comparator);
                        break;
                }

            } while (reader.Allow<MappingEnd>() == null);

            //var check = new Check(variableName, comparators);
            value = new Expression_AND_Mapping { NormalPairs = pairs, ThenExpressionBlock = thenBlock, DecisionLeaf = decision };
            return true;
        }
        public Expression_AND_Mapping_Deserializer(IContext ctx) : base(ctx) { }
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
                if (Ctx.OperatorRecognizer.Recognize(scalar.Value))
                    value = new UnaryOperator { Name = scalar.Value };
                else
                    value = new ScalarValue { Value = scalar.Value };
                return true;
            }

            var map = reader.Expect<MappingStart>();
            var variableOrOperatorScalar = reader.Peek<Scalar>();
            if (variableOrOperatorScalar?.Value != null && Ctx.OperatorRecognizer.Recognize(variableOrOperatorScalar.Value))
                value = (Operator_AND_Mapping)nestedObjectDeserializer(reader, typeof(Operator_AND_Mapping));
            else
                value = (ValueExpressionTree)nestedObjectDeserializer(reader, typeof(ValueExpressionTree));

            var mapEnd = reader.Expect<MappingEnd>();
            return true;
        }
        public ComparatorDeserializer(IContext ctx) : base(ctx) { }
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
        public Operator_AND_Mapping_Deserializer(IContext ctx) : base(ctx) { }
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
        public ValueExpressionTreeDeserializer(IContext ctx) : base(ctx) { }
    }

    public class DecisionLeafDeserializer : NodeDeserializer<DecisionLeaf>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out DecisionLeaf value)
        {
            var start = reader.Current.Start;
            value = new DecisionLeaf { Decision = reader.Expect<Scalar>().Value };
            return true;
        }
        public DecisionLeafDeserializer(IContext ctx) : base(ctx) { }
    }

    public class OperatorDeserializer : NodeDeserializer<Operator>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out Operator value)
        {
            var start = reader.Current.Start;
            var scalar = reader.Expect<Scalar>();
            var operatorName = scalar.Value;
            if (Ctx.OperatorRecognizer.Recognize(operatorName) == false)
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
        public OperatorDeserializer(IContext ctx) : base(ctx) { }
    }
}