using UnityEngine;

public class QuestFactory
{
    public Quest CreateQuest(int questId, int difficultyLevel)
    {
        if (difficultyLevel < 0)
        {
            difficultyLevel = DetermineDifficultyLevel();
        }

        return new Quest(difficultyLevel);
    }

    private int DetermineDifficultyLevel()
    {
        return 1; //TODO: implement logic to determine default difficulty level
    }
}
