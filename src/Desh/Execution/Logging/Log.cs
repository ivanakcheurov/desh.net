using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Desh.Execution.Logging
{
    public class Log
    {
        public string EngineVersion { get; set; }
        public string DeshMd5 { get; set; }
        // still uses double-quoted style when there is an empty-line within serialized Yaml: https://github.com/aaubry/YamlDotNet/issues/261
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Desh { get; set; }
        public IReadOnlyDictionary<int, Step> Steps { get; set; }
    }
}
