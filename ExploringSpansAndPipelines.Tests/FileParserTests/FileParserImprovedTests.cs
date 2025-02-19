using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ExploringSpansAndPipelines.Models;
using ExploringSpansAndPipelines.Parsers;
using ExploringSpansAndPipelines.Tests.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExploringSpansAndPipelines.Tests.FileParserTests
{
    [TestClass]
    public class FileParserImprovedTests
    {
        private CompareLogic _compareLogic = null!;

        [TestInitialize]
        public void Setup()
        {
            _compareLogic = new CompareLogic();
        }

        [TestMethod]
        public async Task Parse_RegularLines()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "TestData.psv");
            var fileParser = new FileParserImproved();

            var videogames = await fileParser.Parse(file);

            var result = _compareLogic.Compare(TestData.Videogames, videogames.ToArray());
            Assert.IsTrue(result.AreEqual);
        }
        
        [TestMethod]
        public async Task Parse_LongLine()
        {
            // Arrange
            var current = Directory.GetCurrentDirectory();
            var longLineFile = Path.Combine(current, "Assets", "really-long-line.txt");
            var longLine = await File.ReadAllTextAsync(longLineFile);

            var file = Path.Combine(current, "temp.psv");
            var expected = CreateExpected(longLine);
            await CreateFile(file, expected);
            
            var fileParser = new FileParserImproved();

            // Act
            var videogames = await fileParser.Parse(file);
            DeleteFile(file);

            // Assert
            var result = _compareLogic.Compare(expected, videogames.ToArray());
            Assert.IsTrue(result.AreEqual);
        }
        
        private static Videogame[] CreateExpected(string longLine)
        {
            return new[]
            {
                new Videogame
                {
                    Id = Guid.Parse("385562C2-00E0-4B45-8A99-0B54E0BC9299"),
                    Name = longLine,
                    Genre = Genres.Action,
                    ReleaseDate = new DateTime(2010, 1, 1),
                    Rating = 50,
                    HasMultiplayer = false
                }
            };
        }

        private static async Task CreateFile(string file, IEnumerable<Videogame> videogames)
        {
            var content = new StringBuilder();
            foreach (var videogame in videogames)
            {
                content.Append($"{videogame}\n");
            }

            await File.WriteAllTextAsync(file, content.ToString());
        }

        private static void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}