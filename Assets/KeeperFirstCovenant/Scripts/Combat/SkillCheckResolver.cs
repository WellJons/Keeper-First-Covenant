using UnityEngine;

namespace KeeperFirstCovenant.Combat
{
    public enum SkillCheckGrade
    {
        Failure,
        Success,
        Mastery
    }

    public readonly struct SkillCheckResult
    {
        public readonly int Score;
        public readonly int Difficulty;
        public readonly int Margin;
        public readonly SkillCheckGrade Grade;

        public bool Success =>
            Grade != SkillCheckGrade.Failure;

        public SkillCheckResult(
            int score,
            int difficulty,
            int margin,
            SkillCheckGrade grade)
        {
            Score = score;
            Difficulty = difficulty;
            Margin = margin;
            Grade = grade;
        }
    }

    public static class SkillCheckResolver
    {
        public static SkillCheckResult Resolve(
            int primaryAttribute,
            int difficulty,
            int secondaryAttribute = 10,
            int flatBonus = 0)
        {
            int primary =
                Mathf.Max(
                    1,
                    primaryAttribute);

            int secondaryContribution =
                Mathf.FloorToInt(
                    (secondaryAttribute - 10) *
                    0.5f);

            int score =
                primary +
                secondaryContribution +
                flatBonus;

            int target =
                Mathf.Max(
                    1,
                    difficulty);

            int margin =
                score - target;

            SkillCheckGrade grade =
                margin < 0
                    ? SkillCheckGrade.Failure
                    : margin >= 5
                        ? SkillCheckGrade.Mastery
                        : SkillCheckGrade.Success;

            return new SkillCheckResult(
                score,
                target,
                margin,
                grade);
        }
    }
}
