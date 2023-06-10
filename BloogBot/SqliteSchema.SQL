CREATE TABLE IF NOT EXISTS [main].[BlacklistedMobs](
	[Guid] [text] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);
 
CREATE TABLE IF NOT EXISTS [main].[Commands](
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL,
	[Command] [text] NOT NULL,
	[Player] [text] NOT NULL,
	[Args] [text] NULL
);

CREATE TABLE IF NOT EXISTS [main].[Hotspots](
	[Zone] [text] NOT NULL,
	[Description] [text] NOT NULL,
	[Faction] [text] NOT NULL,
	[Waypoints] [text] NOT NULL,
	[InnkeeperId] [int] NULL,
	[RepairVendorId] [int] NULL,
	[AmmoVendorId] [int] NULL,
	[MinimumLevel] [int] NOT NULL,
	[TravelPathId] [int] NULL,
	[SafeForGrinding] [int] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[HotspotsV2](
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL,
	[Polyn] [text] NOT NULL,
	[Zone] [text] NOT NULL,
	[Description] [text] NOT NULL,
	[Faction] [text] NOT NULL,
	[MinimumLevel] [int] NOT NULL,
	[TravelPathId] [int] NULL,
	[TownId] [int] NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[Kills](
	[PlayerLevel] [int] NOT NULL,
	[PlayerName] [text] NOT NULL,
	[PlayerClass] [text] NOT NULL,
	[EnemyName] [text] NOT NULL,
	[EnemyLevel] [int] NOT NULL,
	[EnemyType] [text] NOT NULL,
	[HealthPercentLost] [int] NOT NULL,
	[CombatDuration] [int] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[Npcs](
	[Name] [text] NOT NULL,
	[IsInnkeeper] [bit] NOT NULL,
	[SellsAmmo] [bit] NOT NULL,
	[Repairs] [bit] NOT NULL,
	[Quest] [bit] NOT NULL,
	[Horde] [bit] NOT NULL,
	[Alliance] [bit] NOT NULL,
	[PositionX] [numeric](18, 0) NOT NULL,
	[PositionY] [numeric](18, 0) NOT NULL,
	[PositionZ] [numeric](18, 0) NOT NULL,
	[Zone] [text] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[QuestCompletions](
	[PlayerGuid] [text] NOT NULL,
	[QuestId] [int] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[QuestHubs](
	[Name] [text] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[QuestObjectives](
	[Type] [text] NOT NULL,
	[Name] [text] NOT NULL,
	[Source] [text] NOT NULL,
	[Quantity] [int] NOT NULL,
	[QuestId] [int] NOT NULL,
	[ObjectId] [int] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[QuestPrerequisites](
	[ParentId] [int] NOT NULL,
	[ChildId] [int] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[Quests](
	[Name] [text] NOT NULL,
	[QuestHubId] [int] NOT NULL,
	[HotspotId] [int] NULL,
	[StartNpcId] [int] NOT NULL,
	[EndNpcId] [int] NOT NULL,
	[RequiredLevel] [int] NOT NULL,
	[Class] [text] NOT NULL,
	[QuestId] [int] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[ReportSignatures](
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL,
	[Player] [text] NOT NULL,
	[CommandId] [int] NOT NULL
);

CREATE TABLE IF NOT EXISTS [main].[Towns](
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL,
	[InnkeeperId] [int] NULL,
	[RepairVendorId] [int] NULL,
	[AmmoVendorId] [int] NULL,
	[Name] [text] NULL
);

CREATE TABLE IF NOT EXISTS [main].[TravelPaths](
	[Name] [text] NOT NULL,
	[Waypoints] [text] NOT NULL,
	[Id] [integer] PRIMARY KEY AUTOINCREMENT NOT NULL
);

