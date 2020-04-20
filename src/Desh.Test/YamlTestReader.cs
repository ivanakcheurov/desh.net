using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Desh.Test
{
    public static class YamlTestReader
    {
        public static TheoryData<string, TestCaseSource, string, Dictionary<string, string>, string> GetData(string filename)
        {
            var result = new TheoryData<string, TestCaseSource, string, Dictionary<string, string>, string>();
            var deserializer =
                new DeserializerBuilder()
                    .WithNodeDeserializer(new TestCaseWithSourceDeserializer{Resource = filename })
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
            var testCases = deserializer.Deserialize<List<TestCaseWithSource>>(File.ReadAllText(filename));
            foreach (var testCase in testCases)
            {
                if (string.IsNullOrWhiteSpace(testCase.Ignore))
                    result.Add(testCase.Name, testCase.Source, testCase.Desh, testCase.Vars, testCase.Expected);
            }

            return result;
        }
    }

    public class TestCaseWithSource : TestCase
    {
        public TestCaseSource Source { get; set; }
    }
    public class TestCase
    {
        public string Name { get; set; }

        public string Desh { get; set; }
        public Dictionary<string, string> Vars { get; set; }
        public string Expected { get; set; }
        public string Ignore { get; set; }
    }

    public class TestCaseSource
    {
        public string Resource { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public override string ToString()
        {
            return $"{Resource}-{Line}:{Column}";
        }
    }

    public class TestCaseWithSourceDeserializer : INodeDeserializer
    {
        public string Resource { get; set; }

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (expectedType != typeof(TestCaseWithSource))
            {
                value = null;
                return false;
            }

            var start = reader.Current.Start;
            var source = new TestCaseSource { Resource = Resource, Line = start.Line, Column = start.Column };

            var testCase = (TestCase)nestedObjectDeserializer(reader, typeof(TestCase));
            if (testCase == null)
            {
                value = null;
                return false;
            }
            value = new TestCaseWithSource
            {
                Source = source,
                Desh = testCase.Desh,
                Expected = testCase.Expected,
                Ignore = testCase.Ignore,
                Vars = testCase.Vars,
                Name = testCase.Name,
            };
            return true;
        }
    }
}
