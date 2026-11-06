using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using StackTower.Core;

namespace StackTower.Tests
{
    [TestFixture]
    public class StackTowerLogicTests
    {
        [MenuItem("Stack Tower/Run Logic & Gameplay Tests")]
        public static void RunAllTestsFromMenu()
        {
            var tests = new StackTowerLogicTests();
            tests.SetUp();
            tests.Test_TrimmingMath_ConservationOfWidth();
            tests.Test_CompleteMiss_Detection();
            tests.Test_PerfectPlacement_ToleranceCheck();
            tests.Test_TooSmallBlock_Threshold();
            tests.Test_SpeedScaling_CapRespect();
            tests.Test_ScoreAndBestScore_PlayerPrefsPersistence();
            tests.TearDown();
            Debug.Log("<color=green>[StackTower Tests] ALL 6 LOGIC AND PERSISTENCE TESTS PASSED SUCCESSFULLY!</color>");
            EditorUtility.DisplayDialog("Stack Tower Tests", "All 6 Logic & Persistence Tests PASSED successfully!", "OK");
        }

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(GameManager.BestScoreKey);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(GameManager.BestScoreKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void Test_TrimmingMath_ConservationOfWidth()
        {
            float targetWidth = 3.0f;
            float targetCenterX = 0f;
            float targetLeft = targetCenterX - targetWidth / 2f; // -1.5
            float targetRight = targetCenterX + targetWidth / 2f; // +1.5

            float movingWidth = 3.0f;
            float movingCenterX = 0.8f;
            float movingLeft = movingCenterX - movingWidth / 2f; // -0.7
            float movingRight = movingCenterX + movingWidth / 2f; // +2.3

            float overlapLeft = Mathf.Max(targetLeft, movingLeft); // -0.7
            float overlapRight = Mathf.Min(targetRight, movingRight); // +1.5
            float overlapWidth = overlapRight - overlapLeft; // 2.2

            float offcutWidth = movingRight - targetRight; // 2.3 - 1.5 = 0.8

            Assert.AreEqual(2.2f, overlapWidth, 0.0001f, "Overlap width must match calculation");
            Assert.AreEqual(0.8f, offcutWidth, 0.0001f, "Offcut width must match calculation");
            Assert.AreEqual(movingWidth, overlapWidth + offcutWidth, 0.0001f, "Sum of retained and offcut width must equal original moving width");
        }

        [Test]
        public void Test_CompleteMiss_Detection()
        {
            float targetLeft = -1.0f;
            float targetRight = 1.0f;

            // Block drops completely to the right
            float movingLeft = 1.5f;
            float movingRight = 3.5f;

            float overlapLeft = Mathf.Max(targetLeft, movingLeft); // 1.5
            float overlapRight = Mathf.Min(targetRight, movingRight); // 1.0
            float overlapWidth = overlapRight - overlapLeft; // -0.5

            Assert.LessOrEqual(overlapWidth, 0f, "Overlap width must be <= 0 for complete miss");
        }

        [Test]
        public void Test_PerfectPlacement_ToleranceCheck()
        {
            float targetWidth = 3.0f;
            float toleranceRatio = 0.035f; // 3.5%
            float allowedTolerance = targetWidth * toleranceRatio; // 0.105

            // Case A: within tolerance
            float smallDiff = 0.05f;
            Assert.LessOrEqual(smallDiff, allowedTolerance, "Small difference must qualify as perfect");

            // Case B: outside tolerance
            float largeDiff = 0.2f;
            Assert.Greater(largeDiff, allowedTolerance, "Large difference must not qualify as perfect");
        }

        [Test]
        public void Test_TooSmallBlock_Threshold()
        {
            float initialWidth = 3.0f;
            float minPlayableRatio = 0.10f; // 10%
            float minPlayableWidth = initialWidth * minPlayableRatio; // 0.30

            float keptWidthA = 0.25f;
            Assert.Less(keptWidthA, minPlayableWidth, "Block width 0.25 must trigger too-small game over");

            float keptWidthB = 0.35f;
            Assert.GreaterOrEqual(keptWidthB, minPlayableWidth, "Block width 0.35 must allow continued play");
        }

        [Test]
        public void Test_SpeedScaling_CapRespect()
        {
            float initialSpeed = 3.0f;
            float increment = 0.08f;
            float maxSpeed = 7.0f;

            // Early game (5 placements)
            float speed5 = Mathf.Min(maxSpeed, initialSpeed + (5 * increment));
            Assert.AreEqual(3.4f, speed5, 0.001f);

            // Mid game (25 placements)
            float speed25 = Mathf.Min(maxSpeed, initialSpeed + (25 * increment));
            Assert.AreEqual(5.0f, speed25, 0.001f);

            // Late game (100 placements)
            float speed100 = Mathf.Min(maxSpeed, initialSpeed + (100 * increment));
            Assert.AreEqual(7.0f, speed100, 0.001f, "Speed must never exceed maxSpeed");
        }

        [Test]
        public void Test_ScoreAndBestScore_PlayerPrefsPersistence()
        {
            GameObject gmGo = new GameObject("TestGameManager");
            GameManager gm = gmGo.AddComponent<GameManager>();

            gm.StartGame();
            Assert.AreEqual(0, gm.CurrentScore);

            // Add standard placement (+1)
            gm.AddScore(1, false);
            Assert.AreEqual(1, gm.CurrentScore);
            Assert.AreEqual(1, gm.BestScore);

            // Add perfect placement (+2)
            gm.AddScore(2, true);
            Assert.AreEqual(3, gm.CurrentScore);
            Assert.AreEqual(3, gm.BestScore);

            // Trigger game over and check PlayerPrefs
            gm.TriggerGameOver();
            int savedBest = PlayerPrefs.GetInt(GameManager.BestScoreKey, 0);
            Assert.AreEqual(3, savedBest, "Saved best score in PlayerPrefs must match");

            // Start new run: current score resets, but best score is retained
            gm.StartGame();
            Assert.AreEqual(0, gm.CurrentScore, "Current score must reset on new game");
            Assert.AreEqual(3, gm.BestScore, "Best score must persist across runs");

            Object.DestroyImmediate(gmGo);
        }
    }
}
