using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace RobMensching.TinySite.Test
{
    public class IntegrationFixture
    {
        [Fact]
        public void BlogRendersCorrectly()
        {
            var blogPath = Path.GetFullPath(@"..\..\..\examples\blog");
            var outputPath = Path.GetTempPath() + @"tinysite_test\blog_build";
            var verifyPath = Path.GetFullPath(@"data\examples\blog");

            RunTinySite(blogPath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        [Fact]
        public void BookRendersCorrectly()
        {
            var bookPath = Path.GetFullPath(@"..\..\..\examples\book");
            var outputPath = Path.GetTempPath() + @"tinysite_test\book_build";
            var verifyPath = Path.GetFullPath(@"data\examples\book");

            RunTinySite(bookPath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        [Fact]
        public void HomepageRendersCorrectly()
        {
            var homepagePath = Path.GetFullPath(@"..\..\..\examples\homepage");
            var outputPath = Path.GetTempPath() + @"tinysite_test\homepage_build";
            var verifyPath = Path.GetFullPath(@"data\examples\homepage");

            RunTinySite(homepagePath, outputPath);

            AssertFoldersSame(outputPath, verifyPath);
        }

        private static void RunTinySite(string workingFolder, string outputFolder)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = "tinysite.exe";
            process.StartInfo.Arguments = "render -out " + outputFolder;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = workingFolder;
            process.Start();

            var waited = process.WaitForExit(3 * 60 * 1000);
            Assert.True(waited);
            Assert.Equal(0, process.ExitCode);
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

                Assert.Equal(expectedContents, actualContents);
            }

            Assert.Empty(expectedSet);
        }

        private static string NormalizeFeed(string text)
        {
            var startUpdated = text.IndexOf("<updated>");

            while (startUpdated > -1)
            {
                var endUpdated = text.IndexOf("</updated>", startUpdated);

                text = text.Substring(0, startUpdated + 9) + "normalized" + text.Substring(endUpdated);

                startUpdated = text.IndexOf("<updated>", endUpdated + 10);
            }

            return text;
        }
    }
}
