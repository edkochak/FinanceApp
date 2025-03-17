using Xunit;
using FinanceApp.Services.Import;
using System;
using System.IO;
using System.Reflection;
using FinanceApp.Util;

namespace FinanceApp.Tests
{
    public class YamlImportTests
    {
        [Fact]
        public void YamlImport_ShouldHandleEmptyInput()
        {
            // Arrange
            var importer = new YamlImport();
            var parseDataMethod = typeof(YamlImport).GetMethod("ParseData", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Act
            var result = parseDataMethod.Invoke(importer, new object[] { "" });
            
            // Assert
            var list = result as System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>;
            Assert.NotNull(list);
            Assert.Empty(list);
        }
        
        [Fact]
        public void YamlImport_ShouldParseSimpleYaml()
        {
            // Arrange
            var importer = new YamlImport();
            var parseDataMethod = typeof(YamlImport).GetMethod("ParseData", BindingFlags.NonPublic | BindingFlags.Instance);
            
            string yaml = @"---
key1: value1
key2: value2
---
name: Test
type: BankAccount";
            
            // Act
            var result = parseDataMethod.Invoke(importer, new object[] { yaml });
            
            // Assert
            var list = result as System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>;
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
            
            Assert.Equal("value1", list[0]["key1"]);
            Assert.Equal("value2", list[0]["key2"]);
            
            Assert.Equal("Test", list[1]["name"]);
            Assert.Equal("BankAccount", list[1]["type"]);
        }
        
        [Fact]
        public void YamlImport_ShouldIgnoreCommentsAndEmptyLines()
        {
            // Arrange
            var importer = new YamlImport();
            var parseDataMethod = typeof(YamlImport).GetMethod("ParseData", BindingFlags.NonPublic | BindingFlags.Instance);
            
            string yaml = @"---
# This is a comment
key1: value1

# Another comment
key2: value2";
            
            // Act
            var result = parseDataMethod.Invoke(importer, new object[] { yaml });
            
            // Assert
            var list = result as System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>;
            Assert.NotNull(list);
            Assert.Single(list);
            Assert.Equal(2, list[0].Count);
        }
        
        [Fact]
        public void YamlImport_ProcessData_ShouldHandleEmptyList()
        {
            // Arrange
            var importer = new YamlImport();
            var processDataMethod = typeof(YamlImport).GetMethod("ProcessData", BindingFlags.NonPublic | BindingFlags.Instance);
            var emptyList = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>();
            
            var originalOutput = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Act & Assert - no exception should be thrown
                var exception = Record.Exception(() => processDataMethod.Invoke(importer, new object[] { emptyList }));
                Assert.Null(exception);
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }
        
        [Fact]
        public void YamlImport_ProcessData_ShouldHandleObjectsWithoutType()
        {
            // Arrange
            var importer = new YamlImport();
            var processDataMethod = typeof(YamlImport).GetMethod("ProcessData", BindingFlags.NonPublic | BindingFlags.Instance);
            
            var list = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>
            {
                new System.Collections.Generic.Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                }
            };
            
            // Act & Assert - no exception should be thrown
            var exception = Record.Exception(() => processDataMethod.Invoke(importer, new object[] { list }));
            Assert.Null(exception);
        }
    }
}
