namespace SAPTeam.Zily.Tests
{
    public class StreamTests
    {
        [Fact]
        public void IOTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.WriteString("Hello");
            Assert.Equal("Hello", zs.ReadString());
        }

        [Fact]
        public void ParserTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.WriteString("test");
            Assert.Throws<InvalidOperationException>(() => zs.Parse());

            zs.WriteString("");
            Assert.Throws<InvalidOperationException>(() => zs.Parse());

            zs.WriteString(HeaderFlag.Write);
            Assert.Throws<InvalidOperationException>(() => zs.Parse());

            zs.WriteString(HeaderFlag.Unsupported, "test");
            Assert.Throws<ArgumentException>(() => zs.Parse());
        }

        [Fact]
        public void ResponseParserOkTest()
        {
            var ms = new MemoryStream();
            var zs = new ZilyStream(ms);

            zs.WriteString(HeaderFlag.Ok);
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

            zs.WriteString(HeaderFlag.Fail, "Test exception.");
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

            zs.WriteString(HeaderFlag.Fail);
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

            zs.WriteString("test");
            var header = zs.ReadHeader();
            var parseResult = zs.ParseResponse(header);
            Assert.False(parseResult);
            Assert.Equal("test", zs.ReadString(header.length));
        }
    }
}