using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PlayFrame.Systems.SaveSystem;

namespace PlayFrame.Tests.EditMode.Systems
{
    [TestFixture]
    public class SaveManagerTests
    {
        private SaveManager _saveManager;

        [SetUp]
        public void Setup()
        {
            // Ignore EventManager DontDestroyOnLoad errors in EditMode tests
            // (SaveManager triggers events which try to create EventManager)
            LogAssert.ignoreFailingMessages = true;

            // Clean up before each test
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Get SaveManager instance
            _saveManager = SaveManager.Instance;

            // Re-enable log assertions after setup
            LogAssert.ignoreFailingMessages = false;
        }

        [TearDown]
        public void Teardown()
        {
            if (_saveManager != null)
            {
                _saveManager.DeleteSave();
            }
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        #region Save/Load Tests

        [Test]
        public void SaveManager_ShouldCreateNewSaveData_WhenNoSaveExists()
        {
            _saveManager.LoadGame();

            Assert.NotNull(_saveManager.CurrentSaveData, "SaveData should not be null");
            Assert.AreEqual(0, _saveManager.CurrentSaveData.totalScore, "Initial total score should be 0");
            Assert.AreEqual(0, _saveManager.CurrentSaveData.highScore, "Initial high score should be 0");
        }

        [Test]
        public void SaveManager_ShouldSaveAndLoadData_Successfully()
        {
            int expectedScore = 100;
            _saveManager.CurrentSaveData.totalScore = expectedScore;

            _saveManager.SaveGame();
            _saveManager.LoadGame();

            Assert.AreEqual(expectedScore, _saveManager.CurrentSaveData.totalScore,
                "Loaded score should match saved score");
        }

        [Test]
        public void SaveManager_HasSaveData_ShouldReturnTrue_WhenSaveExists()
        {
            _saveManager.SaveGame();

            bool hasSave = _saveManager.HasSaveData();

            Assert.IsTrue(hasSave, "HasSaveData should return true when save exists");
        }

        [Test]
        public void SaveManager_HasSaveData_ShouldReturnFalse_WhenNoSaveExists()
        {
            _saveManager.DeleteSave();

            bool hasSave = _saveManager.HasSaveData();

            Assert.IsFalse(hasSave, "HasSaveData should return false when no save exists");
        }

        [Test]
        public void SaveManager_DeleteSave_ShouldClearAllData()
        {
            _saveManager.CurrentSaveData.totalScore = 500;
            _saveManager.SaveGame();

            _saveManager.DeleteSave();

            Assert.IsFalse(_saveManager.HasSaveData(), "Save data should not exist after deletion");
            Assert.AreEqual(0, _saveManager.CurrentSaveData.totalScore,
                "Total score should be reset to 0 after deletion");
        }

        #endregion

    }
}
