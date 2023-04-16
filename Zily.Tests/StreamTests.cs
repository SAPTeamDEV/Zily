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
    }
}