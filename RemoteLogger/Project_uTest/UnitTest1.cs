using ConsoleApp1;

namespace Project_uTest
{
    public class Tests
    {
        LoggerClient.LoggerClient _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _client = LoggerClient.LoggerClient.Instance;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        [Test]
        public void Test1()
        {
            _client.SendContent("data1.csv", "kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
            _client.SendContent("data2.csv", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            _client.SendContent("data3.csv", "wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww");
            _client.SendContent("data4.csv", "qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq");

            _client.SendContent("data1.csv", "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            _client.SendContent("data2.csv", "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW");
            _client.SendContent("data3.csv", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            _client.SendContent("data4.csv", "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            Thread.Sleep(4000);
            Assert.Pass();
        }
    }
}