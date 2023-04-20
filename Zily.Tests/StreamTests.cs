using Serilog;

namespace SAPTeam.Zily.Tests
{
    public class StreamTests
    {
        public StreamTests()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();
        }

        [Fact]
        public void IOTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            Assert.Equal(-1, zs.ReadByte());
            zs.Write("Hello");
            Assert.Equal("Hello", zs.ReadString());
        }

        [Fact]
        public void ParserTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.Write("test");
            Assert.Throws<InvalidOperationException>(() => zs.Parse());

            zs.Write("");
            Assert.Throws<InvalidOperationException>(() => zs.Parse());

            zs.WriteCommand(HeaderFlag.Write);
            Assert.Throws<InvalidOperationException>(() => zs.Parse());

            zs.WriteCommand(HeaderFlag.Unsupported, "test");
            zs.Parse();
            Assert.Throws<Exception>(() => zs.Parse());
        }

        [Fact]
        public void ResponseParserOkTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.WriteCommand(HeaderFlag.Ok);
            var header = zs.ReadHeader();
            var parseResult = zs.ParseResponse(header);
            Assert.True(parseResult);
            Assert.Equal("", zs.ReadString(header.length));
        }

        [Fact]
        public void ResponseParserFailTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.WriteCommand(HeaderFlag.Fail, "Test exception.");
            var header = zs.ReadHeader();
            try
            {
                zs.ParseResponse(header);
                Assert.Fail("This code block must throw an Exception.");
            }
            catch(Exception e)
            {
                Assert.Equal("Test exception.", e.Message);
            }
        }

        [Fact]
        public void ResponseParserEmptyFailTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.WriteCommand(HeaderFlag.Fail);
            var header = zs.ReadHeader();
            try
            {
                zs.ParseResponse(header);
                Assert.Fail("This code block must throw an Exception.");
            }
            catch (Exception e)
            {
                Assert.Equal("", e.Message);
            }
        }

        [Fact]
        public void ResponseParserWriteTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.Write("test");
            var header = zs.ReadHeader();
            var parseResult = zs.ParseResponse(header);
            Assert.False(parseResult);
            Assert.Equal("test", zs.ReadString(header.length));
        }

        [Fact]
        public void VersionTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.WriteCommand(HeaderFlag.Version);
            zs.Parse();
            zs.Parse();
            Assert.Equal("2.0", zs.StreamVersion.ToString());
        }
    }
}