using UnityEngine;
using TMPro;

public struct Move
{
    public Word word;
    public int score;
    public string username;

    public Move (Word word, string username)
    {
        this.word = word;
        this.username = username;

        //calulate score from connected words
        score = 0;
        foreach (Word w in word.SecondaryWords)
            score += w.CalculateIndividualScore();
        
        score += word.CalculateIndividualScore();
    }

    public Move (Word word, string username, int score)
    {
        this.word = word;
        this.username = username;
        this.score = score;
    }


    public string Serialize()
    {
        return word.letters + "," + word.startingSpace.x + "," + word.startingSpace.y + "," + word.horizontal + "," + score + "," + username;
    }

    public static Move Deserialize(string serialized)
    {
        string[] split = serialized.Split(',');
        if(split.Length != 6) throw new System.Exception("Invalid move serialization");
        return new Move(new Word(split[0], new Vector2Int(int.Parse(split[1]), int.Parse(split[2])), bool.Parse(split[3])), split[5], int.Parse(split[4]));
    }
}