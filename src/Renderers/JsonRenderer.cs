using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinySite.Extensions;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("json")]
    public class JsonRenderer : IRenderer
    {
        private readonly object _renderLock = new object();

        public string Render(SourceFile sourceFile, string template, dynamic data)
        {
            dynamic document = data["Document"];

            var content = String.Empty;

            lock (_renderLock)
            {
                try
                {
                    foreach (var keyedToken in JObject.Parse(template))
                    {
                        var token = keyedToken.Value;

                        if (keyedToken.Key.Equals("Content", StringComparison.OrdinalIgnoreCase))
                        {
                            content = (string)token;
                        }
                        else if (token.Type == JTokenType.Array || token.Type == JTokenType.Object)
                        {
                            document[keyedToken.Key] = CaseInsensitiveExpando.FromJToken(token);
                        }
                        else
                        {
                            document[keyedToken.Key] = (string)token;
                        }
                    }
                }
                catch (JsonReaderException e)
                {
                    Console.Error.WriteLine("{0}({1},{2}): error JSON1 : {3}", sourceFile.SourcePath, e.LineNumber, e.LinePosition, e.Message);
                    return String.Empty;
                }
            }

            return content;
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
