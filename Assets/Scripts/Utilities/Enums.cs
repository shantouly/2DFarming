using System;

[Flags]
public enum itemType
{
	None = 0,              // 0 (无任何标志)
	Seed = 1,              // 1 << 0
	Commodity = 2,         // 1 << 1
	Furniture = 4,         // 1 << 2
	HoeTool = 8,           // 1 << 3
	ChopTool = 16,         // 1 << 4
	BreakTool = 32,        // 1 << 5
	ReapTool = 64,         // 1 << 6
	WaterTool = 128,       // 1 << 7
	CollectTool = 256,     // 1 << 8
	ReapableScenery = 512  // 1 << 9
}


public enum slotType
{
	Bag,NPC,Shop,Box
}

public enum InventoryLocation
{
	Player,Box
}

public enum PartType
{
	None,Carry,Hoe,Break,water,Collect,Chop,Reap
}

public enum PartName
{
	Body,Hair,Arm,Tool
}

public enum Season
{
	春天,夏天,秋天,冬天
}

public enum GridType
{
	Diggable,DropItem,PlaceFurniture,NPCObstacle
}

public enum ParticleEffectType
{
	None,LeavesFalling01,LeavesFalling02,Rock,ReapableScenery
}

public enum GameState
{
	GamePlay,GamePause
}

public enum LightShift
{
	Morning,Night
}

public enum SoundName
{
    none,FootStepSoft,FootStepHard,
    Axe,Pickaxe,Hoe,Reap,Water,Basket,Chop,
    Pickup,Plant,TreeFalling,Rustle,
    AmbientCountryside1, AmbientCountryside2,MusicCalm1, MusicCalm2, MusicCalm3, MusicCalm4, MusicCalm5, MusicCalm6,AmbientIndoor1
}
