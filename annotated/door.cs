public class Door : MonoBehaviour
{
	public delegate void DoorOpenedHandler();

	public DoorOpenedHandler OnDoorOpened;

	private static List<DoorOpeningData> openedDoorIDsHistory = new List<DoorOpeningData>();

	private static List<Door> openableDoors = new List<Door>();

	[SerializeField]
	private DoorCursorTarget roof;

	[SerializeField]
	private OffscreenMarker.OffscreenMarkerData doorOpenerIconData;

	[SerializeField]
	private float openingTime;

	[SerializeField]
	private GameObject hitVFXPfb;

	[SerializeField]
	private GameObject brokenVFXPfb;

	[SerializeField]
	private SpriteAnimationRuntime2 bonusAnim;

	[SerializeField]
	private Transform roomSelectableContainerTfm;

	private HeroMobCommon opener;

	private List<Hero> waitingForOpeningHeroes = new List<Hero>();

	private Bounds bounds;

	private Vector3 openingVelocity;

	private Dungeon dungeon;

	private AudioEmitter audioEmitter;

	private Room openingRoom;

	private bool triggerEvents;

	private bool instantOpen;

	private UniqueID uniqueID;

	private RoomElement roomElement;

	private AITarget aiTarget;

	private bool shouldInitTarget = true;

	private List<Mob> attackers;

	private OffscreenMarker offscreenMarker;

	private Vector3 hpBarInitialPos;

	private Vector3 hpBarInitialScale;

	private GameNetworkManager gameNetManager;

	private ParticleSystem hitVFX;

	private Room nextRoom;

	private Mob mobOpener;

	private SpriteAnimDir attackerDirection;

	private DoorSelectable doorSelectable;

	public static int MultiDoorCount = 0;

	public static List<DoorOpeningData> OpenedDoorIDsHistory => openedDoorIDsHistory;

	public static List<Door> OpenableDoors
	{
		get
		{
			return openableDoors;
		}
		set
		{
			openableDoors = value;
		}
	}

	public float RoomAngle
	{
		get;
		set;
	}

	public Vector3 OpeningDir
	{
		get;
		set;
	}

	public Room Room1
	{
		get;
		private set;
	}

	public Room Room2
	{
		get;
		private set;
	}

	public Health HealthCpnt
	{
		get;
		private set;
	}

	public bool IsOpening
	{
		get;
		private set;
	}

	public DoorStep DoorStep
	{
		get;
		private set;
	}

	public UniqueIDNetSyncElement NetSyncElement
	{
		get;
		private set;
	}

	/// <summary>
	/// Door initiation
	/// </summary>
	/// <param name="room1">Room on one side of the door</param>
	/// <param name="room2">Room on the the other side</param>
	/// <param name="openingDir">Direction the door opens (right/left/up/down)</param>
	/// <param name="doorStep">Unclear, 'step' here might relate to game 'tick'</param>
	public void Init(Room room1, Room room2, Vector3 openingDir, DoorStep doorStep)
	{
		uniqueID.RequestUniqueID();
		Room1 = room1;
		Room2 = room2;
		OpeningDir = openingDir;
		DoorStep = doorStep;
		attackers = new List<Mob>();
		
		// OnHit event subscription
		HealthCpnt.OnHit += OnHit;
		
		Room1.AddDoor(this);
		Room2.AddDoor(this);
		roomSelectableContainerTfm.rotation = Quaternion.Euler(270f * Vector3.right);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="revealedRoom"></param>
	private void InitAITarget(Room revealedRoom)
	{
		
		// Unclear why setting the origin room as the parent room is important
		if (roomElement != null)
		{
			roomElement.SetParentRoom(revealedRoom);
		}
		
		// Set the AI target to this door
		if (aiTarget != null)
		{
			aiTarget.Init();
			// event subscription
			aiTarget.OnActorChanged += OnAITargetActorChange;
		}
		
		// Not sure about this null check, it might be redundant. HealthCpnt is probably instantiated by the dungeon
		// generator, and if the generator is working properly, this should never be null. 
		// Anyways, init the door's health bar and render. 
		if (HealthCpnt != null)
		{
			HealthCpnt.InitHealth();
			
			// event subscription
			HealthCpnt.OnDeath += OnDeath;
			hpBarInitialPos = HealthCpnt.HealthBar.BarContainer.transform.localPosition;
			hpBarInitialScale = HealthCpnt.HealthBar.BarContainer.transform.localScale;
			HealthCpnt.HitRenderers = GetComponentsInChildren<Renderer>();
		}
		shouldInitTarget = false;
	}

	/// <summary>
	/// Hide the Door object and sprite
	/// </summary>
	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		DoorStep.gameObject.SetActive(value: false);
	}

	/// <summary>
	/// Shows the Door object and sprite. This seems to be called when another door is opened. 
	/// </summary>
	/// <param name="callingRoom">Parent Room</param>
	public void Show(Room callingRoom = null)
	{
		// if the object isn't already active
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
			DoorStep.gameObject.SetActive(value: true);
			doorSelectable.Register();
			if (shouldInitTarget)
			{
				InitAITarget(callingRoom);
			}
			openableDoors.Add(this);
		}
	}

	/// <summary>
	/// Used in door opening animation transform 
	/// </summary>
	/// <param name="openerPos">Unclear</param>
	/// <returns></returns>
	public Transform GetOpeningSpot(Vector3 openerPos)
	{
		Transform destSpot = DoorStep.DestSpot1;
		Transform destSpot2 = DoorStep.DestSpot2;
		Room uniqueAlreadyOpenRoom = GetUniqueAlreadyOpenRoom();
		if (uniqueAlreadyOpenRoom != null)
		{
			if (Vector3.Dot(destSpot.forward, DoorStep.transform.position - uniqueAlreadyOpenRoom.transform.position) < 0f)
			{
				return destSpot;
			}
			return destSpot2;
		}
		if (Vector3.Dot(destSpot.forward, DoorStep.transform.position - openerPos) < 0f)
		{
			return destSpot;
		}
		return destSpot2;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public Room GetUniqueAlreadyOpenRoom()
	{
		if (Room1.IsVisible && !Room2.IsVisible)
		{
			return Room1;
		}
		if (Room2.IsVisible && !Room1.IsVisible)
		{
			return Room2;
		}
		return null;
	}

	public Room GetRoomToOpen(Vector3 openerPosition)
	{
		if (Room1.IsVisible && !Room2.IsVisible)
		{
			return Room2;
		}
		if (Room2.IsVisible && !Room1.IsVisible)
		{
			return Room1;
		}
		Transform openingSpot = GetOpeningSpot(openerPosition);
		if (Vector3.Dot(openingSpot.forward, DoorStep.transform.position - Room1.transform.position) > 0f)
		{
			return Room1;
		}
		return Room2;
	}

	/// <summary>
	/// Simple check to see if a door is already opening
	/// </summary>
	/// <param name="checkIsOpening">As far as I can tell, this is always true</param>
	/// <returns>true if door can be opened, false otherwise</returns>
	private bool CanBeOpened(bool checkIsOpening = true)
	{
		if (checkIsOpening && IsOpening)
		{
			Diagnostics.LogError(base.name + " > Cannot be opened: door already opening!");
			return false;
		}
		return true;
	}

	public void OpenByHeroOrMob(Room openingRoom, HeroMobCommon opener, bool instantOpen = false, bool checkIsOpening = true)
	{
		if (CanBeOpened(checkIsOpening))
		{
			NetSyncElement.SendRPCToServer(UniqueIDRPC.Door_RequestOpenByHeroOrMob, (!(openingRoom != null)) ? (-1) : openingRoom.UniqueID.ID, opener.UniqueID.GetCategory(), opener.UniqueID.ID, instantOpen, checkIsOpening);
		}
	}

	private void RPC_RequestOpenByHeroOrMob(int openingRoomID, StaticString openerCategory, int openerID, bool instantOpen, bool checkIsOpening)
	{
		if (!gameNetManager.IsServer())
		{
			Diagnostics.LogError("Door.RPC_RequestOpenByHeroOrMob should only be called on server side!");
		}
		else if (!CanBeOpened(checkIsOpening))
		{
			Diagnostics.LogWarning(base.name + " > Door.RPC_RequestOpenByHeroOrMob: NOPE!");
		}
		else
		{
			NetSyncElement.SendRPCToAll(UniqueIDRPC.Door_DoOpenByHeroOrMob, openingRoomID, openerCategory, openerID, instantOpen, checkIsOpening);
		}
	}

	private void RPC_DoOpenByHeroOrMob(int openingRoomID, StaticString openerCategory, int openerID, bool instantOpen, bool checkIsOpening)
	{
		if (!CanBeOpened(checkIsOpening))
		{
			Diagnostics.LogError(base.name + " > Door.RPC_DoOpenByHeroOrMob: CANNOT BE OPENED!");
			return;
		}
		Room room = null;
		if (openingRoomID > 0)
		{
			room = UniqueIDManager.Get<Room>(openingRoomID);
		}
		if (room == null)
		{
			Diagnostics.LogError("RPC_DoOpenByHeroOrMob: openingRoom is null!");
		}
		HeroMobCommon heroMobCommon = (HeroMobCommon)UniqueIDManager.Get(openerCategory, openerID);
		if (heroMobCommon == null)
		{
			Diagnostics.LogError("RPC_DoOpenByHeroOrMob: Can't find opener {0} of category {1}", openerID, openerCategory);
			return;
		}
		Open(room, heroMobCommon, applyFIDSIncome: true, instantOpen, incrementOpenedDoorsStat: true, room != null && !room.IsFullyOpened && heroMobCommon.NetSyncElement.IsOwnedByLocalPlayer(), checkIsOpening);
		Hero hero = heroMobCommon as Hero;
		if (hero != null)
		{
			dungeon.CheckSituationDialog(SituationDialogType.OpenDoor, hero);
			if (TutorialManager.IsEnable)
			{
				Services.GetService<IGameEventService>()?.TriggerDoorOpenedByHeroTutorialEvent();
			}
		}
	}

	/// <summary>
	/// Opens dungeon doors on save restore
	/// </summary>
	/// <param name="openingRoom">The room to open</param>
	public void OpenForSaveRestore(Room openingRoom)
	{
		Open(openingRoom, null, applyFIDSIncome: false, instantOpen: true, incrementOpenedDoorsStat: false, triggerEvents: false);
	}

	/// <summary>
	/// Opens a door for the crystal phase
	/// </summary>
	/// <param name="orderingRoom">The room to be opened</param>
	public void OpenForCrystalPhase(Room orderingRoom)
	{
		if (!IsOpening)
		{
			// error checking
			if (dungeon.CurrentCrystalState != CrystalState.Unplugged)
			{
				Diagnostics.LogError("{0} > Door.OpenForCrystalPhase: shouldn't be called when crystal is not unplugged ({1})!", base.name, dungeon.CurrentCrystalState);
				return;
			}
			Room roomToOpen = GetRoomToOpen(orderingRoom.transform.position);
			// Sync with all players
			NetSyncElement.SendRPCToAll(UniqueIDRPC.Door_DoOpenForCrystalPhase, roomToOpen.UniqueID.ID);
		}
	}

	/// <summary>
	/// RPC for opening doors during the crystal phase. Unclear how this is used. I can't find any calls to the method. 
	/// </summary>
	/// <param name="openingRoomID">The id of the room to open</param>
	private void RPC_DoOpenForCrystalPhase(int openingRoomID)
	{
		//error checking
		if (dungeon.CurrentCrystalState != CrystalState.Unplugged)
		{
			Diagnostics.LogError("{0} > Door.RPC_DoOpenForCrystalPhase: shouldn't be called when crystal is not unplugged ({1})!", base.name, dungeon.CurrentCrystalState);
		}
		else
		{
			//open the door
			StartCoroutine(DoOpenForCrystalPhaseCoroutine(openingRoomID));
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="openingRoomID"></param>
	/// <returns></returns>
	private IEnumerator DoOpenForCrystalPhaseCoroutine(int openingRoomID)
	{
		if (dungeon.CurrentCrystalState != CrystalState.Unplugged)
		{
			Diagnostics.LogError("{0} > Door.DoOpenForCrystalPhaseCoroutine: shouldn't be called when crystal is not unplugged ({1})!", base.name, dungeon.CurrentCrystalState);
			yield break;
		}
		GameConfig gameCfg = GameConfig.GetGameConfig();
		yield return new WaitForSeconds(RandomGenerator.RangeFloat(gameCfg.CrystalPhaseDoorOpeningDelayMin.GetValue(), gameCfg.CrystalPhaseDoorOpeningDelayMax.GetValue()));
		Room openingRoom = UniqueIDManager.Get<Room>(openingRoomID);
		Open(openingRoom, null, applyFIDSIncome: false, instantOpen: false, incrementOpenedDoorsStat: false, !openingRoom.IsFullyOpened && gameNetManager.IsServerOrSinglePlayer());
	}

	/// <summary>
	/// Primary method for opening a door. 
	/// </summary>
	/// <param name="openingRoom">Room the door is opening from</param>
	/// <param name="opener">The agent that is opening the door</param>
	/// <param name="applyFIDSIncome">Bool indicating whether income should be allocated</param>
	/// <param name="instantOpen">Unclear what true indicates here and what it means for a door to instantly open</param>
	/// <param name="incrementOpenedDoorsStat">Parameter for whether the lifetime number of doors should be incremented
	/// (For achievement purposes)</param>
	/// <param name="triggerEvents">Whether events should be triggered on open</param>
	/// <param name="checkIsOpening">Whether the method should check if the door is currently being opened</param>
	private void Open(Room openingRoom = null, HeroMobCommon opener = null, bool applyFIDSIncome = true, bool instantOpen = false, bool incrementOpenedDoorsStat = true, bool triggerEvents = true, bool checkIsOpening = true)
	{
		
		//error checking
		if (checkIsOpening && IsOpening)
		{
			Diagnostics.LogError("Unable to open door, door already opening!");
			return;
		}
		if (dungeon.CurrentCrystalState == CrystalState.PluggedOnExitSlot)
		{
			Diagnostics.LogError("{0} > Door.Open: shouldn't be called when crystal has been plugged on exit slot!", base.name, dungeon.CurrentCrystalState);
			return;
		}
		if (dungeon.CurrentCrystalState == CrystalState.Plugged)
		{
			MultiDoorCount++;
		}
		this.opener = opener;
		if (dungeon == null)
		{
			dungeon = SingletonManager.Get<Dungeon>();
		}
		
		// get the game phase 
		GamePhase currentGamePhase = dungeon.CurrentGamePhase;
		this.instantOpen = instantOpen;
		
		// allocate income to the player
		if (applyFIDSIncome)
		{
			applyFIDSIncome = (!gameNetManager.IsMultiplayerSession() || (MultiplayerConfig.SplitFISIncome && MultiplayerConfig.SplitFIS) || this.opener.NetSyncElement.IsOwnedByLocalPlayer());
			if (applyFIDSIncome)
			{
				dungeon.StartCoroutine(dungeon.ApplyFIDSIncome());
			}
		}
		
		// add event listeners
		Room1.OnRoomPowerChanged += Room2.OnAdjacentRoomPowerChanged;
		Room2.OnRoomPowerChanged += Room1.OnAdjacentRoomPowerChanged;
		this.openingRoom = openingRoom;
		this.triggerEvents = triggerEvents;
		if (this.openingRoom != null)
		{
			this.openingRoom.Open(this);
			if (dungeon.OpeningDoorCount == 0 && opener != null && opener is Hero && (opener as Hero).NetSyncElement.IsOwnedByLocalPlayer())
			{
				this.openingRoom.SelectableForMove.Select();
			}
		}
		bounds = GetComponent<Collider>().bounds;
		if (this.instantOpen)
		{
			OnDoorOpeningMoveFinished();
		}
		else
		{
			Invoke("OnDoorOpeningMoveFinished", openingTime);
			Vector3 tileSize = dungeon.GetTileSize();
			openingVelocity = tileSize.x * OpeningDir.MultiplyBy(dungeon.ContainerScale) / openingTime;
			base.transform.localScale *= 0.99f;
			audioEmitter.PlayEvent(GameConfig.GetGameConfig().GetTilesetConfig().DoorOpeningSFXPath);
		}
		if (aiTarget != null)
		{
			aiTarget.SetActive(active: false);
			if (offscreenMarker != null)
			{
				offscreenMarker.Hide();
			}
		}
		IsOpening = true;
		openableDoors.Remove(this);
		if (openableDoors.Count < 1)
		{
			Diagnostics.Log("Last door just opened");
			string key = "%Notification_LastDoorOpened";
			if (dungeon.CurrentCrystalState != 0)
			{
				key = "%Notification_LastDoorOpened_CrystalPhase";
			}
			dungeon.EnqueueNotification(AgeLocalizer.Instance.LocalizeString(key), null, "LastDoor");
			IGameEventService service = Services.GetService<IGameEventService>();
			Diagnostics.Assert(service != null);
			service.TriggerLastDoorOpenedEvent();
		}
		dungeon.OpeningDoorCount++;
		dungeon.UpdateGamePhase();
		DoorOpeningData item = default(DoorOpeningData);
		item.DoorID = uniqueID.ID;
		item.OpeningRoomID = ((!(openingRoom != null)) ? (-1) : openingRoom.UniqueID.ID);
		openedDoorIDsHistory.Add(item);
		if (incrementOpenedDoorsStat)
		{
			dungeon.Statistics.IncrementStat((currentGamePhase != GamePhase.Action) ? DungeonStatistics.Stat_OpenedDoors : DungeonStatistics.Stat_ActionOpenedDoors);
			AchievementManagerDOTE achievementManagerDOTE = SingletonManager.Get<AchievementManagerDOTE>();
			achievementManagerDOTE.IncrementStatistic(StatisticName.DOORS_OPENED);
			if (Analytics.Instance != null)
			{
				(Analytics.Instance as GoogleAnalyticsManager).SendNewUserMetrics(achievementManagerDOTE.GetStatisticValue(StatisticName.DOORS_OPENED) == 1f);
			}
		}
		bonusAnim.transform.eulerAngles = Vector3.up * 180f;
		bonusAnim.transform.position = roof.transform.position + bonusAnim.transform.localPosition;
		bonusAnim.transform.parent = base.transform.parent;
		bonusAnim.SetFloat(SpriteAnimationFloat.DoorBonus, 0f);
		bonusAnim.Trigger(SpriteAnimationTrigger.OnMultiDoor);
		if (currentGamePhase == GamePhase.Action && dungeon.CurrentCrystalState == CrystalState.Plugged)
		{
			float value = GameConfig.GetGameConfig().ActionDoorFISBonus.GetValue();
			value *= (float)(MultiDoorCount - 1);
			switch (RandomGenerator.RangeInt(1, 4))
			{
			case 1:
				Player.LocalPlayer.AddFood(value);
				break;
			case 2:
				Player.LocalPlayer.AddIndustry(value);
				break;
			case 3:
				Player.LocalPlayer.AddScience(value);
				break;
			}
		}
		Services.GetService<IGameCameraService>().Unlock();
		doorSelectable.Unregister();
	}

	/// <summary>
	/// Registers the hero that is opening this door
	/// </summary>
	/// <param name="hero">The name of the hero</param>
	public void RegisterOpener(Hero hero)
	{
		if (!waitingForOpeningHeroes.Contains(hero))
		{
			waitingForOpeningHeroes.Add(hero);
		}
	}

	/// <summary>
	/// Removes the hero that is opening this door
	/// </summary>
	/// <param name="hero">The name of the hero</param>
	public void RemoveOpener(Hero hero)
	{
		waitingForOpeningHeroes.Remove(hero);
	}

	/// <summary>
	/// Door destruction trigger. Mobs can attack doors and destroy them.
	/// </summary>
	/// <param name="attackerOwnerPlayerID"></param>
	public void OnDeath(ulong attackerOwnerPlayerID)
	{
		
		//error checking
		if (attackers == null || attackers.Count == 0)
		{
			Diagnostics.LogError("Door died but no attacker was ever registered.");
		}
		
		// unclear why there is this null check. May be redundant 
		else if (mobOpener == null)
		{
			mobOpener = attackers[0];
			nextRoom = GetRoomToOpen(mobOpener.transform.position);
			float d = 0f;
			switch (attackerDirection)
			{
			case SpriteAnimDir.Down:
				d = 180f;
				break;
			case SpriteAnimDir.Up:
				d = 0f;
				break;
			case SpriteAnimDir.Right:
				d = 90f;
				break;
			case SpriteAnimDir.Left:
				d = 270f;
				break;
			}
			UnityEngine.Object.Instantiate(brokenVFXPfb, base.transform.position + brokenVFXPfb.transform.position, Quaternion.Euler(brokenVFXPfb.transform.eulerAngles + Vector3.up * d));
			instantOpen = true;
			OpenByHeroOrMob(nextRoom, mobOpener, instantOpen: true, checkIsOpening: false);
			dungeon.IncrementTurn();
		}
	}

	/// <summary>
	/// Override of Monobehavoir Awake() method. Sets values for door instantiation when the script is loaded. 
	/// </summary>
	protected void Awake()
	{
		audioEmitter = GetComponent<AudioEmitter>();
		uniqueID = GetComponent<UniqueID>();
		HealthCpnt = GetComponent<Health>();
		NetSyncElement = GetComponent<UniqueIDNetSyncElement>();
		roomElement = GetComponent<RoomElement>();
		aiTarget = GetComponent<AITarget>();
		doorSelectable = GetComponent<DoorSelectable>();
		gameNetManager = SingletonManager.Get<GameNetworkManager>();
	}

	/// <summary>
	///  Override of MonoBehavoir Start(). Sets the dungeon value when the script is run. 
	/// </summary>
	private void Start()
	{
		dungeon = SingletonManager.Get<Dungeon>();
	}

	/// <summary>
	/// Override of MonoBehavoir Update(). Describes the 
	/// </summary>
	private void Update()
	{
		
		if (IsOpening)
		{
			base.transform.position += openingVelocity * Time.deltaTime;
		}
		if (HealthCpnt != null && HealthCpnt.HealthBar.BarContainer.gameObject.activeInHierarchy && attackers.Count == 0)
		{
			HealthCpnt.HealthBar.HideBar();
		}
	}

	private void OnDoorOpeningMoveFinished(AstarPath script)
	{
		OnDoorOpeningMoveFinished();
	}

	private void OnDoorOpeningMoveFinished()
	{
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnDoorOpeningMoveFinished));
		if (!instantOpen)
		{
		}
		if (!instantOpen)
		{
			if (AstarPath.active.IsAnyGraphUpdatesQueued)
			{
				Diagnostics.LogWarning(base.name + " > Waiting for pathfinding update to remove door bounds");
				AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnDoorOpeningMoveFinished));
				return;
			}
			base.gameObject.layer = LayerMask.NameToLayer("Default");
			AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnPathfindingGraphUpdated));
			AstarPath.active.UpdateGraphs(bounds);
		}
		else
		{
			base.gameObject.layer = LayerMask.NameToLayer("Default");
			AstarPath.active.UpdateGraphs(bounds);
			OnPathfindingGraphUpdated();
		}
		if (!instantOpen)
		{
			dungeon.IncrementTurn();
		}
	}

	private void OnPathfindingGraphUpdated(AstarPath script)
	{
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnPathfindingGraphUpdated));
		OnPathfindingGraphUpdated();
	}

	private void OnPathfindingGraphUpdated()
	{
		if (openingRoom != null && !openingRoom.IsFullyOpened)
		{
			openingRoom.InitAgents();
		}
		foreach (Hero waitingForOpeningHero in waitingForOpeningHeroes)
		{
			if (waitingForOpeningHero != null && waitingForOpeningHero.HealthCpnt.IsAlive() && !waitingForOpeningHero.IsDismissing && !waitingForOpeningHero.IsRespawning)
			{
				waitingForOpeningHero.UnlockInteractions();
				waitingForOpeningHero.OnBlockingDoorOpened();
			}
		}
		if (triggerEvents && dungeon.CurrentCrystalState != CrystalState.PluggedOnExitSlot)
		{
			dungeon.StartCoroutine(dungeon.TriggerEvents(openingRoom, opener, opener != null));
		}
		if (OnDoorOpened != null)
		{
			OnDoorOpened();
		}
		dungeon.OpeningDoorCount--;
		dungeon.UpdateGamePhase();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	/// <summary>
	/// Trigger for when a door is hit / attacked.
	/// </summary>
	/// <param name="hit">Attack info, containing damage, id of attacker, etc.</param>
	private void OnHit(AttackInfo hit)
	{
		
		// make sure animation objects are set
		if (hitVFX == null)
		{
			hitVFX = (UnityEngine.Object.Instantiate(hitVFXPfb, Vector3.zero, base.transform.rotation) as GameObject).GetComponent<ParticleSystem>();
		}
		
		// get the attacker direction so we know which direction to animate the door on hit
		attackerDirection = UniqueIDManager.Get(hit.AttackerCategory, hit.AttackerID).GetComponent<Mob>().GetAnimDirection();
		Vector3 vector = Vector3.zero;
		float d = 1.5f;
		
		// then switch on direction
		switch (attackerDirection)
		{
		case SpriteAnimDir.Down:
			vector = Vector3.back * d;
			break;
		case SpriteAnimDir.Up:
			vector = Vector3.forward * d;
			break;
		case SpriteAnimDir.Right:
			vector = Vector3.right;
			break;
		case SpriteAnimDir.Left:
			vector = Vector3.left;
			break;
		}
		
		// set the animation and animate
		Transform transform = hitVFX.transform;
		Vector3 a = base.transform.position + hitVFXPfb.transform.position;
		Vector3 a2 = vector;
		Vector3 extents = GetComponent<Collider>().bounds.extents;
		transform.position = a + a2 * extents.x;
		hitVFX.Play(withChildren: true);
	}

	/// <summary>
	/// Trigger for when a player right clicks a door. More or less calls MoveToDoor. 
	/// </summary>
	/// <param name="clickInfo">Click info (mouse pos., etc)</param>
	private void OnRightClickDown(ClickDownInfo clickInfo)
	{
		if (Hero.SelectedHeroes.Count > 0)
		{
			foreach (Hero selectedHero in Hero.SelectedHeroes)
			{
				selectedHero.MoveToDoor(this);
			}
			// make sure the rightclick doesn't do anything else
			Services.GetService<IInputService>().StopClickEventPropagation();
		}
	}

	/// <summary>
	/// Trigger for when a door is destroyed
	/// </summary>
	private void OnDestroy()
	{
		// deregister the event handler
		HealthCpnt.OnHit -= OnHit;
		
		// destroy the UI indicator that a door is under attack if the door is offscreen
		if (offscreenMarker != null)
		{
			UnityEngine.Object.Destroy(offscreenMarker.gameObject);
		}
		
		// play the door destruction sound effect
		if (hitVFX != null)
		{
			UnityEngine.Object.Destroy(hitVFX.gameObject);
		}
		
		// play the door destruction animation
		if (bonusAnim != null)
		{
			UnityEngine.Object.Destroy(bonusAnim.gameObject);
		}
		
		// get the game manager to free memory of the door
		if (gameNetManager == null)
		{
			gameNetManager = SingletonManager.Get<GameNetworkManager>();
		}
		
		// memory free?
		if (gameNetManager != null)
		{
			gameNetManager.ClearCachedAITargets(this);
		}
	}

	private void OnAITargetActorChange()
	{
		if (!IsOpening && offscreenMarker == null)
		{
			Mob component = aiTarget.Actor.SimMB.GetComponent<Mob>();
			if (!attackers.Contains(component))
			{
				attackers.Add(component);
				component.OnMobDeath += DoOnAttackerDied;
			}
			offscreenMarker = dungeon.DisplayOffscreenMarker(roof.transform, doorOpenerIconData);
			if (HealthCpnt.HealthBar.BarContainer != null)
			{
				HealthCpnt.HealthBar.BarContainer.transform.parent = offscreenMarker.transform;
				HealthCpnt.HealthBar.BarContainer.transform.localPosition = hpBarInitialPos;
				HealthCpnt.HealthBar.BarContainer.transform.localScale = hpBarInitialScale;
				HealthCpnt.HealthBar.BarContainer.transform.localEulerAngles = Vector3.zero;
				HealthCpnt.HealthBar.ShowBar();
			}
		}
	}

	/// <summary>
	/// RPC for door attacker death. 
	/// </summary>
	/// <param name="category">catagory of the attacking mob</param>
	/// <param name="id">id of the attacking mob</param>
	private void RPC_OnAttackerDied(StaticString category, int id)
	{
		Mob component = UniqueIDManager.Get(category, id).GetComponent<Mob>();
		if (component != null)
		{
			DoOnAttackerDied(component);
		}
	}

	/// <summary>
	/// Door attacker death trigger. Players can kill door attackers. This function removes the attack from the
	/// attackers list and hides the door health bar. 
	/// </summary>
	/// <param name="attacker">The attacker that has died</param>
	public void DoOnAttackerDied(Mob attacker)
	{
		// remove from attackers list 
		attackers.Remove(attacker);
		if (offscreenMarker != null && attackers.Count == 0)
		{
			// hide the health bar
			HealthCpnt.HealthBar.BarContainer.transform.parent = base.transform;
			HealthCpnt.HealthBar.HideBar();
			offscreenMarker.Hide();
		}
	}
}
