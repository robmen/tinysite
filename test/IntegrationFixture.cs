﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinySite;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class IntegrationFixture
    {
        [Fact]
        public void BlogRendersCorrectly()
        {
            var blogPath = Path.GetFullPath(@"data\examples\blog");
            var outputPath = Path.GetTempPath() + @"tinysite_test\blog_build";
            var verifyPath = Path.GetFullPath(@"data\examples_output\blog");

            SafeDeleteFolder(outputPath);

            RunTinySite(blogPath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        [Fact]
        public void BookRendersCorrectly()
        {
            var bookPath = Path.GetFullPath(@"data\examples\book");
            var outputPath = Path.GetTempPath() + @"tinysite_test\book_build";
            var verifyPath = Path.GetFullPath(@"data\examples_output\book");

            SafeDeleteFolder(outputPath);

            RunTinySite(bookPath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        [Fact]
        public void DataSiteRendersCorrectly()
        {
            var datasitePath = Path.GetFullPath(@"data\examples\datasite");
            var outputPath = Path.GetTempPath() + @"tinysite_test\datasite_build";
            var verifyPath = Path.GetFullPath(@"data\examples_output\datasite");

            SafeDeleteFolder(outputPath);

            RunTinySite(datasitePath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        [Fact]
        public void HomepageRendersCorrectly()
        {
            var homepagePath = Path.GetFullPath(@"data\examples\homepage");
            var outputPath = Path.GetTempPath() + @"tinysite_test\homepage_build";
            var verifyPath = Path.GetFullPath(@"data\examples_output\homepage");

            SafeDeleteFolder(outputPath);

            RunTinySite(homepagePath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        [Fact]
        public void HomepageRendersNothingSecondPass()
        {
            var homepagePath = Path.GetFullPath(@"data\examples\homepage");
            var outputPath = Path.GetTempPath() + @"tinysite_test\homepage_build";
            var verifyPath = Path.GetFullPath(@"data\examples_output\homepage");

            SafeDeleteFolder(outputPath);

            RunTinySite(homepagePath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);

            RunTinySite(homepagePath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        private static void SafeDeleteFolder(string outputPath)
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }

        private static void RunTinySite(string workingFolder, string outputFolder)
        {
            var result = 0;
            var arguments = "render -out " + outputFolder;

            //var path = Path.GetFullPath("tinysite.exe");
            //if (File.Exists(path))
            //{
            //    var process = new Process();
            //    process.StartInfo = new ProcessStartInfo();
            //    process.StartInfo.FileName = path;
            //    process.StartInfo.Arguments = arguments;
            //    process.StartInfo.CreateNoWindow = true;
            //    process.StartInfo.UseShellExecute = false;
            //    process.StartInfo.WorkingDirectory = workingFolder;
            //    process.Start();

            //    var waited = process.WaitForExit(3 * 60 * 1000);
            //    Assert.True(waited);
            //    result = process.ExitCode;
            //}
            //else
            {
                var folder = Environment.CurrentDirectory;
                Environment.CurrentDirectory = workingFolder;

                result = Program.Main(arguments.Split(' '));

                Environment.CurrentDirectory = folder;
            }
            Assert.Equal(0, result);
        }

        private static void AssertFoldersSame(string outputPath, string verifyPath)
        {
            var expectedFiles = Directory.GetFiles(verifyPath, "*.*", SearchOption.AllDirectories);
            var actualFiles = Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories);

            var expectedSet = new HashSet<string>(expectedFiles);

            foreach (var actualFile in actualFiles)
            {
                var relativeFile = actualFile.Substring(outputPath.Length).TrimStart('\\');

                var expectedFile = Path.Combine(verifyPath, relativeFile);
                Assert.True(expectedSet.Remove(expectedFile), String.Format("Missing {0} file", relativeFile));

                var expectedContents = File.ReadAllText(expectedFile).Replace("\r\n", "\n");
                var actualContents = File.ReadAllText(actualFile).Replace("\r\n", "\n");

                if (Path.GetExtension(relativeFile).Equals(".feed", StringComparison.OrdinalIgnoreCase))
                {
                    actualContents = NormalizeFeed(actualContents);
                }

                var expectedLines = expectedContents.Split('\n').Select((s, i) => $"{relativeFile}({i}): {s}").ToList();
                var actualLines = actualContents.Split('\n').Select((s, i) => $"{relativeFile}({i}): {s}").ToList();

                var index = 0;
                for (; index < actualLines.Count; ++index)
                {
                    if (index >= expectedLines.Count)
                    {
                        break;
                    }

                    Assert.Equal(expectedLines[index], actualLines[index]);
                }

                for (; index < actualLines.Count; ++index)
                {
                    Assert.Equal(String.Empty, actualLines[index]);
                }

                for (; index < expectedLines.Count; ++index)
                {
                    Assert.Equal(expectedLines[index], String.Empty);
                }
            }

            Assert.Empty(expectedSet);
        }

        private static string NormalizeFeed(string text)
        {
            var startUpdated = text.IndexOf("<updated>", StringComparison.Ordinal);

            while (startUpdated > -1)
            {
                var endUpdated = text.IndexOf("</updated>", startUpdated, StringComparison.Ordinal);

                text = text.Substring(0, startUpdated + 9) + "normalized" + text.Substring(endUpdated);

                startUpdated = text.IndexOf("<updated>", endUpdated + 10, StringComparison.Ordinal);
            }

            return text;
        }
    }
}
