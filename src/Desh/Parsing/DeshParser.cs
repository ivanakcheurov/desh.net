using Desh.Parsing.Ast;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
#pragma warning disable 1591

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
                builder.WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

            //Dictionary<string, string[]> map;
            using (var reader = new StringReader(desh))
            {
                var eventReader = new Parser(reader);
                // Consume the stream start event "manually"
                // https://stackoverflow.com/questions/27490434/does-yamldotnet-library-support-the-document-separator
                eventReader.Consume<StreamStart>();
                //map = deserializer.Deserialize<Dictionary<string, string[]>>(eventReader);
                var decisionTree = deserializer.Deserialize<ExpressionBlock>(eventReader);
                return decisionTree;
            }

        }
    }

    public abstract class NodeDeserializer<T> : INodeDeserializer
    {
        public IContext Ctx { get; }
        public string SourceDesh => Ctx.SourceDesh;

        public string ClassName => GetType().Name;

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
            var _ = reader.Current.Start;

            if (reader.Accept<SequenceStart>(out var seq))
            {
                // OR list of Expression_AND_Mapping
                var expressionAndMappings = (Expression_AND_Mapping[])nestedObjectDeserializer(reader, typeof(Expression_AND_Mapping[]));
                // todo: probably seq doesn't cover the whole list (but only a couple of characters in the beginning)
                value = new Expression_OR_List(SourceDesh, seq.ToDeshSpan()) { ExpressionAndMappings = expressionAndMappings };
                return true;
            }

            if (reader.Accept<Scalar>(out var _))
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
            var _ = reader.Current.Start;
            // ReSharper disable once UnusedVariable, used for debugging purposes
            var map = reader.Consume<MappingStart>();

            var pairs = new Dictionary<Variable, Comparator>();
            ExpressionBlock thenBlock = null;
            DecisionLeaf decision = null;
            MappingEnd end;
            do
            {
                var variableScalar = reader.Consume<Scalar>();
                switch (variableScalar.Value)
                {
                    case "anyIsTrue":
                        throw new NotImplementedException("anyIsTrue");
                        //var seq = reader.Peek<SequenceStart>();
                        //if (seq != null)
                        //{
                        //    // OR list of Expression_AND_Mapping
                        //    var expressionAndMappings = (Expression_AND_Mapping[])nestedObjectDeserializer(reader, typeof(Expression_AND_Mapping[]));
                        //    value = new Expression_OR_List { ExpressionAndMappings = expressionAndMappings };
                        //    return true;
                        //}
                        //pairs.Add();
                        //thenBlock = (ExpressionBlock)nestedObjectDeserializer(reader, typeof(ExpressionBlock));
                        //break;
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
                        var variable = new Variable(SourceDesh, variableScalar.ToDeshSpan()) {Name = variableScalar.Value}; /*.TrimEnd('?')*/
                        var comparator = (Comparator)nestedObjectDeserializer(reader, typeof(Comparator));
                        pairs.Add(variable, comparator);
                        break;
                }

            } while (!reader.TryConsume(out end));

            //var check = new Check(variableName, comparators);
            value = new Expression_AND_Mapping(SourceDesh, Extensions.ToDeshSpan(map, end)) { NormalPairs = pairs, ThenExpressionBlock = thenBlock, DecisionLeaf = decision };
            return true;
        }
        public Expression_AND_Mapping_Deserializer(IContext ctx) : base(ctx) { }
    }

    public class ComparatorDeserializer : NodeDeserializer<Comparator>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out Comparator value)
        {
            var _ = reader.Current.Start;
            if (reader.TryConsume<SequenceStart>(out var seq))
            {
                // OR list of Expression_AND_Mapping
                var comparators = new List<Comparator>();
                // ReSharper disable once NotAccessedVariable
                SequenceEnd endCond;
                do
                {
                    var comp = (Comparator)nestedObjectDeserializer(reader, typeof(Comparator));
                    comparators.Add(comp);
                    // ReSharper disable once RedundantAssignment
                } while (!reader.TryConsume(out endCond));

                value = new ComparatorOrList (SourceDesh, Extensions.ToDeshSpan(seq, endCond)) { Comparators = comparators.ToArray()};
                return true;
            }

            if (reader.TryConsume<Scalar>(out var scalar))
            {
                if (Ctx.OperatorRecognizer.Recognize(scalar.Value))
                    value = new UnaryOperator(SourceDesh, scalar.ToDeshSpan()) { Name = scalar.Value };
                else
                    value = new ScalarValue(SourceDesh, scalar.ToDeshSpan()) { Value = scalar.Value };
                return true;
            }

            reader.Consume<MappingStart>();
            if (reader.Accept<Scalar>(out var variableOrOperatorScalar) && variableOrOperatorScalar.Value != null && Ctx.OperatorRecognizer.Recognize(variableOrOperatorScalar.Value))
                value = (Operator_AND_Mapping)nestedObjectDeserializer(reader, typeof(Operator_AND_Mapping));
            else
                value = (ValueExpressionTree)nestedObjectDeserializer(reader, typeof(ValueExpressionTree));

            reader.Consume<MappingEnd>();
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
            var _ = reader.Current;
            List<Operator> operators = new List<Operator>();
            DecisionLeaf decision = null;
            ExpressionBlock thenBlock = null;
            MappingEnd end;
            do
            {
                reader.Accept<Scalar>(out var scalar);
                switch (scalar.Value)
                {
                    case "then":
                        if (thenBlock != null)
                            throw new ParseException("Cannot have two THEN blocks");
                        reader.Consume<Scalar>();
                        thenBlock = (ExpressionBlock)nestedObjectDeserializer(reader, typeof(ExpressionBlock));
                        break;
                    case "decide":
                        if (decision != null)
                            throw new ParseException("Cannot have multiple DECIDE blocks");
                        reader.Consume<Scalar>();
                        decision = (DecisionLeaf)nestedObjectDeserializer(reader, typeof(DecisionLeaf));
                        break;
                    default:
                        var @operator = (Operator)nestedObjectDeserializer(reader, typeof(Operator));
                        operators.Add(@operator);
                        break;
                }
            } while (!reader.Accept(out end));

            value = new Operator_AND_Mapping(operators.ToArray(), thenBlock, decision, SourceDesh, Extensions.ToDeshSpan(_, end));
            return true;
        }
        public Operator_AND_Mapping_Deserializer(IContext ctx) : base(ctx) { }
    }

    public class ValueExpressionTreeDeserializer : NodeDeserializer<ValueExpressionTree>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer,
            out ValueExpressionTree value)
        {
            var _ = reader.Current;
            List<ScalarValue> scalars = new List<ScalarValue>();
            if (reader.TryConsume<SequenceStart>(out var _))
            {
                do
                {
                    var scalar = reader.Consume<Scalar>();
                    scalars.Add(new ScalarValue(SourceDesh, scalar.ToDeshSpan()){Value = scalar.Value});
                }
                while (!reader.TryConsume(out SequenceEnd _));
            }
            else
            {
                var scalar = reader.Consume<Scalar>();
                scalars.Add(new ScalarValue(SourceDesh, scalar.ToDeshSpan()) { Value = scalar.Value });
            }

            var thenExpressionBlock = (ExpressionBlock)nestedObjectDeserializer(reader, typeof(ExpressionBlock));
            // todo: probably _ doesn't cover the whole list (but only a couple of characters in the beginning)
            value = new ValueExpressionTree(SourceDesh, _.ToDeshSpan()) { ScalarValues = scalars.ToArray(), ThenExpressionBlock = thenExpressionBlock };
            return true;
        }
        public ValueExpressionTreeDeserializer(IContext ctx) : base(ctx) { }
    }

    public class DecisionLeafDeserializer : NodeDeserializer<DecisionLeaf>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out DecisionLeaf value)
        {
            var _ = reader.Current.Start;
            var decisionScalar = reader.Consume<Scalar>();
            value = new DecisionLeaf (SourceDesh, decisionScalar.ToDeshSpan()) { Decision = decisionScalar.Value };
            return true;
        }
        public DecisionLeafDeserializer(IContext ctx) : base(ctx) { }
    }

    public class OperatorDeserializer : NodeDeserializer<Operator>
    {
        public override bool Deserialize(YamlDotNet.Core.IParser reader, Func<YamlDotNet.Core.IParser, Type, object> nestedObjectDeserializer, out Operator value)
        {
            var _ = reader.Current.Start;
            var operatorNameScalar = reader.Consume<Scalar>();
            var operatorName = operatorNameScalar.Value;
            if (Ctx.OperatorRecognizer.Recognize(operatorName) == false)
                throw new ParseException("Must be an operator name");
            string[] args = null;
            if (reader.Accept<SequenceStart>(out var _))
            {
                args = (string[])nestedObjectDeserializer(reader, typeof(string[]));
            }
            else
            {
                var arg = reader.Consume<Scalar>().Value;
                if (arg != null)
                    args = new[] { arg };
            }
            // todo: also cover the arguments and not just the name of the operator
            value = new Operator(SourceDesh, operatorNameScalar.ToDeshSpan()) { Name = operatorName, Arguments = args };
            return true;
        }
        public OperatorDeserializer(IContext ctx) : base(ctx) { }
    }
}