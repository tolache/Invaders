using Xunit;
using Invaders.Model;

namespace InvadersTests
{
    public class Tests
    {
        [Fact]
        public void GameStartsWithWaveOne()
        {
            InvadersModel model = new InvadersModel();
            model.StartGame();
            Assert.Equal(1, model.Wave);
        }
    }
}
