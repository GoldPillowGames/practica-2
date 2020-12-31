﻿public class Item21 : Item
{
    public override void Start()
    {
        id = 21;
        base.Start();
    }

    // Random Object
    public override void OnPickUpItem(PlayerStatus player)
    {
        player.canRoll = false;
        player.health += player.health * 75 / 100;
    }
}