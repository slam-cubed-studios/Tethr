using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int level;
    public int coins;

    public PlayerData(TestPlayer player)
    {
        this.level = player.level;
        this.coins = player.coins;
    }
}
