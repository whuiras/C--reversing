public class Hero : HeroMobCommon
{
	public delegate void HeroSelectEventHandler(Hero selectedHero);

	public delegate void HasCrystalChangedEventHandler(bool hasCrystal);

	public delegate void SkillsChangedEventHandler();

	public delegate void ActiveHeroDeathEventHandler();

	public delegate void HeroRecruitedEventHandler();

	public const string RepairFirstHeroAIType = "RepairFirstHero";

	public const string FreHeroAIType = "Free{0}";

	[SerializeField]
	protected OffscreenMarker.OffscreenMarkerData heroDangerIconData;

	private FMOD.Event operateSFXEvent;

	private FMOD.Event repairSFXEvent;

	private StaticString previousOperatingBonusModuleSimDescName;

	private MovableAITarget followTarget;

	private Mob tamedMob;

	[SerializeField]
	private float itemGatheringDuration;

	private Item gatheringItem;

	[SerializeField]
	private Vector3 heroMoveToOffset;

	[SerializeField]
	private Vector3 crystalMoveToOffset;

	[SerializeField]
	private Vector3 npcMoveToOffset;

	[SerializeField]
	private Vector3 doorDestMarkerOffset;

	[SerializeField]
	private Vector3 heroDestMarkerOffset;

	[SerializeField]
	private Vector3 npcDestMarkerOffset;

	[SerializeField]
	private Vector3 specialModuleDestMarkerOffset;

	[SerializeField]
	private Vector3 crystalDestMarkerOffset;

	[SerializeField]
	private float crystalSlotApproachDistance;

	private MajorModule crystalMoveTarget;

	private SpecialMajorModule specialMajorModuleMoveTarget;

	private NPC npcMoveTarget;

	private Hero heroMoveTarget;

	private Room roomMoveTarget;

	private Door doorMoveTarget;

	private Item itemMoveTarget;

	private Room nextMoveRoomMoveTarget;

	private Door nextMoveDoorMoveTarget;

	private List<ActiveSkill> activeSkills;

	private List<PassiveSkill> passiveSkills;

	private static List<Hero> localPlayerActiveRecruitedHeroes = new List<Hero>();

	private static List<Hero> remotePlayersActiveRecruitedHeroes = new List<Hero>();

	private static List<Hero> selectedHeroes = new List<Hero>();

	private static List<Hero> deadHeroes = new List<Hero>();

	private static SimulationDescriptor operatingBonusGenericSimDesc;

	private static Dictionary<StaticString, SimulationDescriptor> operatingBonusModuleSimDesc = new Dictionary<StaticString, SimulationDescriptor>();

	[SerializeField]
	protected SpriteAnimationRuntime2 actionAnim;

	[SerializeField]
	private ParticleSystem supportBoosterVFX;

	[SerializeField]
	private ParticleSystem supportHealingVFX;

	[SerializeField]
	private ParticleSystem supportSonarVFX;

	[SerializeField]
	private ParticleSystem dismissVFX;

	[SerializeField]
	private Renderer selectorRenderer;

	[SerializeField]
	private Transform destRoomMarkerTfm;

	[SerializeField]
	private Renderer destRoomMarkerRnd;

	[SerializeField]
	private float hideDelayOnDismiss;

	[SerializeField]
	private float destroyDelayOnDismiss;

	[SerializeField]
	private Light heroLight;

	[SerializeField]
	private Light crystalLight;

	[SerializeField]
	private bool debug;

	[SerializeField]
	private Transform crystalContainer;

	[SerializeField]
	private SpriteAnimationRuntime2 crystalSprite;

	[SerializeField]
	private float crystalDistanceToPlayer;

	[SerializeField]
	private float crystalHeight;

	[SerializeField]
	private float doubleSelectMaxTimeDelta;

	[SerializeField]
	private GameObject activeSkillPfb;

	[SerializeField]
	private GameObject passiveSkillPfb;

	[SerializeField]
	private AITarget heroAttackTargetCpnt;

	[SerializeField]
	private AITarget mobsAggroAttackTargetCpnt;

	[SerializeField]
	private GameObject situationDialogPanelPfb;

	[SerializeField]
	private float lowHealthBarScaleY;

	[SerializeField]
	private float lowHealthBarScaleX;

	[SerializeField]
	private float lowHealthValue;

	[SerializeField]
	private GameObject playerNamePanelPfb;

	[SerializeField]
	private Color recruitementTextColor;

	[SerializeField]
	private SpriteAnimationRuntime2 tacticalMapElementAnim;

	[SerializeField]
	private SpriteAnimationRuntime2 selectTacticalMapElementAnim;

	[SerializeField]
	private SpriteAnimationRuntime2 itemRespawnSprite;

	private Vector3 targetLastKnownPos;

	private Dungeon dungeon;

	private List<HeroLevelConfig> levelConfigs;

	private List<Dictionary<StaticString, float>> levelUpModifiersByLevel;

	private ParticleSystem levelUpVfxParticles;

	private IGameEventService gameEventManager;

	private float lastSelectRealTime;

	private int attackingMobCount;

	private float defenseBonusFromMobKills;

	private float attackBonusFromMobKills;

	private float foodBonusFromMobKills;

	private float industryBonusFromMobKills;

	private SituationDialogPanel situationDialogPanel;

	private IGameControlService gameControlManager;

	private float initalHealthBarScaleY;

	private float initalHealthBarScaleX;

	private IAudioEventService audioEventManager;

	private bool allowAnim = true;

	private HeroPlayerNamePanel playerNamePanel;

	private AchievementManagerDOTE achievementManager;

	private SelectableManager selectableManager;

	private GameObject SkillA0039VFXPfb;

	private int floorRecruited = 1;

	private float previousMobTaming;

	private float previousSkillA0039Value;

	private bool shipRespawn = true;

	private float respawnHealthRatio;

	public static List<Hero> RecentlyDeceasedHeroes = new List<Hero>();

	private float cachedSkill_A0039_ZoneAttackRadius;

	private float cachedSkill_A0039_AttackPower;

	public MajorModule OperatingModule
	{
		get;
		private set;
	}

	public bool HasOperatingBonus
	{
		get;
		private set;
	}

	public Module RepairingModule
	{
		get;
		private set;
	}

	public MajorModule PreviousOperatedModule
	{
		get;
		set;
	}

	public Mob TamedMob
	{
		get
		{
			return tamedMob;
		}
		private set
		{
			tamedMob = value;
			if (followTarget != null)
			{
				followTarget.AuthorizedFollower = TamedMob.gameObject;
			}
		}
	}

	public Mover MoverCpnt
	{
		get;
		private set;
	}

	public ModuleSlot CrystalSlotMoveTarget
	{
		get;
		private set;
	}

	public List<ActiveSkill> FilteredActiveSkills
	{
		get;
		private set;
	}

	public List<PassiveSkill> FilteredPassiveSkills
	{
		get;
		private set;
	}

	public static List<Hero> SelectedHeroes => selectedHeroes;

	public static List<Hero> LocalPlayerActiveRecruitedHeroes => localPlayerActiveRecruitedHeroes;

	public static List<Hero> DeadHeroes => deadHeroes;

	public static List<Hero> RemotePlayersActiveRecruitedHeroes => remotePlayersActiveRecruitedHeroes;

	public int UnlockLevel
	{
		get;
		set;
	}

	public bool IsUsable
	{
		get;
		set;
	}

	public HeroConfig Config
	{
		get;
		set;
	}

	public bool IsStartingHero
	{
		get;
		set;
	}

	public bool IsRecruitable
	{
		get;
		set;
	}

	public int CurrentRespawnRoomCount
	{
		get;
		private set;
	}

	public EquipmentSlot[] EquipmentSlots
	{
		get;
		private set;
	}

	public bool IsInteracting
	{
		get;
		private set;
	}

	public bool IsRespawning
	{
		get;
		private set;
	}

	public bool HasCrystal
	{
		get;
		private set;
	}

	public string LocalizedName
	{
		get;
		private set;
	}

	public bool IsRecruited
	{
		get;
		private set;
	}

	public int FloorRecruited
	{
		get
		{
			return floorRecruited;
		}
		private set
		{
			floorRecruited = value;
		}
	}

	public bool IsDismissing
	{
		get;
		private set;
	}

	public List<SimulationDescriptor> PermanentDescriptors
	{
		get;
		private set;
	}

	public HeroAI AICpnt
	{
		get;
		private set;
	}

	public float LowHealthValue => lowHealthValue;

	public event Action<Mob, Mob> OnMobTamed;

	public static event Action OnLocalActiveHeroesChange;

	public event HasCrystalChangedEventHandler OnHasCrystalChanged;

	public event SkillsChangedEventHandler OnSkillsChanged;

	public static event ActiveHeroDeathEventHandler OnActiveHeroDeath;

	public static event HeroRecruitedEventHandler OnHeroRecruited;

	
	/// <summary>
	/// Method for operating modules. Calls RPC_StartModuleOperating
	/// </summary>
	/// <param name="module">The module to operate</param>
	public void OperateModule(MajorModule module)
	{
		// error checking
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.OperateModule should only be called on owner side!");
		}
		else if (!(OperatingModule == module))
		{
			AICpnt.OnTargetChanged += OnOperateTargetChanged;
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_StartModuleOperating, module.UniqueID.GetCategory(), module.UniqueID.ID);
		}
	}

	
	/// <summary>
	/// RPC for starting to operate a module
	/// </summary>
	/// <param name="moduleCategory">Catagory of module</param>
	/// <param name="moduleID">The ID of the module</param>
	private void RPC_StartModuleOperating(StaticString moduleCategory, int moduleID)
	{
		
		// get the module
		MajorModule majorModule = OperatingModule = UniqueIDManager.Get(moduleCategory, moduleID).GetComponent<MajorModule>();
		
		// If hero has operating abilities that give bonuses, update to new module
		if (HasOperatingBonus)
		{
			RemoveOperatingDescriptors(PreviousOperatedModule);
			if (PreviousOperatedModule == null)
			{
				HasOperatingBonus = false;
			}
			else
			{
				AddOperatingDescriptors();
			}
		}
		OperatingModule.Operate(this);
		
		// animate and play SFX
		spriteAnim.SetAnimDirection(OperatingModule.transform.position - base.transform.position);
		spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: true);
		actionAnim.Trigger((!HasOperatingBonus) ? SpriteAnimationTrigger.OnOperate : SpriteAnimationTrigger.OnOperateBonus);
		operateSFXEvent = base.AudioEmitter.PlayEvent("Master/Environment/ModuleEntretien");
		
		// Unclear
		gameEventManager.TriggerHeroListChangedEvent();
		
		if (!HasOperatingBonus)
		{
			// add listeners
			gameEventManager.OnDungeonTurnChanged -= OnDungeonTurnChangedDuringAction;
			gameEventManager.OnDungeonTurnChanged += OnDungeonTurnChangedDuringAction;
		}
	}

	/// <summary>
	/// Unclear what AddSimDescriptor does, method definition seems like a rabbit-hole for the time being 
	/// </summary>
	private void AddOperatingDescriptors()
	{
		AddSimDescriptor(operatingBonusGenericSimDesc);
		SimulationDescriptor moduleOperatingDescriptor = GetModuleOperatingDescriptor();
		if (moduleOperatingDescriptor != null)
		{
			AddSimDescriptor(moduleOperatingDescriptor);
			previousOperatingBonusModuleSimDescName = moduleOperatingDescriptor.Name;
		}
	}

	/// <summary>
	/// Removes operating descriptors. See AddOperatingDescriptors()
	/// </summary>
	/// <param name="module">Module whose descriptor to remove</param>
	private void RemoveOperatingDescriptors(MajorModule module = null)
	{
		RemoveSimDescriptor(operatingBonusGenericSimDesc);
		SimulationDescriptor moduleOperatingDescriptor = GetModuleOperatingDescriptor(module);
		if (moduleOperatingDescriptor != null)
		{
			RemoveSimDescriptor(moduleOperatingDescriptor);
		}
		else if (!string.IsNullOrEmpty(previousOperatingBonusModuleSimDescName))
		{
			RemoveSimDescriptor(previousOperatingBonusModuleSimDescName);
		}
	}

	/// <summary>
	/// Unclear
	/// </summary>
	private void OnDungeonTurnChangedDuringAction()
	{
		gameEventManager.OnDungeonTurnChanged -= OnDungeonTurnChangedDuringAction;
		if (HasOperatingBonus)
		{
			Diagnostics.LogError("{0} > OnDungeonTurnChanged: this.HasOperatingBonus shouldn't already be true!", base.name);
			return;
		}
		HasOperatingBonus = true;
		RemoveOperatingDescriptors();
		AddOperatingDescriptors();
		if (OperatingModule != null)
		{
			OperatingModule.UpdateOperatorWit(this);
			gameEventManager.TriggerHeroListChangedEvent();
			actionAnim.Trigger(SpriteAnimationTrigger.OnOperateBonus);
		}
	}

	/// <summary>
	/// Repair operation on module. Calls RPC_StartModuleRepairing
	/// </summary>
	/// <param name="module">The module to repair</param>
	public void RepairModule(Module module)
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.RepairModule should only be called on owner side!");
		}
		else if (!(RepairingModule == module))
		{
			AICpnt.OnTargetChanged += OnRepairTargetChanged;
			dungeon.CheckSituationDialog(SituationDialogType.RepairModule, this);
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_StartModuleRepairing, module.UniqueID.GetCategory(), module.UniqueID.ID);
		}
	}

	/// <summary>
	/// RPC for repair operation. Calls module.repair, VFX/SFX, and refreshes
	/// </summary>
	/// <param name="moduleCategory">Catagory of module</param>
	/// <param name="moduleID">Module ID</param>
	private void RPC_StartModuleRepairing(StaticString moduleCategory, int moduleID)
	{
		Module module = RepairingModule = UniqueIDManager.Get(moduleCategory, moduleID).GetComponent<Module>();
		
		// Call to repair
		RepairingModule.Repair(this);
		
		// Animate and play SFX
		if (allowAnim)
		{
			spriteAnim.SetAnimDirection(RepairingModule.transform.position - base.transform.position);
			spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: true);
			actionAnim.Trigger(SpriteAnimationTrigger.OnRepair);
		}
		repairSFXEvent = base.AudioEmitter.PlayEvent("Master/Environment/RepairModule");
		
		// Unclear. Refreshing something after a chain of subsequent calls
		gameEventManager.TriggerHeroListChangedEvent();
	}

	/// <summary>
	/// This seems like a call for when a module is built in a room where there is unclaimed, AI hero. Handles operating
	/// target aquisition. "AI" might be a red herring here though, on further inspection it looks like this is called
	/// for AI and player-controlled heros the same. More or less calls 
	/// </summary>
	/// <param name="previousTarget">Previous module target</param>
	/// <param name="newTarget">New module target</param>
	private void OnOperateTargetChanged(AITarget previousTarget, AITarget newTarget)
	{
		// error checking
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.RepairModule should only be called on owner side!");
			return;
		}
		MajorModule majorModule = null;
		if (previousTarget != null)
		{
			majorModule = previousTarget.GetComponent<MajorModule>();
			if (majorModule != OperatingModule)
			{
				Diagnostics.LogError(base.name + " > OnOperateTargetChanged: previous target != current operating module");
				return;
			}
		}
		AICpnt.OnTargetChanged -= OnOperateTargetChanged;
		if (HasOperatingBonus && (previousTarget == null || !previousTarget.GetComponent<Health>().IsAlive()))
		{
			RemoveOperatingDescriptors(majorModule);
			HasOperatingBonus = false;
			gameEventManager.OnDungeonTurnChanged -= OnDungeonTurnChangedDuringAction;
		}
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_StopModuleOperating);
	}

	/// <summary>
	/// Unclear how this is called. May be connected to the above function. See:
	/// 		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_StopModuleOperating);
	/// It is likely that this is connected via server-side code.
	///
	/// RPC for stopping module operation. Calls StopOperating() and stops VFX/SFX
	/// </summary>
	private void RPC_StopModuleOperating()
	{
		if (HasOperatingBonus)
		{
			RemoveOperatingDescriptors();
		}
		if (OperatingModule != null)
		{
			PreviousOperatedModule = OperatingModule;
			OperatingModule.StopOperating(this);
			OperatingModule = null;
		}
		spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: false);
		actionAnim.Stop();
		base.AudioEmitter.StopEvent(operateSFXEvent);
		gameEventManager.TriggerHeroListChangedEvent();
	}

	/// <summary>
	/// Method for changing repair target. Calls Hero_StopModuleRepairing
	/// </summary>
	/// <param name="previousTarget">Previous hero target. Like the previous methods, it's unclear why these parameters are
	/// prefixed with 'AI'</param>
	/// <param name="newTarget">New hero target</param>
	private void OnRepairTargetChanged(AITarget previousTarget, AITarget newTarget)
	{
		
		//error checking
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.RepairModule should only be called on owner side!");
			return;
		}
		if (previousTarget != null && previousTarget.GetComponent<Module>() != RepairingModule)
		{
			Diagnostics.LogError(base.name + " > OnRepairTargetChanged: previous target != current repairing module");
			return;
		}
		
		// unhook event
		AICpnt.OnTargetChanged -= OnRepairTargetChanged;
		
		// call to RPC method
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_StopModuleRepairing);
	}

	/// <summary>
	/// RPC for stop repairing module. Calls StopRepairing and stops VFX/SFX
	/// </summary>
	private void RPC_StopModuleRepairing()
	{
		if (RepairingModule != null)
		{
			RepairingModule.StopRepairing(this);
			RepairingModule = null;
		}
		spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: false);
		actionAnim.Stop();
		base.AudioEmitter.StopEvent(repairSFXEvent);
		gameEventManager.OnDungeonTurnChanged -= OnDungeonTurnChangedDuringAction;
		gameEventManager.TriggerHeroListChangedEvent();
	}

	/// <summary>
	/// Gets module operating descriptors. Still unclear what these descriptors are. 
	/// </summary>
	/// <param name="module">Module in question</param>
	/// <returns>The descriptor</returns>
	private SimulationDescriptor GetModuleOperatingDescriptor(Module module = null)
	{
		SimulationDescriptor value = null;
		if (OperatingModule != null)
		{
			StaticString staticString = (module ?? OperatingModule).BPConfig.ModuleName;
			if (!operatingBonusModuleSimDesc.TryGetValue(staticString, out value))
			{
				value = SimMonoBehaviour.GetDBDescriptorByName("Hero_Operating_Bonus_" + staticString);
				operatingBonusModuleSimDesc.Add(staticString, value);
			}
		}
		return value;
	}

	/// <summary>
	/// One hero can uniquely tame hostile mobs to fight for them, for a breif period. This function controls that
	/// behavior. 
	/// </summary>
	public void FindAndTameAMob()
	{
		// get list of mobs
		Mob[] array = base.RoomElement.ParentRoom.Mobs.Where((Mob m) => m.Tamer == null && m.Config != null && m.HealthCpnt != null && m.AttackTargetCpnt != null && m.AttackTargetCpnt.CanBeAttacked()).ToArray();
		if (array.Length > 0)
		{
			if (TamedMob != null && TamedMob.HealthCpnt != null)
			{
				TamedMob.HealthCpnt.Kill();
			}
			// select the most threatening
			int highestDifficultyValue = array.Max((Mob m) => m.Config.DifficultyValue);
			Mob mob = (from m in array
				where m.Config.DifficultyValue == highestDifficultyValue
				orderby m.HealthCpnt.GetHealth() descending
				select m).ElementAt(0);
			
			// call tame mob RPC
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_TameMob, mob.UniqueID.GetCategory(), mob.UniqueID.ID);
		}
		else
		{
			Diagnostics.Log("No tamable mob was found");
		}
	}

	/// <summary>
	/// RPC for taming a mob. Calls DoTameMob
	/// </summary>
	/// <param name="mobCategory">Catagory of mob</param>
	/// <param name="mobId">ID of the mob</param>
	private void RPC_TameMob(StaticString mobCategory, int mobId)
	{
		Mob component = UniqueIDManager.Get(mobCategory, mobId).GetComponent<Mob>();
		if (component != null)
		{
			DoTameMob(component);
		}
	}

	/// <summary>
	/// Tames a mob. Calls Tame() on the mob, triggers OnMobTamed(), and sets the mob to follow the hero
	/// </summary>
	/// <param name="mob">Mob to tame</param>
	/// <param name="ownerChanged">If there was a previous owner of the mob</param>
	public void DoTameMob(Mob mob, bool ownerChanged = false)
	{
		if (!(mob.Tamer != null))
		{
			Diagnostics.Log("Mob {0} tamed by {1}", mob.name, base.name);
			if (this.OnMobTamed != null)
			{
				this.OnMobTamed(mob, TamedMob);
			}
			if (followTarget == null)
			{
				followTarget = base.gameObject.AddComponent<MovableAITarget>();
				followTarget.Init(AITargetType.Tamer, AIInteraction.Follow);
			}
			TamedMob = mob;
			attackerCpnt.StopAttack();
			TamedMob.Tame(this, ownerChanged);
		}
	}

	/// <summary>
	/// Plays gathering SFX and VFX for an item. Calls item.Aquire() 
	/// </summary>
	public void AcquireItem()
	{
		if (allowAnim)
		{
			spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: false);
		}
		if (!(gatheringItem == null))
		{
			if (base.NetSyncElement.IsOwnedByLocalPlayer())
			{
				base.AudioEmitter.PlayEvent("Master/Environment/CollectItem");
			}
			gatheringItem.Acquire(this);
			actionAnim.Stop();
			gatheringItem = null;
		}
	}

	/// <summary>
	/// Conditional for determining whether a hero can equip an item or not. Heros are natually restricted from some
	/// item types. Calls CanHoldItem() on each equipment slot
	/// </summary>
	/// <param name="item">The item in question</param>
	/// <returns>Bool whether the item can be equiped</returns>
	public bool CanEquipItem(InventoryItem item)
	{
		for (int i = 0; i < EquipmentSlots.Length; i++)
		{
			if (EquipmentSlots[i].CanHoldItem(item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Calls RPC for gathering an item
	/// </summary>
	/// <param name="item">the item in question</param>
	public void StartItemGathering(Item item)
	{
		// sync error checking
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.NetSyncStartItemGathering should only be called on owner side!");
		}
		else
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoStartItemGathering, item.UniqueID.GetCategory(), item.UniqueID.ID);
		}
	}
	
	/// <summary>
	/// RPC for item gathering. Lots of error checking, then calls gather item along with VFX/SFX. 
	/// </summary>
	/// <param name="itemCategory">Catagory of item</param>
	/// <param name="itemID">item ID</param>
	private void RPC_DoStartItemGathering(StaticString itemCategory, int itemID)
	{
		UniqueIDGetError error;
		//error checking
		UnityEngine.Component component = UniqueIDManager.Get(itemCategory, itemID, out error, logError: false);
		if (component == null)
		{
			if (error == UniqueIDGetError.ObjectIsNull)
			{
				Diagnostics.Log("Hero.RPC_DoStartItemGathering: {0}#{1} item not found (already gathered&destroyed?)", itemCategory, itemID);
			}
			else
			{
				Diagnostics.LogError("Hero.RPC_DoStartItemGathering: {0}#{1} item not found (error={2})", itemCategory, itemID, error);
			}
			return;
		}
		Item component2 = component.GetComponent<Item>();
		if (component2 == null)
		{
			Diagnostics.LogError("Hero.RPC_DoStartItemGathering: couldn't find Item component on {0}#{1} item!", itemCategory, itemID);
			return;
		}
		if (!component2.CanBeGathered)
		{
			Diagnostics.LogWarning(base.name + " > OnItemReached: Item cannot be gathered");
			return;
		}
		if (gatheringItem == component2)
		{
			Diagnostics.LogWarning(base.name + " > OnItemReached: Already gathering this item");
			return;
		}
		gatheringItem = component2;
		if (MoverCpnt.IsMoving)
		{
			MoverCpnt.StopMove();
		}
		if (allowAnim)
		{
			spriteAnim.SetAnimDirection(gatheringItem.transform.position - base.transform.position);
			spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: true);
		}
		
		//animate and actually gather the item
		actionAnim.Trigger(SpriteAnimationTrigger.OnLoot);
		gatheringItem.StartGathering(this);
		Invoke("AcquireItem", itemGatheringDuration);
		AICpnt.OnTargetChanged += OnItemLootTargetChanged;
	}

	/// <summary>
	/// Cancel command for item gathering. Calls cancel RPC.
	/// </summary>
	public void CancelItemGathering()
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.NetSyncCancelItemGathering should only be called on owner side!");
		}
		else
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoCancelItemGathering);
		}
	}

	/// <summary>
	/// RPC for gather cancel
	/// </summary>
	private void RPC_DoCancelItemGathering()
	{
		if (gatheringItem != null)
		{
			gatheringItem.StopGathering();
			spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: false);
			actionAnim.Stop();
			gatheringItem = null;
			CancelInvoke("AcquireItem");
		}
	}

	/// <summary>
	/// Calls Equip RPC. 
	/// </summary>
	/// <param name="slotIndex">Which inventory slot to equip on</param>
	/// <param name="item">The item to equip</param>
	/// <param name="removeFromInventory">Whether the item should be removed from the hero's inventory</param>
	/// <param name="netSync">Unclear</param>
	/// <param name="checkItemInventory">Unclear</param>
	public void EquipItemOnSlot(int slotIndex, InventoryItem item, bool removeFromInventory, bool netSync, bool checkItemInventory)
	{
		
		// unclear what the difference between these two calls is. Will have to look into this further. 
		if (netSync)
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_EquipItemOnSlot, slotIndex, item.UniqueIDCategory, item.UniqueID, removeFromInventory, netSync, checkItemInventory);
		}
		else
		{
			DoEquipItemOnSlot(slotIndex, item, removeFromInventory, netSync, checkItemInventory);
		}
	}

	/// <summary>
	/// RPC for equipping an item. More or less calls DoEquipItemOnSlot
	/// </summary>
	/// <param name="slotIndex">The inventory slot to equip on</param>
	/// <param name="itemUniqueIDCategory">The item catagory</param>
	/// <param name="itemUniqueID">The item ID</param>
	/// <param name="removeFromInventory">Whether the item should be removed from the hero's inventory</param>
	/// <param name="netSync">Unclear</param>
	/// <param name="checkItemInventory">Unclear</param>
	private void RPC_EquipItemOnSlot(int slotIndex, StaticString itemUniqueIDCategory, int itemUniqueID, bool removeFromInventory, bool netSync, bool checkItemInventory)
	{
		// The item in question
		InventoryItem item = InventoryItem.GetItem(itemUniqueIDCategory, itemUniqueID);
		if (item != null)
		{
			DoEquipItemOnSlot(slotIndex, item, removeFromInventory, netSync, checkItemInventory);
		}
	}

	/// <summary>
	/// Main method for equipping an item in a inventory slot. Calls DoEquipItem on an item slot. 
	/// </summary>
	/// <param name="slotIndex">The slot to equip on</param>
	/// <param name="item">The item to equip</param>
	/// <param name="removeFromInventory">Whether the item should be removed from the hero's inventory</param>
	/// <param name="netSync"></param>
	/// <param name="checkItemInventory"></param>
	private void DoEquipItemOnSlot(int slotIndex, InventoryItem item, bool removeFromInventory, bool netSync, bool checkItemInventory)
	{
		bool flag = base.NetSyncElement.IsOwnedByLocalPlayer();
		float ratio = 0f;
		
		// No idea what the health ratio business is doing. 
		if (flag)
		{
			ratio = base.HealthCpnt.GetHealthRatio();
		}
		
		// call to equip the item
		EquipmentSlots[slotIndex].DoEquipItem(item, removeFromInventory, netSync, checkItemInventory);
		if (flag)
		{
			base.HealthCpnt.SetHealthRatio(ratio);
		}
	}

	/// <summary>
	/// RPC.... 
	/// </summary>
	/// <param name="slotIndex"></param>
	/// <param name="netSync"></param>
	/// <param name="checkConfig"></param>
	public void UnequipItemFromSlot(int slotIndex, bool netSync, bool checkConfig = true)
	{
		if (netSync)
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_UnequipItemFromSlot, slotIndex, checkConfig);
		}
		else
		{
			DoUnequipItemFromSlot(slotIndex, checkConfig);
		}
	}

	private void RPC_UnequipItemFromSlot(int slotIndex, bool checkConfig)
	{
		DoUnequipItemFromSlot(slotIndex, checkConfig);
	}

	private void DoUnequipItemFromSlot(int slotIndex, bool checkConfig = true)
	{
		bool flag = base.NetSyncElement.IsOwnedByLocalPlayer();
		float ratio = 0f;
		if (flag)
		{
			ratio = base.HealthCpnt.GetHealthRatio();
		}
		EquipmentSlots[slotIndex].DoUnequipItem(checkConfig);
		if (flag)
		{
			base.HealthCpnt.SetHealthRatio(ratio);
		}
	}

	private void InitEquipmentSlots(Dictionary<StaticString, List<ItemPersistentData>> initItemsByCategory = null)
	{
		bool flag = initItemsByCategory == null;
		EquipmentSlots = new EquipmentSlot[Config.EquipmentSlots.Length];
		StaticString staticString = null;
		StaticString staticString2 = null;
		for (int i = 0; i < EquipmentSlots.Length; i++)
		{
			EquipmentSlotConfig equipmentSlotConfig = Config.EquipmentSlots[i];
			EquipmentSlots[i] = new EquipmentSlot(i, equipmentSlotConfig, this);
			EquipmentSlots[i].OnItemChanged += OnItemEquipped;
			staticString = null;
			staticString2 = null;
			if (flag)
			{
				ItemDatatableReference defaultEquippedItem = equipmentSlotConfig.DefaultEquippedItem;
				if (defaultEquippedItem != null)
				{
					staticString = defaultEquippedItem.Name;
					staticString2 = defaultEquippedItem.RarityName;
				}
			}
			else if (initItemsByCategory.ContainsKey(equipmentSlotConfig.CategoryName) && initItemsByCategory[equipmentSlotConfig.CategoryName].Count > 0)
			{
				ItemPersistentData itemPersistentData = initItemsByCategory[equipmentSlotConfig.CategoryName].PullAt(0);
				staticString = itemPersistentData.ItemDescName;
				staticString2 = itemPersistentData.RarityDescName;
			}
			if (!(staticString != null))
			{
				continue;
			}
			InventoryItem inventoryItem = InventoryItem.BuildInventoryItem(staticString, staticString2, null, base.NetSyncElement.OwnerPlayerID);
			if (inventoryItem != null)
			{
				if (inventoryItem.ItemConfig.CategoryParameters.IsCategoryMatching(equipmentSlotConfig))
				{
					EquipmentSlots[i].EquipItem(inventoryItem, removeFromInventory: true, netSync: false, skipRequestToServer: true, checkItemInventory: false);
				}
				else
				{
					Diagnostics.LogError("[Hero] InitEquipmentSlots: the item (" + staticString + ") category and/or type is not matching with the slot for hero " + base.name);
				}
			}
		}
	}

	private void OnItemLootTargetChanged(AITarget previousTarget, AITarget newTarget)
	{
		if (!(gatheringItem == null))
		{
			if (previousTarget.GetComponent<Item>() != gatheringItem)
			{
				Diagnostics.LogError(base.name + " > OnItemLootTargetChanged: previous target != current gathering item");
				return;
			}
			AICpnt.OnTargetChanged -= OnItemLootTargetChanged;
			CancelItemGathering();
		}
	}

	public void OnItemEquipped(EquipmentSlot slot, InventoryItem item, bool equipped)
	{
		if (attackerCpnt != null)
		{
			if (equipped)
			{
				AttackTypeConfig attackTypeConfig = null;
				if (item.ItemConfig.AttackTypeConfigName != null)
				{
					attackTypeConfig = Databases.GetDatabase<AttackTypeConfig>().GetValue(item.ItemConfig.AttackTypeConfigName);
				}
				if (attackTypeConfig != null)
				{
					attackerCpnt.SwitchAttackType(attackTypeConfig);
				}
				base.AudioEmitter.PlayEvent("Master/GUI/EquipItem");
			}
			else
			{
				attackerCpnt.ResetAttackType();
			}
		}
		if (base.HealthCpnt != null)
		{
			base.HealthCpnt.ClampHealth();
		}
		if (item.ItemConfig.Skills != null && item.ItemConfig.Skills.Length > 0)
		{
			string[] skills = item.ItemConfig.Skills;
			foreach (string skillName in skills)
			{
				if (equipped)
				{
					AddSkill(skillName);
				}
				else
				{
					RemoveSkill(skillName);
				}
			}
		}
		if (OperatingModule != null)
		{
			OperatingModule.UpdateOperatorWit(this);
		}
		if (RepairingModule != null)
		{
			RepairingModule.UpdateReparatorWit(this);
		}
	}

	private void RPC_RequestBuyItemFromEquipmentSlot(StaticString itemUniqueIDCategory, int itemUniqueID, int slotIndex, ulong buyerID)
	{
		if (!gameNetManager.IsServerOrSinglePlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_RequestBuyItemFromEquipmentSlot should only be called on server side!");
			return;
		}
		InventoryItem item = InventoryItem.GetItem(itemUniqueIDCategory, itemUniqueID);
		if (item != null && dungeon.RequestBuyItem(item))
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoBuyItemFromEquipmentSlot, itemUniqueIDCategory, itemUniqueID, slotIndex, buyerID);
		}
	}

	private void RPC_DoBuyItemFromEquipmentSlot(StaticString itemUniqueIDCategory, int itemUniqueID, int slotIndex, ulong buyerID)
	{
		InventoryItem item = InventoryItem.GetItem(itemUniqueIDCategory, itemUniqueID);
		if (item == null)
		{
			return;
		}
		FIDS currency = (item.CurrentInventory.ParentSimMB as NPCMerchant).CurrencyCfg.Currency;
		bool flag = buyerID == gameNetManager.GetLocalPlayerID();
		if (gameNetManager.IsMultiplayerSession() && ((currency == FIDS.Dust && !MultiplayerConfig.SplitDust) || (currency != FIDS.Dust && !MultiplayerConfig.SplitFIS)))
		{
			flag = false;
		}
		if (flag && !Player.LocalPlayer.ConsumeFIDS(item.GetCostForCurrency(currency), currency))
		{
			Diagnostics.LogError("Hero.RPC_DoBuyItemFromEquipmentSlot: not enough resources!");
			return;
		}
		if (flag && item.IsGift)
		{
			ItemHero.SendItemAcquisitionNotification(new ItemHeroElement(item));
			audioEventManager.Play2DEvent("Master/GUI/DeathMerchant_Buy");
		}
		bool flag2 = true;
		item.MoveToInventory((!flag2) ? null : dungeon.GetBestAvailableInventoryForPlayer(buyerID));
		item.OwnerPlayerID = buyerID;
		EquipmentSlot equipmentSlot = EquipmentSlots[slotIndex];
		equipmentSlot.DoBuyCurrentItem(item);
		MerchantPanel merchantPanel = SingletonManager.Get<MainGameScreen>().MerchantPanel;
		merchantPanel.OnCurrentItemBought();
	}

	private void RPC_RequestEquipItemFromEquipmentSlot(StaticString itemUniqueIDCategory, int itemUniqueID, int slotIndex, bool removeFromInventory, bool netSync, bool checkItemInventory)
	{
		InventoryItem item = InventoryItem.GetItem(itemUniqueIDCategory, itemUniqueID);
		if (item != null)
		{
			EquipmentSlot equipmentSlot = EquipmentSlots[slotIndex];
			equipmentSlot.RequestEquipItem(item, removeFromInventory, netSync, checkItemInventory);
		}
	}

	public void MoveToRoom(Room room, bool allowMoveInterruption = false, bool isMoveOrderedByPlayer = true, bool triggerTutorialEvent = true)
	{
		if (isMoveOrderedByPlayer && !IsUsable)
		{
			Diagnostics.LogWarning("MoveToRoom: hero is not usable");
		}
		else if (!MoverCpnt.CanMove)
		{
			Diagnostics.LogWarning("MoveToRoom: hero cannot move");
		}
		else if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > MoveToRoom but hero is dead");
		}
		else
		{
			if ((MoverCpnt.IsMoving && roomMoveTarget == room) || (!MoverCpnt.IsMoving && base.RoomElement.ParentRoom == room))
			{
				return;
			}
			if (isMoveOrderedByPlayer)
			{
				base.AudioEmitter.PlayEvent("Master/GUI/MoveHero");
			}
			if (triggerTutorialEvent && TutorialManager.IsEnable)
			{
				Services.GetService<IGameEventService>()?.TriggerHeroMovedToRoomTutorialEvent();
			}
			if (!room.IsFullyOpened)
			{
				Door openingDoor = room.GetOpeningDoor();
				if (openingDoor != null)
				{
					Diagnostics.Log(base.name + " > move to opening door and then to room");
					MoveToDoor(openingDoor, allowMoveInterruption, null, isMoveOrderedByPlayer: false);
					return;
				}
				Diagnostics.LogError("No door opening room " + room + ", impossible to reach that room");
			}
			ResetMoveTargets();
			roomMoveTarget = room;
			Vector3 centerPosition = room.CenterPosition;
			SetDestMarkerPosition(centerPosition);
			RequestMoveToPosition(centerPosition, null, allowMoveInterruption, room);
		}
	}

	public void MoveToDoor(Door door, bool allowMoveInterruption = false, Door nextMoveDoorTarget = null, bool isMoveOrderedByPlayer = true)
	{
	
	    ///
		if (isMoveOrderedByPlayer && !IsUsable)
		{
			Diagnostics.LogWarning("MoveToDoor: hero is not usable");
		}
		else if (isMoveOrderedByPlayer && HasCrystal)
		{
			Diagnostics.LogWarning("[Hero.MoveToDoor] Hero cannot move to door while carrying the crystal.");
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_CannotOpenDoorCrystal"));
		}
		else if (!MoverCpnt.CanMove)
		{
			Diagnostics.LogWarning("MoveToDoor: hero cannot move");
		}
		else if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > MoveToDoor but hero is dead");
		}
		else
		{
			if (MoverCpnt.IsMoving && doorMoveTarget == door)
			{
				return;
			}
			if (isMoveOrderedByPlayer)
			{
				base.AudioEmitter.PlayEvent("Master/GUI/OpenDoorOrder");
			}
			Room uniqueAlreadyOpenRoom = door.GetUniqueAlreadyOpenRoom();
			if (uniqueAlreadyOpenRoom != null && !uniqueAlreadyOpenRoom.IsFullyOpened)
			{
				Door openingDoor = uniqueAlreadyOpenRoom.GetOpeningDoor();
				if (openingDoor != null)
				{
					Diagnostics.Log(base.name + " > move to opening door and then to door");
					MoveToDoor(openingDoor, allowMoveInterruption, door, isMoveOrderedByPlayer: false);
				}
				else
				{
					Diagnostics.LogError("No door opening room " + uniqueAlreadyOpenRoom + ", impossible to reach door " + door);
				}
				return;
			}
			ResetMoveTargets();
			nextMoveDoorMoveTarget = nextMoveDoorTarget;
			if (nextMoveDoorMoveTarget == null)
			{
				nextMoveRoomMoveTarget = door.GetRoomToOpen(base.transform.position);
			}
			else
			{
				nextMoveRoomMoveTarget = null;
			}
			if (nextMoveDoorMoveTarget != null)
			{
				SetDestMarkerPosition(nextMoveDoorMoveTarget.DoorStep.transform.position + doorDestMarkerOffset);
			}
			else
			{
				SetDestMarkerPosition(door.DoorStep.transform.position + doorDestMarkerOffset);
			}
			doorMoveTarget = door;
			RequestMoveToPosition(door.GetOpeningSpot(base.transform.position).position, OnDoorReached, allowMoveInterruption);
			door.RegisterOpener(this);
		}
	}

	public void MoveToHero(Hero hero)
	{
		if (!IsUsable)
		{
			Diagnostics.LogWarning("MoveToHero: hero is not usable");
		}
		else if (MoverCpnt.CanMove)
		{
			if (!base.HealthCpnt.IsAlive())
			{
				Diagnostics.Log(base.name + " > MoveToHero but hero is dead");
			}
			else if (!hero.HealthCpnt.IsAlive())
			{
				Diagnostics.Log(base.name + " > MoveToHero but target is dead");
			}
			else if (!MoverCpnt.IsMoving || !(heroMoveTarget == hero))
			{
				base.AudioEmitter.PlayEvent("Master/GUI/OpenDoorOrder");
				ResetMoveTargets();
				heroMoveTarget = hero;
				SetDestMarkerPosition(heroMoveTarget.transform.position + heroDestMarkerOffset);
				RequestMoveToPosition(heroMoveTarget.transform.position + heroMoveToOffset, OnHeroReached, allowMoveInterruption: false);
			}
		}
	}

	public void MoveToNPC(NPC npc)
	{
		if (!IsUsable)
		{
			Diagnostics.LogWarning("MoveToNPC: hero is not usable");
		}
		else if (MoverCpnt.CanMove)
		{
			if (!base.HealthCpnt.IsAlive())
			{
				Diagnostics.Log(base.name + " > MoveToNPC but hero is dead");
			}
			else if (!npc.HealthCpnt.IsAlive())
			{
				Diagnostics.Log(base.name + " > MoveToNPC but npc is dead");
			}
			else if (HasCrystal)
			{
				Diagnostics.Log(base.name + " > MoveToNPC but carrying the crystal");
			}
			else if (!MoverCpnt.IsMoving || !(npcMoveTarget == npc))
			{
				base.AudioEmitter.PlayEvent("Master/GUI/OpenDoorOrder");
				ResetMoveTargets();
				EndNPCInteraction();
				npcMoveTarget = npc;
				npcMoveTarget.HealthCpnt.OnDeath += OnInteractionTargetDeath;
				SetDestMarkerPosition(npcMoveTarget.transform.position + npcDestMarkerOffset);
				RequestMoveToPosition(npcMoveTarget.transform.position + npcMoveToOffset, OnNPCReached, allowMoveInterruption: false, null, cancelCurrentNPCInteraction: false);
			}
		}
	}

	public void MoveToCrystal(MajorModule crystalModule)
	{
		if (!IsUsable)
		{
			Diagnostics.LogWarning("MoveToCrystal: hero is not usable");
		}
		else if (HasCrystal)
		{
			Diagnostics.LogWarning("[Hero.MoveToCrystal] Hero cannot move to crystal while carrying another crystal.");
		}
		else if (!MoverCpnt.CanMove)
		{
			Diagnostics.LogWarning("MoveToCrystal: hero cannot move");
		}
		else if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > MoveToCrystal but hero is dead");
		}
		else if (!MoverCpnt.IsMoving || !(crystalMoveTarget == crystalModule))
		{
			Dungeon dungeon = SingletonManager.Get<Dungeon>();
			if (dungeon.ExitRoom == null)
			{
				dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_CrystalUnplugFail_ExitNotFound"));
				return;
			}
			base.AudioEmitter.PlayEvent("Master/GUI/OpenDoorOrder");
			ResetMoveTargets();
			crystalMoveTarget = crystalModule;
			SetDestMarkerPosition(crystalModule.transform.position + crystalDestMarkerOffset);
			RequestMoveToPosition(crystalModule.transform.position + crystalMoveToOffset, UnplugCrystal, allowMoveInterruption: false);
		}
	}

	public void MoveToCrystalSlot(ModuleSlot crystalSlot)
	{
		if (!HasCrystal)
		{
			Diagnostics.LogWarning("[Hero.MoveToCrystalSlot] Hero cannot move to crystal slot if he doesn't carry a crystal.");
			return;
		}
		if (!MoverCpnt.CanMove)
		{
			Diagnostics.LogWarning("MoveToCrystalSlot: hero cannot move");
			return;
		}
		if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > MoveToCrystalSlot but hero is dead");
			return;
		}
		base.AudioEmitter.PlayEvent("Master/GUI/MoveHero");
		ResetMoveTargets();
		CrystalSlotMoveTarget = crystalSlot;
		SetDestMarkerPosition(crystalSlot.transform.position);
		RequestMoveToPosition(crystalSlot.transform.position, PlugCrystal, allowMoveInterruption: false, null, cancelCurrentNPCInteraction: true, crystalSlotApproachDistance);
		if (base.RoomElement.ParentRoom == crystalSlot.ParentRoom && ((CrystalModuleSlot)crystalSlot).IsExitSlot)
		{
			PlayLevelSuccessCinematic();
		}
	}

	public void MoveToItem(Item item)
	{
		if (!IsUsable)
		{
			Diagnostics.LogWarning("MoveToItem: hero is not usable");
		}
		else if (HasCrystal)
		{
			Diagnostics.LogWarning("[Hero.MoveToItem] Hero cannot move to an item while carrying a crystal.");
		}
		else if (!MoverCpnt.CanMove)
		{
			Diagnostics.LogWarning("MoveToItem: hero cannot move");
		}
		else if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > MoveToItem but hero is dead");
		}
		else if (!(itemMoveTarget == item) && !(gatheringItem == item) && (!MoverCpnt.IsMoving || !(itemMoveTarget == item)))
		{
			ResetMoveTargets();
			itemMoveTarget = item;
			RequestMoveToPosition(item.GetGatheringPosition(), OnItemReached);
		}
	}

	public void MoveToSpecialMajorModule(SpecialMajorModule module)
	{
		if (!IsUsable)
		{
			Diagnostics.LogWarning("MoveToSpecialMajorModule: hero is not usable");
		}
		else if (HasCrystal)
		{
			Diagnostics.LogWarning("[Hero.MoveToSpecialMajorModule] Hero cannot move to a module while carrying a crystal.");
		}
		else if (!MoverCpnt.CanMove)
		{
			Diagnostics.LogWarning("MoveToSpecialMajorModule: hero cannot move");
		}
		else if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > MoveToSpecialMajorModule but hero is dead");
		}
		else if (!module.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > MoveToSpecialMajorModule but module is destroyed");
		}
		else if (!MoverCpnt.IsMoving || !(specialMajorModuleMoveTarget == module))
		{
			base.AudioEmitter.PlayEvent("Master/GUI/OpenDoorOrder");
			ResetMoveTargets();
			specialMajorModuleMoveTarget = module;
			Vector3 position = module.transform.position;
			SetDestMarkerPosition(position + specialModuleDestMarkerOffset);
			RequestMoveToPosition(position, OnSpecialMajorModuleReached, allowMoveInterruption: false);
		}
	}

	public void MoveToRoomInteractibleElement(Room room)
	{
		if (room.NPCs.Count > 0)
		{
			MoveToNPC(room.NPCs[0]);
			return;
		}
		if (room.Heroes.Count > 0)
		{
			for (int i = 0; i < room.Heroes.Count; i++)
			{
				Hero hero = room.Heroes[i];
				if (!hero.IsRecruited)
				{
					MoveToHero(hero);
					return;
				}
			}
		}
		if (room.MajorModule != null && room.MajorModule is SpecialMajorModule)
		{
			MoveToSpecialMajorModule((SpecialMajorModule)room.MajorModule);
		}
		else if (room.IsStartRoom && room.MajorModule != null && room.MajorModule.IsCrystal)
		{
			MoveToCrystal(room.MajorModule);
		}
	}

	public void OnBlockingDoorOpened()
	{
		if (IsRespawning)
		{
			return;
		}
		if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > OnBlockingDoorOpened but hero is dead");
			return;
		}
		Door door = nextMoveDoorMoveTarget;
		Room room = nextMoveRoomMoveTarget;
		nextMoveDoorMoveTarget = null;
		nextMoveRoomMoveTarget = null;
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DisplayDoorOpeningStop);
		if (door != null)
		{
			MoveToDoor(door, allowMoveInterruption: false, null, isMoveOrderedByPlayer: false);
		}
		else if (room != null)
		{
			MoveToRoom(room, allowMoveInterruption: false, isMoveOrderedByPlayer: false, triggerTutorialEvent: false);
		}
	}

	public void OnMoveByAI()
	{
		nextMoveDoorMoveTarget = null;
		nextMoveRoomMoveTarget = null;
	}

	private void RequestMoveToPosition(Vector3 position, MoveTargetReachedHandler onMoveTargetReached = null, bool allowMoveInterruption = true, Room moveInterruptionAllowedRoom = null, bool cancelCurrentNPCInteraction = true, float approachDistance = 0f)
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.RequestMoveToPosition should only be called on owner side!");
			return;
		}
		if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > RequestMoveToPosition but hero is dead");
			return;
		}
		CancelItemGathering();
		if (cancelCurrentNPCInteraction)
		{
			EndNPCInteraction();
		}
		AICpnt.AllowMoveInterruption(allowMoveInterruption, moveInterruptionAllowedRoom);
		MoverCpnt.MoveToPosition(position, onMoveTargetReached, forceRePath: false, approachDistance);
	}

	private void OnDoorReached()
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError("{0} > Hero.OnDoorReached should only be called on owner side!", base.name);
			return;
		}
		if (doorMoveTarget == null)
		{
			Diagnostics.LogError("{0} > Hero.OnDoorReached: this.doorMoveTarget is null!", base.name);
			return;
		}
		LockInteractions();
		AICpnt.IsActive = false;
		if (nextMoveRoomMoveTarget != null)
		{
			IsUsable = false;
		}
		if (allowAnim)
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DisplayDoorOpening, doorMoveTarget.transform.position - base.transform.position);
		}
		if (!doorMoveTarget.IsOpening)
		{
			doorMoveTarget.OpenByHeroOrMob(nextMoveRoomMoveTarget, this);
		}
	}

	private void RPC_DisplayDoorOpening(Vector3 animDirection)
	{
		spriteAnim.SetAnimDirection(animDirection);
		spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: true);
	}

	private void RPC_DisplayDoorOpeningStop()
	{
		spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: false);
	}

	private void OnItemReached()
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.OnItemReached should only be called on owner side!");
		}
		else if (!(itemMoveTarget == null))
		{
			StartItemGathering(itemMoveTarget);
			itemMoveTarget = null;
		}
	}

	private void OnHeroReached()
	{
		if (HasCrystal)
		{
			Diagnostics.Log(base.name + " > Cannot recruit heroes while carrying the crystal");
		}
		else if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.OnHeroReached should only be called on owner side!");
		}
		else if (heroMoveTarget != null && !heroMoveTarget.IsRecruited)
		{
			heroMoveTarget.DisplayRecruitmentDialog();
		}
	}

	private void OnNPCReached()
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.OnNPCReached should only be called on owner side!");
		}
		else if (npcMoveTarget != null)
		{
			npcMoveTarget.BeginInteraction(this);
			IsInteracting = true;
		}
	}

	private void OnSpecialMajorModuleReached()
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.OnSpecialMajorModuleReached should only be called on owner side!");
		}
		else if (specialMajorModuleMoveTarget != null)
		{
			specialMajorModuleMoveTarget.Interact(this);
		}
	}

	private void OnMoveStopped()
	{
		if (destRoomMarkerRnd != null)
		{
			destRoomMarkerRnd.enabled = (nextMoveDoorMoveTarget != null || nextMoveRoomMoveTarget != null);
		}
	}

	private void Mover_OnMoveDirectionChanged(Vector3 direction)
	{
		crystalContainer.localPosition = direction.normalized * crystalDistanceToPlayer + Vector3.up * crystalHeight;
	}

	private void ResetMoveTargets()
	{
		nextMoveDoorMoveTarget = null;
		nextMoveRoomMoveTarget = null;
		roomMoveTarget = null;
		heroMoveTarget = null;
		npcMoveTarget = null;
		doorMoveTarget = null;
		crystalMoveTarget = null;
		specialMajorModuleMoveTarget = null;
		CrystalSlotMoveTarget = null;
		itemMoveTarget = null;
	}

	public void AddSkill(string skillName)
	{
		AddSkill(Databases.GetDatabase<SkillConfig>().GetValue(skillName));
	}

	public void AddSkill(SkillConfig skillConfig)
	{
		skillConfig.Init();
		if (skillConfig.IsActive)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(activeSkillPfb, base.transform.position, Quaternion.identity);
			gameObject.transform.parent = base.transform;
			gameObject.name = skillConfig.Name;
			ActiveSkill component = gameObject.GetComponent<ActiveSkill>();
			component.Init(base.NetSyncElement.OwnerPlayerID, skillConfig, this);
			activeSkills.Add(component);
			FilterActiveSkills();
		}
		else
		{
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(passiveSkillPfb, base.transform.position, Quaternion.identity);
			gameObject2.transform.parent = base.transform;
			gameObject2.name = skillConfig.Name;
			PassiveSkill passiveSkill = gameObject2.AddComponent<PassiveSkill>();
			passiveSkill.Init(base.NetSyncElement.OwnerPlayerID, skillConfig, this);
			passiveSkills.Add(passiveSkill);
			FilterPassiveSkills();
		}
		if (this.OnSkillsChanged != null)
		{
			this.OnSkillsChanged();
		}
	}

	public void RemoveSkill(string skillName)
	{
		RemoveSkill(Databases.GetDatabase<SkillConfig>().GetValue(skillName));
	}

	public void RemoveSkill(SkillConfig skillConfig)
	{
		if (skillConfig.IsActive)
		{
			for (int i = 0; i < activeSkills.Count; i++)
			{
				ActiveSkill activeSkill = activeSkills[i];
				if (activeSkill.Config.Name == skillConfig.Name)
				{
					if (activeSkill.IsActivated)
					{
						activeSkill.Deactivate();
					}
					activeSkills.RemoveAt(i);
					i--;
					UnityEngine.Object.Destroy(activeSkill.gameObject);
					break;
				}
			}
			FilterActiveSkills();
		}
		else
		{
			for (int j = 0; j < passiveSkills.Count; j++)
			{
				PassiveSkill passiveSkill = passiveSkills[j];
				if (passiveSkill.Config.Name == skillConfig.Name)
				{
					passiveSkills.RemoveAt(j);
					j--;
					UnityEngine.Object.Destroy(passiveSkill.gameObject);
					break;
				}
			}
			FilterPassiveSkills();
		}
		if (this.OnSkillsChanged != null)
		{
			this.OnSkillsChanged();
		}
	}

	private void FilterActiveSkills()
	{
		if (FilteredActiveSkills == null)
		{
			FilteredActiveSkills = new List<ActiveSkill>();
		}
		else
		{
			FilteredActiveSkills.Clear();
		}
		for (int i = 0; i < activeSkills.Count; i++)
		{
			ActiveSkill activeSkill = activeSkills[i];
			ActiveSkill activeSkill2 = null;
			for (int j = 0; j < FilteredActiveSkills.Count; j++)
			{
				if (FilteredActiveSkills[j].Config.BaseName == activeSkill.Config.BaseName)
				{
					activeSkill2 = FilteredActiveSkills[j];
					break;
				}
			}
			if (activeSkill2 != null)
			{
				if (activeSkill.Config.Level > activeSkill2.Config.Level)
				{
					FilteredActiveSkills.Remove(activeSkill2);
					FilteredActiveSkills.Add(activeSkill);
				}
			}
			else
			{
				FilteredActiveSkills.Add(activeSkill);
			}
		}
	}

	private void FilterPassiveSkills()
	{
		RemovePassiveSkillsEffects();
		if (FilteredPassiveSkills == null)
		{
			FilteredPassiveSkills = new List<PassiveSkill>();
		}
		else
		{
			FilteredPassiveSkills.Clear();
		}
		for (int i = 0; i < passiveSkills.Count; i++)
		{
			PassiveSkill passiveSkill = passiveSkills[i];
			PassiveSkill passiveSkill2 = null;
			for (int j = 0; j < FilteredPassiveSkills.Count; j++)
			{
				if (FilteredPassiveSkills[j].Config.BaseName == passiveSkill.Config.BaseName)
				{
					passiveSkill2 = FilteredPassiveSkills[j];
					break;
				}
			}
			if (passiveSkill2 != null)
			{
				if (passiveSkill.Config.Level > passiveSkill2.Config.Level)
				{
					FilteredPassiveSkills.Remove(passiveSkill2);
					FilteredPassiveSkills.Add(passiveSkill);
				}
			}
			else
			{
				FilteredPassiveSkills.Add(passiveSkill);
			}
		}
		ApplyPassiveSkillsEffects();
	}

	public void ActivateActiveSkill(int activeSkillIndex)
	{
		if (activeSkillIndex < 0 || activeSkillIndex > FilteredActiveSkills.Count - 1)
		{
			Diagnostics.Log("Unable to activate active skill #" + activeSkillIndex + ": INDEX OUT OF BOUNDS!");
		}
		else
		{
			FilteredActiveSkills[activeSkillIndex].Activate();
		}
	}

	public void DeactivateActiveSkills()
	{
		foreach (ActiveSkill filteredActiveSkill in FilteredActiveSkills)
		{
			if (filteredActiveSkill.IsActivated)
			{
				filteredActiveSkill.Deactivate();
			}
		}
	}

	public ActiveSkill ActiveSkillLesserThan(SkillConfig skillConf)
	{
		for (int i = 0; i < FilteredActiveSkills.Count; i++)
		{
			if (FilteredActiveSkills[i].Config.BaseName == skillConf.BaseName && FilteredActiveSkills[i].Config.Level < skillConf.Level)
			{
				return FilteredActiveSkills[i];
			}
		}
		return null;
	}

	public PassiveSkill PassiveSkillLesserThan(SkillConfig skillConf)
	{
		for (int i = 0; i < FilteredPassiveSkills.Count; i++)
		{
			if (FilteredPassiveSkills[i].Config.BaseName == skillConf.BaseName && FilteredPassiveSkills[i].Config.Level < skillConf.Level)
			{
				return FilteredPassiveSkills[i];
			}
		}
		return null;
	}

	private void RemovePassiveSkillsEffects()
	{
		foreach (PassiveSkill filteredPassiveSkill in FilteredPassiveSkills)
		{
			filteredPassiveSkill.RemoveEffects();
		}
	}

	private void ApplyPassiveSkillsEffects()
	{
		foreach (PassiveSkill filteredPassiveSkill in FilteredPassiveSkills)
		{
			filteredPassiveSkill.ApplyEffects();
		}
	}

	public static void SwitchHero(int indexInc)
	{
		if (LocalPlayerActiveRecruitedHeroes.Count < 1)
		{
			Diagnostics.LogError("No more heroes to switch to!");
			return;
		}
		int num = 0;
		if (selectedHeroes.Count == 1)
		{
			num = localPlayerActiveRecruitedHeroes.IndexOf(selectedHeroes[0]);
		}
		num = (num + indexInc + localPlayerActiveRecruitedHeroes.Count) % localPlayerActiveRecruitedHeroes.Count;
		localPlayerActiveRecruitedHeroes[num].Select();
	}

	public static void SelectAllHeroes()
	{
		bool unselectOthers = true;
		foreach (Hero localPlayerActiveRecruitedHero in localPlayerActiveRecruitedHeroes)
		{
			localPlayerActiveRecruitedHero.Select(recordSelectTime: true, unselectOthers);
			unselectOthers = false;
		}
	}

	/// <summary>
	/// Method for unlocking a new hero for the Player
	/// </summary>
	/// <param name="heroConfig"></param>
	/// <param name="checkStatus"></param>
	public static void UnlockHero(HeroConfig heroConfig, bool checkStatus = true)
	{
		int num = UserProfile.Data.HeroesGameStats.FindIndex((HeroGameStatsData h) => h.ConfigName == heroConfig.Name);
		if (num < 0)
		{
			Diagnostics.LogError("No game stats for " + heroConfig.Name + "!");
			return;
		}
		HeroGameStatsData value = UserProfile.Data.HeroesGameStats[num];
		if (heroConfig.IsHidden())
		{
			return;
		}
		if (checkStatus && value.Status != HeroStatus.Discovered)
		{
			Diagnostics.LogError((string)heroConfig.Name + " not discovered (" + value.Status + ")!");
			return;
		}
		AchievementManagerDOTE achievementManagerDOTE = SingletonManager.Get<AchievementManagerDOTE>();
		achievementManagerDOTE.IncrementStatistic((heroConfig.Name.ToString().ToUpper() + "_UNLOCKS").ToEnum<StatisticName>());
		achievementManagerDOTE.IncrementStatistic(StatisticName.ANY_HERO_UNLOCKS);
		if (value.Status == HeroStatus.Unlocked)
		{
			Diagnostics.LogError(heroConfig.Name + " already unlocked!");
			return;
		}
		value.Status = HeroStatus.Unlocked;
		UserProfile.Data.HeroesGameStats[num] = value;
		UserProfile.SaveToFile();
		IGuiService service = Services.GetService<IGuiService>();
		Diagnostics.Assert(service != null);
		if (service.GuiPanelHelper.TryGetGuiElement(heroConfig.Name, out GuiElement guiElement))
		{
			string newValue = AgeLocalizer.Instance.LocalizeString(guiElement.Title);
			Dungeon dungeon = SingletonManager.Get<Dungeon>();
			if (dungeon != null)
			{
				dungeon.EnqueueUnlockNotification(AgeLocalizer.Instance.LocalizeString("%Notification_HeroUnlocked").Replace("$HeroName", newValue), NotificationType.MiscUnlock, null, heroConfig.Name, foreGround: false, heroConfig.Name);
			}
		}
	}

	public static List<Hero> GetAllPlayersActiveRecruitedHeroes()
	{
		List<Hero> list = new List<Hero>();
		list.AddRange(localPlayerActiveRecruitedHeroes);
		list.AddRange(remotePlayersActiveRecruitedHeroes);
		return list;
	}

	public static List<Hero> GetLevelWinningHeroes()
	{
		List<Hero> list = new List<Hero>();
		for (int i = 0; i < LocalPlayerActiveRecruitedHeroes.Count; i++)
		{
			Hero hero = LocalPlayerActiveRecruitedHeroes[i];
			if (hero.WasInExitRoomAtExitTime)
			{
				list.Add(hero);
			}
		}
		for (int j = 0; j < RemotePlayersActiveRecruitedHeroes.Count; j++)
		{
			Hero hero = RemotePlayersActiveRecruitedHeroes[j];
			if (hero.WasInExitRoomAtExitTime)
			{
				list.Add(hero);
			}
		}
		return list;
	}

	public void InitForGameStart(ulong ownerPlayerID, StaticString heroDescName, Room spawnRoom)
	{
		Init(ownerPlayerID, heroDescName, spawnRoom, isRecruited: true, registerRecruitment: true, 1, 0, hasOperatingBonus: false, null, displayRecruitmentDialog: true, consumeLevelUpFood: true, updateDiscoverableHeroPool: true, 1, null, isStartingHero: true);
	}

	public void InitForEvent(ulong ownerPlayerID, StaticString heroDescName, Room spawnRoom, bool displayRecruitmentDialog, int level, bool recruitable = true)
	{
		Init(ownerPlayerID, heroDescName, spawnRoom, isRecruited: false, registerRecruitment: false, level, 0, hasOperatingBonus: false, null, displayRecruitmentDialog, consumeLevelUpFood: false, updateDiscoverableHeroPool: true, -1, null, isStartingHero: false, 0, recruitable);
	}

	public void InitForSaveRestore(ulong ownerPlayerID, StaticString heroDescName, Room spawnRoom, bool isRecruited, int initLevel, int unlockLevel, bool hasOperatingBonus, Dictionary<StaticString, List<ItemPersistentData>> initItemsByCategory, int floorRecruited, StaticString[] permanentDescriptors, bool isStartingHero, int currentRespawnRoomCount, bool recruitable)
	{
		Init(ownerPlayerID, heroDescName, spawnRoom, isRecruited, registerRecruitment: false, initLevel, unlockLevel, hasOperatingBonus, initItemsByCategory, displayRecruitmentDialog: false, consumeLevelUpFood: false, updateDiscoverableHeroPool: false, floorRecruited, permanentDescriptors, isStartingHero, currentRespawnRoomCount, recruitable);
	}

	private void Init(ulong ownerPlayerID, StaticString heroDescName, Room spawnRoom, bool isRecruited = false, bool registerRecruitment = true, int initLevel = 1, int unlockLevel = 0, bool hasOperatingBonus = false, Dictionary<StaticString, List<ItemPersistentData>> initItemsByCategory = null, bool displayRecruitmentDialog = true, bool consumeLevelUpFood = true, bool updateDiscoverableHeroPool = true, int floorRecruited = 1, StaticString[] permanentDescriptors = null, bool isStartingHero = false, int currentRespawnRoomCount = 0, bool recruitable = true)
	{
		Init(ownerPlayerID);
		base.NetSyncElement.OnOwnerLeft += OnOwnerLeft;
		base.NetSyncElement.OnOwnerChanged += OnOwnerChanged;
		gameControlManager = Services.GetService<IGameControlService>();
		selectableManager = SingletonManager.Get<SelectableManager>();
		activeSkills = new List<ActiveSkill>();
		FilteredActiveSkills = new List<ActiveSkill>();
		passiveSkills = new List<PassiveSkill>();
		FilteredPassiveSkills = new List<PassiveSkill>();
		FloorRecruited = floorRecruited;
		IsStartingHero = isStartingHero;
		CurrentRespawnRoomCount = currentRespawnRoomCount;
		IsRecruitable = recruitable;
		if (operatingBonusGenericSimDesc == null)
		{
			operatingBonusGenericSimDesc = SimMonoBehaviour.GetDBDescriptorByName("Hero_Operating_Bonus");
		}
		Config = Databases.GetDatabase<HeroConfig>().GetValue(heroDescName);
		if (Config == null)
		{
			Diagnostics.LogError("Unable to find hero config by name = " + heroDescName + " in config pool");
			return;
		}
		SimulationDescriptor dBDescriptorByName = SimMonoBehaviour.GetDBDescriptorByName(Config.Name);
		base.name = dBDescriptorByName.Name.ToString().Replace("Hero_", string.Empty) + "_" + base.name;
		AddSimDescriptor(dBDescriptorByName);
		IGuiService service = Services.GetService<IGuiService>();
		Diagnostics.Assert(service != null);
		if (service.GuiPanelHelper.TryGetGuiElement(Config.Name, out GuiElement guiElement))
		{
			LocalizedName = AgeLocalizer.Instance.LocalizeString(guiElement.Title);
		}
		else
		{
			Diagnostics.LogError("Unable to find localized name for " + base.name);
		}
		if (Config.SituationDialogCount == null)
		{
			Config.SituationDialogCount = new Dictionary<StaticString, int>();
			SituationDialogType[] enumValues = GenericUtilities.GetEnumValues<SituationDialogType>();
			foreach (SituationDialogType situationDialogType in enumValues)
			{
				string str = "%" + Config.Name + "_SituationDialog_" + situationDialogType.ToString();
				int num = 1;
				string b;
				while (AgeLocalizer.Instance.LocalizeString(b = str + num.ToString()) != b)
				{
					num++;
				}
				Config.SituationDialogCount.Add(situationDialogType.ToString(), num - 1);
			}
		}
		spriteAnim.OverrideClipsFromPath("SpriteAnimations/Hero/" + Config.Name.ToString().Replace("Hero_", string.Empty));
		tacticalMapElementAnim.OverrideClipsFromPath("SpriteAnimations/Hero/" + Config.Name.ToString().Replace("Hero_", string.Empty));
		crystalSprite.OverrideClipsFromPath("SpriteAnimations/Modules/" + SimulationProperties.SpecialModule_Crystal + "_" + SingletonManager.Get<Dungeon>().CrystalType);
		base.transform.position += 0.001f * Vector3.up;
		spawnRoom.OnHeroEnter(this);
		HasOperatingBonus = hasOperatingBonus;
		Bind();
		attackerCpnt.Init(Config.AttackType);
		if (!isRecruited)
		{
			heroAttackTargetCpnt.Init(isActive: true, AITargetType.FreeHero);
		}
		else
		{
			heroAttackTargetCpnt.Init(isActive: true, Config.AITargetType);
		}
		mobsAggroAttackTargetCpnt.Init(isActive: false);
		levelConfigs = new List<HeroLevelConfig>();
		levelUpModifiersByLevel = new List<Dictionary<StaticString, float>>();
		int num2 = 1;
		while (true)
		{
			string levelDescriptorName = GetLevelDescriptorName(num2);
			IDatabase<HeroLevelConfig> database = Databases.GetDatabase<HeroLevelConfig>();
			HeroLevelConfig value = database.GetValue(levelDescriptorName);
			if (value == null)
			{
				break;
			}
			if (value.Skills != null)
			{
				value.HasActiveSkills = false;
				for (int j = 0; j < value.Skills.Length; j++)
				{
					if (value.Skills[j].StartsWith("Skill_A"))
					{
						value.HasActiveSkills = true;
						break;
					}
				}
				value.HasPassiveSkills = false;
				for (int k = 0; k < value.Skills.Length; k++)
				{
					if (value.Skills[k].StartsWith("Skill_P"))
					{
						value.HasPassiveSkills = true;
						break;
					}
				}
			}
			levelConfigs.Add(value);
			Dictionary<StaticString, float> dictionary = new Dictionary<StaticString, float>();
			SimulationDescriptor dBDescriptorByName2 = SimMonoBehaviour.GetDBDescriptorByName(levelDescriptorName);
			if (dBDescriptorByName2 != null)
			{
				SimulationObject simObj = GetSimObj();
				SimulationModifierDescriptor[] simulationModifierDescriptors = dBDescriptorByName2.SimulationModifierDescriptors;
				foreach (SimulationModifierDescriptor simulationModifierDescriptor in simulationModifierDescriptors)
				{
					dictionary.Add(simulationModifierDescriptor.TargetPropertyName, simulationModifierDescriptor.ComputeValue(simObj, simObj, SimulationPropertyRefreshContext.GetContext()));
				}
			}
			levelUpModifiersByLevel.Add(dictionary);
			num2++;
		}
		IsUsable = true;
		string text = "VFX/LevelUp/Hero";
		GameObject gameObject = (GameObject)Resources.Load(text, typeof(GameObject));
		if (gameObject != null)
		{
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(gameObject, base.transform.position, base.transform.rotation);
			gameObject2.transform.parent = base.transform;
			levelUpVfxParticles = gameObject2.GetComponent<ParticleSystem>();
		}
		else
		{
			Diagnostics.LogWarning("No level up VFX found for " + base.name + " (path=" + text + ")");
		}
		RefreshSim();
		base.HealthCpnt.InitHealth(eligibleToInstaRegen: true);
		Vector3 localScale = base.HealthCpnt.HealthBar.BarContainer.localScale;
		initalHealthBarScaleY = localScale.y;
		Vector3 localScale2 = base.HealthCpnt.HealthBar.BarContainer.localScale;
		initalHealthBarScaleX = localScale2.x;
		base.HealthCpnt.HealthBar.UpdateHealthBarHeight();
		base.HealthCpnt.OnHealthChanged += OnHealthChanged;
		InitEquipmentSlots(initItemsByCategory);
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			for (int m = 1; m <= initLevel; m++)
			{
				LevelUp(playFeedback: false, consumeLevelUpFood);
			}
		}
		UnlockLevel = unlockLevel;
		PermanentDescriptors = new List<SimulationDescriptor>();
		if (permanentDescriptors != null)
		{
			foreach (StaticString x in permanentDescriptors)
			{
				AddPermanentDescriptor(x);
			}
		}
		SelectionCategoryConfig currentCategoryConfig = selectableManager.GetCurrentCategoryConfig();
		if (currentCategoryConfig.EnableNonContextualControl && !isRecruited && displayRecruitmentDialog && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			IGameCameraService service2 = Services.GetService<IGameCameraService>();
			if (!service2.IsTacticalMapActive() && !service2.IsSwitchingCamera)
			{
				service2.Focus(base.transform.position, LerpType.Smoothed, -1f);
				if (Services.GetService<IInputService>().CurrentControlScheme != ControlScheme.XBoxOneController)
				{
					service2.ZoomIn();
				}
			}
			DisplayRecruitmentDialog();
			base.RoomElement.ParentRoom.SelectableForMove.Select(silent: true);
		}
		dungeon.RegisterHeroSpawn(this, updateDiscoverableHeroPool);
		if (isRecruited && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Recruit(consumeFood: false, registerRecruitment, requestAccessToServer: false);
		}
		UpdateAICpnt();
		if (IsRecruited && UnlockLevel > 0 && dungeon.Level >= UnlockLevel && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			if (dungeon.Level > UnlockLevel)
			{
				Diagnostics.LogError((string)Config.Name + " should have been unlocked when reaching level " + UnlockLevel + "!");
			}
			UnlockHero(Config);
			UnlockLevel = 0;
		}
		gameEventManager.OnGameVictory += OnGameVictory;
		spriteAnim.MonitorAnimEvent("OnDeathEnd", OnDeathEndAnimEvent);
	}

	private void UpdateAICpnt()
	{
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			if (GetSimPropertyValue(SimulationProperties.RepairFirst) > 0f)
			{
				AICpnt.Init("RepairFirstHero");
			}
			else if (!IsRecruited)
			{
				AICpnt.Init($"Free{Config.Name}");
			}
			else
			{
				AICpnt.Init(Config.Name);
			}
			AICpnt.OnTargetChanged += OnAITargetChanged;
		}
		else
		{
			AICpnt.IsActive = false;
		}
	}

	private void OnOwnerLeft()
	{
		if (gameNetManager.IsServer())
		{
			GiveToPlayer(gameNetManager.GetLocalPlayerID(), checkIfOwnedByLocalPlayer: false);
		}
	}

	private void OnOwnerChanged()
	{
		UpdateAICpnt();
	}

	private void DisplayPlayerName()
	{
		if (playerNamePanel == null)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(playerNamePanelPfb, Vector3.zero, Quaternion.identity);
			gameObject.transform.parent = SingletonManager.Get<MainGameScreen>().HeroPlayerNameContainerTfm;
			gameObject.GetComponent<GameToScreenPositionSync>().GameEntityTfm = base.transform;
			playerNamePanel = gameObject.GetComponent<HeroPlayerNamePanel>();
		}
		HeroPlayerNamePanel heroPlayerNamePanel = playerNamePanel;
		string playerName = gameNetManager.GetPlayerName(base.NetSyncElement.OwnerPlayerID);
		Vector3 containerScale = dungeon.ContainerScale;
		heroPlayerNamePanel.DisplayPlayerName(playerName, containerScale.y * GetSimPropertyValue(SimulationProperties.HealthBarHeight));
	}

	private void HidePlayerName()
	{
		if (playerNamePanel != null)
		{
			playerNamePanel.Hide();
		}
	}

	public void RegisterAttackingMob(Mob mob)
	{
		attackingMobCount++;
		OnAttackingMobCountChanged();
	}

	public void UnregisterAttackingMob(Mob mob)
	{
		attackingMobCount--;
		OnAttackingMobCountChanged();
	}

	public float GetHiringFoodCost()
	{
		return Config.RecruitmentFoodCost + GenericUtilities.RoundHalfAwayFromZero((float)(base.Level * base.Level) * GameConfig.GetGameConfig().HiringFoodCoef);
	}

	public bool CanBeRecruited(bool consumeFood = true, bool checkPlayerMaxHeroCount = true, bool displayErrors = true)
	{
		if (IsRecruited)
		{
			Diagnostics.Log(base.name + " > Hero.CanBeRecruited: cannot recruit an already recruited hero!");
			return false;
		}
		if (IsDismissing)
		{
			Diagnostics.Log(base.name + " > Hero.CanBeRecruited: cannot recruit a dismissed hero!");
			return false;
		}
		if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.Log(base.name + " > Hero.CanBeRecruited: cannot recruit a dead hero!");
			return false;
		}
		if (checkPlayerMaxHeroCount && (float)localPlayerActiveRecruitedHeroes.Count >= GameConfig.GetGameConfig().PlayerMaxHeroCount.GetValue())
		{
			Diagnostics.Log(base.name + " > Hero.CanBeRecruited: player max hero count reached!");
			if (displayErrors)
			{
				dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroRecruitmentFailMaxPlayerHeroCount"));
			}
			return false;
		}
		if (localPlayerActiveRecruitedHeroes.Count + remotePlayersActiveRecruitedHeroes.Count >= GameConfig.GetGameConfig().MaxHeroCount)
		{
			Diagnostics.Log(base.name + " > Hero.CanBeRecruited: total max hero count reached!");
			if (displayErrors)
			{
				dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroRecruitmentFailMaxTotalHeroCount"));
			}
			return false;
		}
		if (consumeFood && Player.LocalPlayer.FoodStock < GetHiringFoodCost())
		{
			Diagnostics.Log(base.name + " > Hero.CanBeRecruited: not enough food!");
			if (displayErrors)
			{
				dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroRecruitmentFailResources"));
			}
			return false;
		}
		return true;
	}

	public void Recruit(bool consumeFood = true, bool registerRecruitment = true, bool requestAccessToServer = true)
	{
		if (!CanBeRecruited(consumeFood))
		{
			Diagnostics.Log(base.name + " > Hero.Recruit: cannot be recruited!");
			return;
		}
		bool flag = consumeFood;
		bool flag2 = false;
		if (consumeFood && gameNetManager.IsMultiplayerSession() && MultiplayerConfig.SplitFIS)
		{
			flag = false;
			flag2 = true;
		}
		if (consumeFood)
		{
			FloorRecruited = dungeon.Level;
		}
		if (requestAccessToServer)
		{
			base.NetSyncElement.SendRPCToServer(UniqueIDRPC.Hero_RequestRecruit, gameNetManager.GetLocalPlayerID(), flag, flag2, registerRecruitment);
		}
		else if (flag2 && !Player.LocalPlayer.ConsumeFood(GetHiringFoodCost()))
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroRecruitmentFailResources"));
		}
		else
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoRecruit, gameNetManager.GetLocalPlayerID(), registerRecruitment, false);
		}
	}

	private void RPC_RequestRecruit(ulong recruiterPlayerID, bool consumeFoodOnServer, bool consumeFoodOnRecruiter, bool registerRecruitment)
	{
		if (!gameNetManager.IsServer())
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_RequestRecruit should only be called on server side!");
			return;
		}
		if (!CanBeRecruited(consumeFoodOnServer, checkPlayerMaxHeroCount: false, displayErrors: false))
		{
			Diagnostics.Log(base.name + " > Hero.RPC_RequestRecruit: cannot be recruited!");
			return;
		}
		if (consumeFoodOnServer && !Player.LocalPlayer.ConsumeFood(GetHiringFoodCost()))
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroRecruitmentFailResources"));
			return;
		}
		if (consumeFoodOnRecruiter || consumeFoodOnServer)
		{
			SingletonManager.Get<AchievementManagerDOTE>().IncrementStatistic(StatisticName.HIRED_HEROES);
		}
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoRecruit, recruiterPlayerID, registerRecruitment, consumeFoodOnRecruiter);
	}

	private void RPC_DoRecruit(ulong recruiterPlayerID, bool registerRecruitment, bool consumeFoodOnRecruiter)
	{
		bool consumeFood = consumeFoodOnRecruiter && gameNetManager.GetLocalPlayerID() == recruiterPlayerID;
		bool checkPlayerMaxHeroCount = gameNetManager.GetLocalPlayerID() == recruiterPlayerID;
		if (!CanBeRecruited(consumeFood, checkPlayerMaxHeroCount, displayErrors: false))
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_DoRecruit: cannot be recruited!");
			return;
		}
		if (consumeFoodOnRecruiter && gameNetManager.GetLocalPlayerID() == recruiterPlayerID && !Player.LocalPlayer.ConsumeFood(GetHiringFoodCost()))
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_DoRecruit: not enough resources!");
			return;
		}
		RecentlyDeceasedHeroes.Clear();
		if (base.NetSyncElement.OwnerPlayerID != recruiterPlayerID)
		{
			base.NetSyncElement.ChangeOwner(recruiterPlayerID);
		}
		if (achievementManager == null)
		{
			achievementManager = SingletonManager.Get<AchievementManagerDOTE>();
		}
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			AICpnt.UpdateAIType(Config.Name);
			ModifyLocalActiveHeroes(add: true, this);
			if (localPlayerActiveRecruitedHeroes.Count >= 4)
			{
				achievementManager.IncrementStatistic(StatisticName.FULL_TEAM);
			}
		}
		else
		{
			remotePlayersActiveRecruitedHeroes.Add(this);
			DisplayPlayerName();
		}
		gameEventManager.TriggerHeroListChangedEvent();
		if (base.NetSyncElement.IsOwnedByLocalPlayer() && registerRecruitment)
		{
			dungeon.RegisterHeroRecruitment(this);
		}
		if (heroAttackTargetCpnt.Type != Config.AITargetType)
		{
			heroAttackTargetCpnt.UpdateType(Config.AITargetType);
		}
		IsRecruited = true;
		base.HealthCpnt.HealthBar.ShowEvenWhenHolderIsHidden = true;
		if (base.RoomElement.ParentRoom != null)
		{
			base.RoomElement.ParentRoom.UpdateLightsOnHeroEnter();
		}
		if (SelectedHeroes.Count == 0 && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Select();
		}
		if (!IsStartingHero)
		{
			achievementManager.AddToStatistic(StatisticName.RECRUITMENT_NEGATIVE, -1f);
		}
		gameEventManager.OnDungeonTurnChanged += GameEventManager_OnDungeonTurnChanged;
		if (Hero.OnHeroRecruited != null)
		{
			Hero.OnHeroRecruited();
		}
	}

	private void GameEventManager_OnDungeonTurnChanged()
	{
		if (GetSimPropertyValue(SimulationProperties.RespawnRoomCount) > 0f)
		{
			CurrentRespawnRoomCount++;
		}
	}

	public void Dismiss()
	{
		if (!IsRecruited)
		{
			Diagnostics.LogError(base.name + " > Hero.Dismiss: cannot dismiss a free hero!");
			return;
		}
		if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.LogError(base.name + " > Hero.Dismiss: cannot dismiss a dead hero!");
			return;
		}
		if (LocalPlayerActiveRecruitedHeroes.Count + RemotePlayersActiveRecruitedHeroes.Count <= 1)
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroDismissFailTeamSize"));
			return;
		}
		if (HasCrystal)
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroDismissFailCrystal"));
			return;
		}
		Player.LocalPlayer.AddFood((float)(base.Level * base.Level) + GameConfig.GetGameConfig().DismissingFoodCoef);
		if (selectedHeroes.Contains(this))
		{
			Unselect();
		}
		SpawnDeadHeroLoot();
		AICpnt.IsActive = false;
		MoverCpnt.StopMove();
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoDismiss);
	}

	private void RPC_DoDismiss()
	{
		IsDismissing = true;
		if (TamedMob != null && TamedMob.HealthCpnt != null)
		{
			TamedMob.HealthCpnt.Kill();
		}
		gameEventManager.OnDungeonTurnChanged -= GameEventManager_OnDungeonTurnChanged;
		dungeon.AddToDiscoverableHeroPool(Config);
		heroAttackTargetCpnt.SetActive(active: false);
		LockInteractions();
		RemoveFromDungeon();
		gameEventManager.TriggerHeroListChangedEvent();
		dismissVFX.Play(withChildren: true);
		Invoke("Hide", hideDelayOnDismiss);
		UnityEngine.Object.Destroy(base.gameObject, destroyDelayOnDismiss);
	}

	public void GiveToPlayer(ulong playerID, bool checkIfOwnedByLocalPlayer = true)
	{
		if (checkIfOwnedByLocalPlayer && !base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError("Cannot give {0} to player #{1}: hero is owned by another player! ({2})", base.name, playerID, base.NetSyncElement.OwnerPlayerID);
			return;
		}
		if (HasCrystal)
		{
			Diagnostics.LogError("Cannot give {0} to player #{1}: hero is carrying the crystal!", base.name, playerID);
			return;
		}
		if (IsRecruited && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Unselect(triggerHeroSelectionEvent: true, assertAtLeastOneHeroIsSelected: false);
			ModifyLocalActiveHeroes(add: false, this);
			remotePlayersActiveRecruitedHeroes.Add(this);
		}
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_GiveToPlayer, playerID);
	}

	private void RPC_GiveToPlayer(ulong playerID)
	{
		base.NetSyncElement.ChangeOwner(playerID);
		if (IsRecruited)
		{
			UnlockLevel = dungeon.Level + Config.UnlockLevelCount;
			if (base.NetSyncElement.IsOwnedByLocalPlayer())
			{
				ModifyLocalActiveHeroes(add: true, this);
				remotePlayersActiveRecruitedHeroes.Remove(this);
				HidePlayerName();
				if (TamedMob != null)
				{
					DoTameMob(TamedMob, ownerChanged: true);
				}
				if (localPlayerActiveRecruitedHeroes.Count == 1 || (selectedHeroes.Count > 1 && selectedHeroes.Count == localPlayerActiveRecruitedHeroes.Count - 1))
				{
					Select(recordSelectTime: true, unselectOthers: false);
				}
			}
			else
			{
				DisplayPlayerName();
			}
		}
		gameEventManager.TriggerHeroListChangedEvent();
	}

	public int GetHealFoodCost()
	{
		return Mathf.Max(GenericUtilities.RoundHalfAwayFromZeroToInt((float)GetHealAmount() * GetSimPropertyValue(SimulationProperties.HealFoodCostCoeff)), 1);
	}

	public int GetRestScienceCost()
	{
		float num = 0f;
		float skillLevelScienceCoef = GameConfig.GetGameConfig().SkillLevelScienceCoef;
		foreach (ActiveSkill filteredActiveSkill in FilteredActiveSkills)
		{
			num += (float)(filteredActiveSkill.GetRemainingTurns() * filteredActiveSkill.Config.Level) * skillLevelScienceCoef;
		}
		return Mathf.CeilToInt(num);
	}

	public int GetHealAmount()
	{
		return GenericUtilities.RoundHalfAwayFromZeroToInt(base.HealthCpnt.GetMaxHealth() * GameConfig.GetGameConfig().HeroHealPct);
	}

	public bool Heal()
	{
		bool flag = TamedMob != null && !TamedMob.HealthCpnt.IsFullLive();
		float num = 0f;
		if (flag)
		{
			num = GetSimPropertyValue(SimulationProperties.MobTamedHealPct) / 100f;
			flag = (flag && num > 0f);
		}
		if (base.HealthCpnt.IsFullLive() && !flag)
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroHealFailFullLife"));
			return false;
		}
		if (dungeon.CurrentGamePhase == GamePhase.Strategy && !dungeon.ShipConfig.ForbidStrategyHealthRegen)
		{
			return false;
		}
		if (!base.HealthCpnt.IsAlive())
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroHealFailAlreadyDead"));
			return false;
		}
		float num2 = GetHealFoodCost();
		if (!Player.LocalPlayer.ConsumeFood(num2))
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroHealFailResources"));
			return false;
		}
		float num3 = GetHealAmount();
		base.HealthCpnt.AddHealth(num3);
		if (TamedMob != null && TamedMob.HealthCpnt != null)
		{
			TamedMob.HealthCpnt.AddHealth(num3 * num);
		}
		achievementManager.AddToStatistic(StatisticName.HEAL_FOOD_USED, num2);
		return true;
	}

	public bool Rest()
	{
		if (FilteredActiveSkills.Count == 0)
		{
			return false;
		}
		int restScienceCost = GetRestScienceCost();
		if (!Player.LocalPlayer.ConsumeScience(restScienceCost))
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroRestFailResources"));
			return false;
		}
		if (restScienceCost <= 0)
		{
			dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroNothingToRest"));
			return false;
		}
		foreach (ActiveSkill filteredActiveSkill in FilteredActiveSkills)
		{
			if (filteredActiveSkill.GetRemainingTurns() > 0)
			{
				filteredActiveSkill.EndCooldown();
			}
		}
		return true;
	}

	public void Bind()
	{
		if (dungeon == null)
		{
			dungeon = SingletonManager.Get<Dungeon>();
		}
		dungeon.OnRoomPowered += Dungeon_OnRoomPowered;
		MoverCpnt.OnMoveDirectionChanged += Mover_OnMoveDirectionChanged;
	}

	public void Unbind()
	{
		dungeon.OnRoomPowered -= Dungeon_OnRoomPowered;
		MoverCpnt.OnMoveDirectionChanged -= Mover_OnMoveDirectionChanged;
	}

	public void CancelNPCInteraction()
	{
		IsInteracting = false;
		EndNPCInteraction();
	}

	public void EndNPCInteraction()
	{
		if (npcMoveTarget != null)
		{
			npcMoveTarget.HealthCpnt.OnDeath -= OnInteractionTargetDeath;
			if (IsInteracting)
			{
				npcMoveTarget.EndInteraction(this);
			}
			else
			{
				npcMoveTarget.CancelInteraction(this);
			}
			IsInteracting = false;
			attackerCpnt.CanAttack = true;
			npcMoveTarget = null;
		}
	}

	public void ChangeCurrentRoom(Room newRoom)
	{
		if (HasOperatingBonus && base.RoomElement.ParentRoom != newRoom)
		{
			HasOperatingBonus = false;
			gameEventManager.OnDungeonTurnChanged -= OnDungeonTurnChangedDuringAction;
		}
		if (base.RoomElement.ParentRoom != null && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			base.RoomElement.ParentRoom.OnMobDied -= ParentRoom_OnMobDied;
		}
		base.RoomElement.SetParentRoom(newRoom);
		newRoom.UpdateSupportEffectVFXs();
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			base.RoomElement.ParentRoom.OnMobDied += ParentRoom_OnMobDied;
		}
		if (!HasCrystal)
		{
			return;
		}
		CrystalModuleSlot freeCrystalModuleSlot = newRoom.GetFreeCrystalModuleSlot(isExitSlot: true);
		if (freeCrystalModuleSlot != null && CrystalSlotMoveTarget == freeCrystalModuleSlot)
		{
			int num = 1;
			num = ((!gameNetManager.IsMultiplayerSession()) ? 1 : ((!MultiplayerConfig.OneCrystalPerPlayer) ? 1 : gameNetManager.GetPlayerCount()));
			if (dungeon.PluggedOnExitSlotCrystalCount == num - 1)
			{
				PlayLevelSuccessCinematic();
			}
		}
	}

	private void ParentRoom_OnMobDied(Mob obj)
	{
		float simPropertyValue = GetSimPropertyValue(SimulationProperties.FISBonusPerRoomMobKilled);
		if (simPropertyValue > 0f)
		{
			int num = RandomGenerator.RangeInt(0, 3);
			if (num == 2)
			{
				num = 3;
			}
			Player.LocalPlayer.AddFIDS(simPropertyValue, (FIDS)num, displayFeedback: true, triggerDungeonFIDSChangedEvent: true, checkPlayerIsLocal: false);
		}
	}

	public void UpdateSupportEffectVFXs(bool on = true)
	{
		if (on && GetSimPropertyValue(SimulationProperties.SupportHealingEffect) > 0f)
		{
			if (!supportHealingVFX.isPlaying)
			{
				supportHealingVFX.Play(withChildren: true);
			}
		}
		else if (supportHealingVFX.isPlaying)
		{
			supportHealingVFX.Stop(withChildren: true);
		}
		if (on && GetSimPropertyValue(SimulationProperties.SupportBoosterEffect) > 0f)
		{
			if (!supportBoosterVFX.isPlaying)
			{
				supportBoosterVFX.Play(withChildren: true);
			}
		}
		else if (supportBoosterVFX.isPlaying)
		{
			supportBoosterVFX.Stop(withChildren: true);
		}
		if (on && GetSimPropertyValue(SimulationProperties.SupportSonarEffect) > 0f)
		{
			if (!supportSonarVFX.isPlaying)
			{
				supportSonarVFX.Play(withChildren: true);
			}
		}
		else if (supportSonarVFX.isPlaying)
		{
			supportSonarVFX.Stop(withChildren: true);
		}
	}

	public void PlayLevelSuccessCinematic()
	{
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoPlayLevelSuccessCinematic, base.RoomElement.ParentRoom.UniqueID.ID);
	}

	private void RPC_DoPlayLevelSuccessCinematic(int exitRoomID)
	{
		Room room = UniqueIDManager.Get<Room>(exitRoomID);
		CrystalModuleSlot freeCrystalModuleSlot = room.GetFreeCrystalModuleSlot(isExitSlot: true);
		if (freeCrystalModuleSlot == null)
		{
			Diagnostics.LogError("PlayLevelSuccessCinematic: Unable to find crystal slot in exit room");
			return;
		}
		foreach (Hero localPlayerActiveRecruitedHero in localPlayerActiveRecruitedHeroes)
		{
			localPlayerActiveRecruitedHero.HealthCpnt.IsInvincible = true;
		}
		dungeon.StopExitWaves();
		foreach (Mob activeMob in Mob.ActiveMobs)
		{
			activeMob.HealthCpnt.IsInvincible = true;
		}
		IsUsable = false;
		IGameCameraService service = Services.GetService<IGameCameraService>();
		if (service.IsTacticalMapActive())
		{
			service.SwitchToGameCamera();
		}
		service.Focus(freeCrystalModuleSlot.transform.position, LerpType.Smoothed, -1f);
		service.Lock();
	}

	public bool RespawnStart()
	{
		if (IsRespawning)
		{
			return true;
		}
		float simPropertyValue = GetSimPropertyValue(SimulationProperties.LifeHealPctOnRespawn);
		if (HasCrystal)
		{
			GameOver();
		}
		if ((IsRecruited && simPropertyValue > 0f) || (dungeon.CrystalType == "Respawn" && Player.LocalPlayer.FoodStock > 0f))
		{
			achievementManager.AddToStatistic(StatisticName.HERO_DEATH_NEGATIVE, -1f);
			float simPropertyValue2 = GetSimPropertyValue(SimulationProperties.RespawnRoomCount);
			float simPropertyValue3 = GetSimPropertyValue(SimulationProperties.LifeHealPctOnRespawn);
			audioEventManager.Play2DEvent("Master/Jingles/HeroDeath");
			List<KeyValuePair<EquipmentSlot, InventoryItem>> list = new List<KeyValuePair<EquipmentSlot, InventoryItem>>();
			EquipmentSlot[] equipmentSlots = EquipmentSlots;
			foreach (EquipmentSlot equipmentSlot in equipmentSlots)
			{
				if (equipmentSlot.EquippedItem != null)
				{
					list.Add(new KeyValuePair<EquipmentSlot, InventoryItem>(equipmentSlot, equipmentSlot.EquippedItem));
					equipmentSlot.UnequipItem(netSync: false, checkConfig: false);
				}
			}
			if (simPropertyValue > 0f)
			{
				CurrentRespawnRoomCount = (int)((float)CurrentRespawnRoomCount / simPropertyValue2);
				if (CurrentRespawnRoomCount > 0)
				{
					respawnHealthRatio = (float)CurrentRespawnRoomCount * simPropertyValue3;
					shipRespawn = false;
				}
			}
			else
			{
				float maxHealth = base.HealthCpnt.GetMaxHealth();
				float num = Mathf.Min(Player.LocalPlayer.FoodStock * GameConfig.GetGameConfig().RespawnFoodCoef, maxHealth);
				float num2 = GenericUtilities.RoundHalfAwayFromZero(maxHealth - num);
				respawnHealthRatio = 1f;
				base.HealthCpnt.PermanantHealthMalus += num2;
				float foodCost = num / GameConfig.GetGameConfig().RespawnFoodCoef;
				Player.LocalPlayer.ConsumeFood(foodCost);
			}
			foreach (KeyValuePair<EquipmentSlot, InventoryItem> item in list)
			{
				item.Key.EquipItem(item.Value);
			}
			if (simPropertyValue > 0f && CurrentRespawnRoomCount == 0)
			{
				return false;
			}
			for (int j = 0; j < EquipmentSlots.Length; j++)
			{
				if (EquipmentSlots[j].EquippedItem != null && EquipmentSlots[j].EquippedItem.ItemConfig.DestroyOnDeath)
				{
					InventoryItem equippedItem = EquipmentSlots[j].EquippedItem;
					EquipmentSlots[j].UnequipItem(netSync: true, checkConfig: false);
					dungeon.NetSyncElement.SendRPCToAll(SingletonRPC.Dungeon_DoRemoveItem, equippedItem.UniqueIDCategory, equippedItem.UniqueID);
					break;
				}
			}
			CurrentRespawnRoomCount = 0;
			dungeon.EnqueueAlertNotification(AgeLocalizer.Instance.LocalizeString("%Notification_HeroRespawn").Replace("$HeroName", LocalizedName), "HeroDied");
			if (!dungeon.IsLevelOver)
			{
				audioEventManager.Play2DEvent("Master/Jingles/HeroDeath");
			}
			IsRespawning = true;
			allowAnim = false;
			CancelNPCInteraction();
			LockInteractions();
			MoverCpnt.StopMoving();
			attackerCpnt.StopAttack();
			IsUsable = false;
			destRoomMarkerRnd.enabled = false;
			heroAttackTargetCpnt.SetActive(active: false);
			spriteAnim.ResetAllTriggers();
			spriteAnim.Trigger(SpriteAnimationTrigger.OnDeath);
			spriteAnim.SetBool(SpriteAnimationBool.IsActionning, value: false);
			if (simPropertyValue <= 0f)
			{
				base.HealthCpnt.SetHealthRatio(1f);
			}
			if (doorMoveTarget != null)
			{
				doorMoveTarget.RemoveOpener(this);
			}
			return true;
		}
		return false;
	}

	private void OnDeathEndAnimEvent()
	{
		if (IsRespawning)
		{
			if (shipRespawn)
			{
				RespawnEnd();
				return;
			}
			itemRespawnSprite.MonitorAnimEvent("OnHeroRespawn", RespawnEnd);
			itemRespawnSprite.Play();
			audioEventManager.Play2DEvent("Master/Jingles/HalloweenRevive");
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void RespawnEnd()
	{
		allowAnim = true;
		IsRespawning = false;
		attackerCpnt.enabled = true;
		heroAttackTargetCpnt.SetActive(active: true);
		IsUsable = true;
		UnlockInteractions();
		spriteAnim.Trigger(SpriteAnimationTrigger.OnRespawn);
		base.HealthCpnt.SetHealthRatio(respawnHealthRatio);
		if (shipRespawn)
		{
			MoverCpnt.SetPosition(dungeon.StartRoom.CenterPosition);
			dungeon.StartRoom.OnHeroEnter(this);
			MoveToRoom(base.RoomElement.ParentRoom);
		}
		else
		{
			itemRespawnSprite.UnmonitorAnimEvent("OnHeroRespawn", RespawnEnd);
			itemRespawnSprite.MonitorAnimEvent("AnimEnd", OnRespawnAnimEnd);
			shipRespawn = true;
		}
	}

	private void OnRespawnAnimEnd()
	{
		itemRespawnSprite.Stop();
	}

	public override string GetLevelDescriptorName(int level)
	{
		return (string)GetSimDescriptorByType(SimulationProperties.SimDescTypeHero).Name + "_LVL" + level;
	}

	public HeroLevelConfig GetNextLevelConfig()
	{
		int num = base.Level + 1;
		int num2 = num - 1;
		if (num2 <= levelConfigs.Count - 1)
		{
			return levelConfigs[num2];
		}
		return null;
	}

	public Dictionary<StaticString, float> GetLevelUpModifiers()
	{
		int num = base.Level + 1;
		int num2 = num - 1;
		if (num2 <= levelUpModifiersByLevel.Count - 1)
		{
			return levelUpModifiersByLevel[num2];
		}
		return null;
	}

	public void AddPermanentDescriptor(string descriptorName)
	{
		SimulationDescriptor value = Databases.GetDatabase<SimulationDescriptor>().GetValue(descriptorName);
		if (value == null)
		{
			Diagnostics.LogError("Can't find descriptor: " + descriptorName);
		}
		else if (!PermanentDescriptors.Contains(value))
		{
			PermanentDescriptors.Add(value);
			if (value.Type == "Skill")
			{
				AddSkill(descriptorName);
			}
			else
			{
				AddSimDescriptor(value, refresh: false);
			}
		}
	}

	private bool CanLevelUp(bool displayErrors = false, bool consumeFood = true)
	{
		HeroLevelConfig nextLevelConfig = GetNextLevelConfig();
		if (nextLevelConfig == null)
		{
			if (displayErrors)
			{
				dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroLevelUpFailMaxLevel"));
			}
			return false;
		}
		if (consumeFood && nextLevelConfig.FoodCost > 0f && Player.LocalPlayer.FoodStock < nextLevelConfig.FoodCost)
		{
			if (displayErrors)
			{
				dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_HeroLevelUpFailResources").Replace("$HeroName", LocalizedName));
			}
			return false;
		}
		return true;
	}

	public void LevelUp(bool playFeedback = true, bool consumeFood = true)
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.LevelUp should only be called on owner side!");
		}
		else if (CanLevelUp(displayErrors: true, consumeFood))
		{
			if (consumeFood && gameNetManager.IsMultiplayerSession() && !MultiplayerConfig.SplitFIS)
			{
				base.NetSyncElement.SendRPCToServer(UniqueIDRPC.Hero_RequestLevelUp, playFeedback, consumeFood);
			}
			else if (consumeFood && !Player.LocalPlayer.ConsumeFood(GetNextLevelConfig().FoodCost))
			{
				Diagnostics.LogError(base.name + " > Hero.LevelUp: not enough resources!");
			}
			else
			{
				base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoLevelUp, playFeedback);
			}
		}
	}

	private void RPC_RequestLevelUp(bool playFeedback, bool consumeFood)
	{
		if (!gameNetManager.IsServer())
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_RequestLevelUp should only be called on server side!");
		}
		else if (!CanLevelUp(displayErrors: false, consumeFood))
		{
			Diagnostics.LogWarning(base.name + " > Hero.RPC_RequestLevelUp: NOPE!");
		}
		else if (consumeFood && !Player.LocalPlayer.ConsumeFood(GetNextLevelConfig().FoodCost))
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_RequestLevelUp: not enough resources!");
		}
		else
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoLevelUp, playFeedback);
		}
	}

	private void RPC_DoLevelUp(bool playFeedback)
	{
		if (!CanLevelUp(displayErrors: false, consumeFood: false))
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_DoLevelUp: CANNOT LEVEL UP!");
			return;
		}
		HeroLevelConfig nextLevelConfig = GetNextLevelConfig();
		if (nextLevelConfig.Skills != null && nextLevelConfig.Skills.Length > 0)
		{
			string[] skills = nextLevelConfig.Skills;
			foreach (string skillName in skills)
			{
				AddSkill(skillName);
			}
		}
		ApplyLevelUp();
		if (playFeedback)
		{
			if (allowAnim)
			{
				spriteAnim.Trigger(SpriteAnimationTrigger.OnLevelUp);
				levelUpSpriteAnim.Trigger(SpriteAnimationTrigger.OnLevelUp);
			}
			base.AudioEmitter.PlayEvent("Master/Jingles/LvlUpHero");
			if (levelUpVfxParticles != null)
			{
				levelUpVfxParticles.Play(withChildren: true);
			}
		}
		if (OperatingModule != null)
		{
			OperatingModule.UpdateOperatorWit(this);
		}
		if (RepairingModule != null)
		{
			RepairingModule.UpdateReparatorWit(this);
		}
		gameEventManager.TriggerDungeonFIDSChangedEvent();
		if (GetNextLevelConfig() == null && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			SingletonManager.Get<AchievementManagerDOTE>().IncrementStatistic(StatisticName.MAX_LEVEL_HEROES);
		}
	}

	public void Select(bool recordSelectTime = true, bool unselectOthers = true, bool triggerHeroSelectionEvent = true)
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError("Cannot select a hero owned by another player!");
			return;
		}
		if (!IsRecruited)
		{
			Diagnostics.LogError("Cannot select a free hero!");
			return;
		}
		if (!base.HealthCpnt.IsAlive())
		{
			Diagnostics.LogError("Cannot select a dead hero!");
			return;
		}
		if (selectedHeroes.Count >= 1 && selectedHeroes[0] == this && Time.realtimeSinceStartup <= lastSelectRealTime + doubleSelectMaxTimeDelta)
		{
			Focus();
		}
		if (gameControlManager.MultipleHeroSelectionMode && SelectedHeroes.Contains(this))
		{
			Unselect();
			return;
		}
		unselectOthers &= !gameControlManager.MultipleHeroSelectionMode;
		if (selectedHeroes.Count > 0 && unselectOthers)
		{
			while (selectedHeroes.Count > 0)
			{
				selectedHeroes[0].Unselect(triggerHeroSelectionEvent: false, assertAtLeastOneHeroIsSelected: false);
			}
		}
		selectorRenderer.enabled = true;
		destRoomMarkerRnd.enabled = (MoverCpnt.IsMoving || nextMoveDoorMoveTarget != null || nextMoveRoomMoveTarget != null);
		if (!selectedHeroes.Contains(this))
		{
			selectedHeroes.Add(this);
		}
		if (triggerHeroSelectionEvent)
		{
			gameEventManager.TriggerHeroSelectionEvent();
		}
		base.AudioEmitter.PlayEvent("Master/GUI/SelectHero");
		if (recordSelectTime)
		{
			lastSelectRealTime = Time.realtimeSinceStartup;
		}
		if (TamedMob != null)
		{
			TamedMob.Select(selected: true);
		}
		selectTacticalMapElementAnim.Play();
	}

	public void Focus()
	{
		IGameCameraService service = Services.GetService<IGameCameraService>();
		if (service.IsTacticalMapActive())
		{
			service.SwitchToGameCamera();
		}
		service.Focus(base.transform.position, LerpType.Smoothed, -1f);
		base.RoomElement.ParentRoom.SelectableForMove.Select(silent: true);
	}

	public void LockInteractions()
	{
		attackerCpnt.CanAttack = false;
		MoverCpnt.CanMove = false;
	}

	public void UnlockInteractions()
	{
		if (!IsRespawning)
		{
			attackerCpnt.CanAttack = true;
			MoverCpnt.CanMove = true;
		}
	}

	public void DisplayDialogPanel(string text, float duration = -1f)
	{
		if (situationDialogPanel == null)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(situationDialogPanelPfb, Vector3.zero, Quaternion.identity);
			gameObject.transform.parent = SingletonManager.Get<MainGameScreen>().SituationDialogPanelContainer;
			gameObject.GetComponent<GameToScreenPositionSync>().GameEntityTfm = base.transform;
			situationDialogPanel = gameObject.GetComponent<SituationDialogPanel>();
		}
		situationDialogPanel.Display(text, base.transform, duration);
	}

	protected override void Awake()
	{
		base.Awake();
		MoverCpnt = GetComponent<Mover>();
		AICpnt = GetComponent<HeroAI>();
		heroAttackTargetCpnt = GetComponent<AITarget>();
		achievementManager = SingletonManager.Get<AchievementManagerDOTE>();
		gameEventManager = Services.GetService<IGameEventService>();
		audioEventManager = Services.GetService<IAudioEventService>();
		Mover moverCpnt = MoverCpnt;
		moverCpnt.OnMoveStopped = (Mover.MoveStoppedHandler)Delegate.Combine(moverCpnt.OnMoveStopped, new Mover.MoveStoppedHandler(OnMoveStopped));
		base.HealthCpnt.OnDeath += OnDeath;
		base.HealthCpnt.OnHealStart += OnHealStart;
		base.HealthCpnt.OnDanger += OnDanger;
		base.Level = 0;
		attackerCpnt.OnTargetKilled += OnTargetKilled;
	}

	protected override void Update()
	{
		base.Update();
		if (defenseBonusFromMobKills > 0f)
		{
			defenseBonusFromMobKills = Mathf.Max(defenseBonusFromMobKills - GetSimPropertyValue(SimulationProperties.DefenseBonusPerMobKilled_TimeMalus) * Time.deltaTime, 0f);
			SetSimPropertyBaseValue(SimulationProperties.DefenseBonusFromMobKills, defenseBonusFromMobKills);
		}
		if (attackBonusFromMobKills > 0f)
		{
			attackBonusFromMobKills = Mathf.Max(attackBonusFromMobKills - GetSimPropertyValue(SimulationProperties.AttackBonusPerMobKilled_TimeMalus) * Time.deltaTime, 0f);
			SetSimPropertyBaseValue(SimulationProperties.AttackBonusFromMobKills, attackBonusFromMobKills);
		}
		if (!mobsAggroAttackTargetCpnt.IsActive && GetSimPropertyValue(SimulationProperties.MobsAggro) > 0f)
		{
			mobsAggroAttackTargetCpnt.SetActive(active: true);
		}
		else if (mobsAggroAttackTargetCpnt.IsActive && GetSimPropertyValue(SimulationProperties.MobsAggro) == 0f)
		{
			mobsAggroAttackTargetCpnt.SetActive(active: false);
		}
		bool flag = GetSimPropertyValue(SimulationProperties.Stealth) > 0f;
		if (!HasCrystal && heroAttackTargetCpnt.Type != AITargetType.Stealth && flag)
		{
			heroAttackTargetCpnt.UpdateType(AITargetType.Stealth);
		}
		else if (heroAttackTargetCpnt.Type == AITargetType.Stealth && !flag)
		{
			if (HasCrystal)
			{
				heroAttackTargetCpnt.UpdateType(AITargetType.HeroWithCrystal);
			}
			else if (IsRecruited)
			{
				heroAttackTargetCpnt.UpdateType(Config.AITargetType);
			}
			else
			{
				heroAttackTargetCpnt.UpdateType(AITargetType.FreeHero);
			}
		}
		bool flag2 = GetSimPropertyValue(SimulationProperties.RepairFirst) > 0f;
		if (AICpnt.AIConfigName != "RepairFirstHero" && flag2)
		{
			AICpnt.UpdateAIType("RepairFirstHero");
		}
		else if (AICpnt.AIConfigName == "RepairFirstHero" && !flag2)
		{
			if (IsRecruited)
			{
				AICpnt.UpdateAIType(Config.Name);
			}
			else
			{
				AICpnt.UpdateAIType($"Free{Config.Name}");
			}
		}
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			float simPropertyValue = GetSimPropertyValue(SimulationProperties.MobTaming);
			if (previousMobTaming == 0f && simPropertyValue > 0f)
			{
				FindAndTameAMob();
			}
			previousMobTaming = simPropertyValue;
		}
	}

	private void FixedUpdate()
	{
		float simPropertyValue = GetSimPropertyValue(SimulationProperties.Skill_A0039_Effect);
		if (simPropertyValue > 0f)
		{
			if (previousSkillA0039Value == 0f)
			{
				cachedSkill_A0039_AttackPower = GetSimPropertyValue(SimulationProperties.Skill_A0039_AttackPower);
				cachedSkill_A0039_ZoneAttackRadius = GetSimPropertyValue(SimulationProperties.Skill_A0039_ZoneAttackRadius);
			}
			for (int i = 0; i < base.RoomElement.ParentRoom.Mobs.Count; i++)
			{
				Mob mob = base.RoomElement.ParentRoom.Mobs[i];
				if (!(mob.HealthCpnt != null) || !mob.HealthCpnt.IsAlive() || !(mob.HealthCpnt.GetHealthRatio() <= 0.25f) || !mob.NetSyncElement.IsOwnedByLocalPlayer())
				{
					continue;
				}
				mob.HealthCpnt.Kill();
				mob.AttackerCpnt.ApplyZoneDamagesInRoom(heroAttackTargetCpnt, directHit: true, base.RoomElement.ParentRoom, cachedSkill_A0039_ZoneAttackRadius, cachedSkill_A0039_AttackPower);
				if (SkillA0039VFXPfb == null)
				{
					ActiveSkill activeSkill = activeSkills.First((ActiveSkill skill) => skill.Config.Name.ToString().StartsWith("Skill_A0039"));
					if (activeSkill != null)
					{
						SkillA0039VFXPfb = Resources.Load<GameObject>(activeSkill.Config.TargetVFXPath);
					}
				}
				if (SkillA0039VFXPfb != null)
				{
					UnityEngine.Object.Instantiate(SkillA0039VFXPfb, mob.transform.position, Quaternion.identity);
				}
			}
		}
		previousSkillA0039Value = simPropertyValue;
	}

	private void OnAttackingMobCountChanged()
	{
		if (attackingMobCount < 0)
		{
			Diagnostics.LogError(base.name + " > OnAttackingMobCountChanged: attackingMobCount should be >= 0 (" + attackingMobCount + ")");
		}
		else if (attackingMobCount == 1)
		{
			if (GetSimPropertyValue(SimulationProperties.IsAttackedByOneMob) != 1f)
			{
				SetSimPropertyBaseValue(SimulationProperties.IsAttackedByOneMob, 1f);
			}
		}
		else if (GetSimPropertyValue(SimulationProperties.IsAttackedByOneMob) != 0f)
		{
			SetSimPropertyBaseValue(SimulationProperties.IsAttackedByOneMob, 0f);
		}
	}

	private void SetDestMarkerPosition(Vector3 destPosition)
	{
		if (destRoomMarkerTfm != null && destRoomMarkerRnd != null)
		{
			destRoomMarkerTfm.position = destPosition;
			destRoomMarkerRnd.enabled = selectedHeroes.Contains(this);
		}
	}

	public void DisplayRecruitmentDialog()
	{
		Diagnostics.LogWarning(base.name + " > DisplayRecruitmentDialog");
		if (IsRecruited)
		{
			Diagnostics.LogError("Hero already recruited!");
		}
		IGameCameraService service = Services.GetService<IGameCameraService>();
		if (!service.IsTacticalMapActive() && !service.IsSwitchingCamera)
		{
			if (!IsRecruitable)
			{
				dungeon.EnqueueErrorNotification("%Error_UnrecruitableHero");
				return;
			}
			AgeUtils.ColorToHexaKey(recruitementTextColor, out string hexaKey);
			StaticString x = hexaKey + AgeLocalizer.Instance.LocalizeString("%" + Config.Name + "_FirstName");
			x = x + " (" + AgeLocalizer.Instance.LocalizeString("%LevelStatTitle") + " " + base.Level.ToString() + "): #REVERT#";
			string heroSpeech = x + " " + AgeLocalizer.Instance.LocalizeString(Config.IntroDialogs.GetRandom().Text);
			IGameControlService service2 = Services.GetService<IGameControlService>();
			service2.ClosePanelsAtStartOfInteraction();
			SingletonManager.Get<HeroRecruitmentPanel>().Display(this, heroSpeech);
		}
	}

	private void OnAITargetChanged(AITarget previousTarget, AITarget newTarget)
	{
		Mob previousMobTarget = null;
		if (previousTarget != null && previousTarget.Interaction == AIInteraction.Attack)
		{
			previousMobTarget = previousTarget.GetComponent<Mob>();
		}
		Mob newMobTarget = null;
		if (newTarget != null && newTarget.Interaction == AIInteraction.Attack)
		{
			newMobTarget = newTarget.GetComponent<Mob>();
		}
		foreach (ActiveSkill filteredActiveSkill in FilteredActiveSkills)
		{
			filteredActiveSkill.UpdateTarget(previousMobTarget, newMobTarget);
		}
		foreach (PassiveSkill filteredPassiveSkill in FilteredPassiveSkills)
		{
			filteredPassiveSkill.UpdateTarget(previousMobTarget, newMobTarget);
		}
	}

	private bool CanUnplugCrystal(MajorModule crystal, bool displayErrorNotifs = true)
	{
		if (crystal == null)
		{
			Diagnostics.LogWarning(base.name + " > Hero.UnplugCrystal: crystalMoveTarget is null!");
			return false;
		}
		if (crystal.IsBeingDestroyed)
		{
			Diagnostics.LogWarning(base.name + " > Hero.UnplugCrystal: crystal has already been unplugged!");
			return false;
		}
		if (dungeon.ExitRoom == null)
		{
			if (displayErrorNotifs)
			{
				dungeon.EnqueueErrorNotification(AgeLocalizer.Instance.LocalizeString("%Error_CrystalUnplugFail_ExitNotFound"));
			}
			return false;
		}
		return true;
	}

	private void UnplugCrystal()
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.UnplugCrystal should only be called on owner side!");
		}
		else if (!CanUnplugCrystal(crystalMoveTarget))
		{
			Diagnostics.LogWarning(base.name + " > Hero.UnplugCrystal: cannot unplug crystal!");
		}
		else
		{
			base.NetSyncElement.SendRPCToServer(UniqueIDRPC.Hero_RequestUnplugCrystal, crystalMoveTarget.UniqueID.GetCategory(), crystalMoveTarget.UniqueID.ID);
		}
	}

	private void RPC_RequestUnplugCrystal(StaticString crystalCategory, int crystalID)
	{
		UnityEngine.Component component = UniqueIDManager.Get(crystalCategory, crystalID);
		MajorModule crystal = (!(component != null)) ? null : component.GetComponent<MajorModule>();
		if (!CanUnplugCrystal(crystal, displayErrorNotifs: false))
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_RequestUnplugCrystal: cannot unplug crystal!");
		}
		else
		{
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoUnplugCrystal, crystalCategory, crystalID);
		}
	}

	private void RPC_DoUnplugCrystal(StaticString crystalCategory, int crystalID)
	{
		UnityEngine.Component component = UniqueIDManager.Get(crystalCategory, crystalID);
		MajorModule majorModule = (!(component != null)) ? null : component.GetComponent<MajorModule>();
		if (!CanUnplugCrystal(majorModule))
		{
			Diagnostics.LogError(base.name + " > Hero.RPC_RequestUnplugCrystal: cannot unplug crystal!");
			return;
		}
		majorModule.RemoveCrystal();
		crystalLight.enabled = true;
		crystalContainer.gameObject.SetActive(value: true);
		HasCrystal = true;
		if (this.OnHasCrystalChanged != null)
		{
			this.OnHasCrystalChanged(hasCrystal: true);
		}
		if (GetSimPropertyValue(SimulationProperties.NoCrystalCarryDescriptor) < 1f)
		{
			AddSimDescriptor(SimulationProperties.SimDescHeroHasCrystal);
		}
		heroAttackTargetCpnt.UpdateType(AITargetType.HeroWithCrystal);
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			AICpnt.IsActive = false;
		}
		dungeon.OnCrystalUnplugged(base.NetSyncElement.IsOwnedByLocalPlayer());
		dungeon.UpdateCrystalTfm(base.transform);
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			dungeon.CheckSituationDialog(SituationDialogType.RemoveCrystal, this);
			DeactivateActiveSkills();
		}
	}

	private void PlugCrystal()
	{
		if (!base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Diagnostics.LogError(base.name + " > Hero.PlugCrystal should only be called on owner side!");
			return;
		}
		if (!HasCrystal)
		{
			Diagnostics.LogError("Hero must have the crystal to plug it!");
			return;
		}
		CrystalModuleSlot freeCrystalModuleSlot = base.RoomElement.ParentRoom.GetFreeCrystalModuleSlot();
		if (freeCrystalModuleSlot == null)
		{
			Diagnostics.LogError("No available crystal slot found in " + base.RoomElement.name + "!");
			return;
		}
		base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_DoPlugCrystal, freeCrystalModuleSlot.UniqueID.GetCategory(), freeCrystalModuleSlot.UniqueID.ID);
		MoverCpnt.CanMove = false;
	}

	private void RPC_DoPlugCrystal(StaticString crystalSlotCategory, int crystalSlotID)
	{
		CrystalModuleSlot component = UniqueIDManager.Get(crystalSlotCategory, crystalSlotID).GetComponent<CrystalModuleSlot>();
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			if (!component.IsExitSlot)
			{
				foreach (Room openedRoom in dungeon.OpenedRooms)
				{
					if (!openedRoom.IsStartRoom)
					{
						openedRoom.Unpower();
					}
				}
				if (!dungeon.StartRoom.IsPowered)
				{
					dungeon.StartRoom.PowerByLocalPlayer(displayErrorNotif: false, checkCrystalState: false, consumeDust: false);
				}
			}
			base.RoomElement.ParentRoom.BuildCrystal();
		}
		component.PlayFlashAnimation();
		crystalLight.enabled = false;
		crystalContainer.gameObject.SetActive(value: false);
		HasCrystal = false;
		if (this.OnHasCrystalChanged != null)
		{
			this.OnHasCrystalChanged(hasCrystal: false);
		}
		RemoveSimDescriptor(SimulationProperties.SimDescHeroHasCrystal);
		heroAttackTargetCpnt.UpdateType(Config.AITargetType);
		dungeon.OnCrystalPlugged();
	}

	private void Start()
	{
		dungeon = SingletonManager.Get<Dungeon>();
		dungeon.AddDynamicElementForScaling(destRoomMarkerTfm);
	}

	public void Unselect(bool triggerHeroSelectionEvent = true, bool assertAtLeastOneHeroIsSelected = true)
	{
		if (!selectedHeroes.Contains(this))
		{
			Diagnostics.LogError("Unable to unselect " + base.name + ": this is not the selected hero!");
			return;
		}
		selectorRenderer.enabled = false;
		destRoomMarkerRnd.enabled = false;
		selectedHeroes.Remove(this);
		if (assertAtLeastOneHeroIsSelected && selectedHeroes.Count < 1 && localPlayerActiveRecruitedHeroes.Count > 0)
		{
			localPlayerActiveRecruitedHeroes[0].Select(recordSelectTime: false, unselectOthers: false, triggerHeroSelectionEvent: false);
			if (!base.HealthCpnt.IsAlive())
			{
				HeroListPanel heroListPanel = SingletonManager.Get<HeroListPanel>();
				heroListPanel.RefreshContent();
				heroListPanel.GetHeroItem(SelectedHeroes[0]).SelectHero();
			}
		}
		if (triggerHeroSelectionEvent)
		{
			gameEventManager.TriggerHeroSelectionEvent();
		}
		selectTacticalMapElementAnim.Stop();
		if (TamedMob != null)
		{
			TamedMob.Select(selected: false);
		}
	}

	private void OnDeath(ulong attackerOwnerPlayerID)
	{
		if (IsRespawning)
		{
			return;
		}
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			if (IsRecruited)
			{
				dungeon.EnqueueAlertNotification(AgeLocalizer.Instance.LocalizeString("%Notification_HeroDied").Replace("$HeroName", LocalizedName), "HeroDied");
				audioEventManager.Play2DEvent("Master/Jingles/HeroDeath");
				dungeon.Statistics.IncrementStat(DungeonStatistics.Stat_LostHeroes);
				dungeon.Statistics.IncrementStat(DungeonStatistics.Stat_LostHeroesCurrentFloor);
				achievementManager.AddToStatistic(StatisticName.HERO_DEATH_NEGATIVE, -1f);
				dungeon.RegisterHeroDeath(this);
				AchievementManagerDOTE achievementManagerDOTE = SingletonManager.Get<AchievementManagerDOTE>();
				achievementManagerDOTE.IncrementStatistic(StatisticName.LOST_HEROES);
				if (base.Level <= GameConfig.GetGameConfig().MaxLevelCount)
				{
					GoogleAnalyticsManager googleAnalyticsManager = SingletonManager.Get<GoogleAnalyticsManager>();
					if (googleAnalyticsManager != null)
					{
						googleAnalyticsManager.SendHeroDeathAnalytics(this);
					}
					try
					{
						achievementManagerDOTE.IncrementStatistic(("FLOOR_" + base.Level.ToString() + "_HERO_DEATHS").ToEnum<StatisticName>());
					}
					catch (ArgumentException ex)
					{
						Diagnostics.LogError("Missing hero death stat: " + ex.Message);
					}
				}
				if (IsInteracting && npcMoveTarget != null && Services.GetService<IInputService>().CurrentControlScheme == ControlScheme.XBoxOneController)
				{
					gameControlManager.SetHeroStatsPanelDisplay(on: false);
					SingletonManager.Get<SelectableManager>().SetCategory(SelectionCategory.RoomForMove);
				}
				if (TamedMob != null && TamedMob.HealthCpnt != null)
				{
					TamedMob.HealthCpnt.Kill();
				}
			}
			SpawnDeadHeroLoot();
		}
		if (IsRecruited)
		{
			RecentlyDeceasedHeroes.Add(this);
			if (!DeadHeroes.Contains(this))
			{
				DeadHeroes.Add(this);
			}
		}
		GameOver();
	}

	private void GameOver()
	{
		if (HasCrystal)
		{
			IGameCameraService service = Services.GetService<IGameCameraService>();
			if (service.IsTacticalMapActive())
			{
				service.SwitchToGameCamera();
			}
			else if (!service.IsSwitchingCamera)
			{
				service.Focus(base.transform.position, LerpType.Smoothed, -1f);
				service.ZoomIn();
			}
			Time.timeScale = 0.5f;
			if (base.NetSyncElement.IsOwnedByLocalPlayer())
			{
				dungeon.LevelOver();
			}
		}
		spriteAnim.ResetAllTriggers();
		spriteAnim.Trigger(SpriteAnimationTrigger.OnDeath);
		RemoveFromDungeon();
	}

	private void OnDanger()
	{
		if (!IsRecruited)
		{
			base.RoomElement.ParentRoom.DisplayOffscreenMarker(heroDangerIconData);
		}
	}

	public void SpawnDeadHeroLoot()
	{
		Room parentRoom = base.RoomElement.ParentRoom;
		if (parentRoom == null)
		{
			return;
		}
		List<InventoryItemData> list = new List<InventoryItemData>();
		for (int i = 0; i < EquipmentSlots.Length; i++)
		{
			InventoryItem equippedItem = EquipmentSlots[i].EquippedItem;
			if (equippedItem != null)
			{
				list.Add(new InventoryItemData
				{
					ItemDescName = equippedItem.ItemConfig.Name,
					RarityDescName = ((equippedItem.RarityCfg == null) ? null : equippedItem.RarityCfg.Name)
				});
			}
		}
		if (list.Count != 0)
		{
			Vector3 vector = base.transform.position;
			if (!AICpnt.CanAttack())
			{
				vector = parentRoom.CenterPosition;
			}
			base.NetSyncElement.SendRPCToAll(UniqueIDRPC.Hero_InstantiateDeadHeroItem, base.NetSyncElement.OwnerPlayerID, list.ToArray(), parentRoom.UniqueID.ID, vector);
		}
	}

	private void RPC_InstantiateDeadHeroItem(ulong ownerPlayerID, InventoryItemData[] inventoryItemsData, int spawnRoomID, Vector3 spawnPosition)
	{
		Room spawnRoom = UniqueIDManager.Get<Room>(spawnRoomID);
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(dungeon.GetItemPrefab(ItemType.ItemHero), spawnPosition, Quaternion.identity);
		dungeon.AddDynamicElementForScaling(gameObject.transform);
		ItemHero component = gameObject.GetComponent<ItemHero>();
		component.InitForDeadHeroLoot(ownerPlayerID, inventoryItemsData, spawnRoom);
	}

	private void RemoveFromDungeon()
	{
		base.RoomElement.ParentRoom.OnHeroExit(this);
		dungeon.RefreshSim();
		if (base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			CancelItemGathering();
		}
		CancelNPCInteraction();
		Unbind();
		if (IsRecruited)
		{
			IsRecruited = false;
			if (base.NetSyncElement.IsOwnedByLocalPlayer())
			{
				ModifyLocalActiveHeroes(add: false, this);
			}
			else
			{
				remotePlayersActiveRecruitedHeroes.Remove(this);
			}
			if (!DeadHeroes.Contains(this))
			{
				DeadHeroes.Add(this);
			}
			bool flag = false;
			if ((!gameNetManager.IsMultiplayerSession()) ? (LocalPlayerActiveRecruitedHeroes.Count < 1) : ((!MultiplayerConfig.AllPlayersMustSurvive) ? (LocalPlayerActiveRecruitedHeroes.Count < 1 && RemotePlayersActiveRecruitedHeroes.Count < 1) : (LocalPlayerActiveRecruitedHeroes.Count < 1)))
			{
				IGameCameraService service = Services.GetService<IGameCameraService>();
				if (service.IsTacticalMapActive())
				{
					service.SwitchToGameCamera();
				}
				service.Focus(base.transform.position, LerpType.Smoothed, -1f);
				service.ZoomIn();
				Time.timeScale = 0.5f;
				if (base.NetSyncElement.IsOwnedByLocalPlayer())
				{
					dungeon.LevelOver();
				}
			}
			if (selectedHeroes.Contains(this))
			{
				Unselect();
			}
			gameEventManager.TriggerHeroListChangedEvent();
			if (Hero.OnActiveHeroDeath != null)
			{
				Hero.OnActiveHeroDeath();
			}
		}
		if (destRoomMarkerTfm != null)
		{
			UnityEngine.Object.Destroy(destRoomMarkerTfm.gameObject);
		}
	}

	private void OnHealStart()
	{
		base.AudioEmitter.PlayEvent("Master/Environment/HealState");
	}

	private void OnHealthChanged()
	{
		float num = initalHealthBarScaleY;
		float num2 = initalHealthBarScaleX;
		if (base.HealthCpnt.GetHealthRatio() < lowHealthValue)
		{
			num *= lowHealthBarScaleY;
			num2 *= lowHealthBarScaleX;
			Vector3 localScale = base.HealthCpnt.HealthBar.BarContainer.localScale;
			if (localScale.y == initalHealthBarScaleY)
			{
				audioEventManager.Play2DEvent("Master/Jingles/LowHealth");
			}
		}
		Transform barContainer = base.HealthCpnt.HealthBar.BarContainer;
		float x = num2;
		float y = num;
		Vector3 localScale2 = base.HealthCpnt.HealthBar.BarContainer.localScale;
		barContainer.localScale = new Vector3(x, y, localScale2.z);
	}

	private void OnTargetKilled(Health targetHealth)
	{
		Mob component = targetHealth.GetComponent<Mob>();
		if (!(component != null))
		{
			return;
		}
		if (dungeon.CurrentCrystalState != CrystalState.Unplugged)
		{
			foodBonusFromMobKills += GetSimPropertyValue(SimulationProperties.FoodBonusPerMobKilled);
			if (foodBonusFromMobKills >= 1f)
			{
				float num = Mathf.Floor(foodBonusFromMobKills);
				Player.LocalPlayer.AddFood(num);
				foodBonusFromMobKills -= num;
			}
			industryBonusFromMobKills += GetSimPropertyValue(SimulationProperties.IndustryBonusPerMobKilled);
			if (industryBonusFromMobKills >= 1f)
			{
				float num2 = Mathf.Floor(industryBonusFromMobKills);
				Player.LocalPlayer.AddIndustry(num2);
				industryBonusFromMobKills -= num2;
			}
		}
		defenseBonusFromMobKills = Mathf.Min(defenseBonusFromMobKills + GetSimPropertyValue(SimulationProperties.DefenseBonusPerMobKilled), GetSimPropertyValue(SimulationProperties.DefenseBonusPerMobKilled_Max));
		SetSimPropertyBaseValue(SimulationProperties.DefenseBonusFromMobKills, defenseBonusFromMobKills);
		attackBonusFromMobKills = Mathf.Min(attackBonusFromMobKills + GetSimPropertyValue(SimulationProperties.AttackBonusPerMobKilled), GetSimPropertyValue(SimulationProperties.AttackBonusPerMobKilled_Max));
		SetSimPropertyBaseValue(SimulationProperties.AttackBonusFromMobKills, attackBonusFromMobKills);
	}

	private void OnInteractionTargetDeath(ulong attackerOwnerPlayerID)
	{
		CancelNPCInteraction();
	}

	private void Dungeon_OnRoomPowered(Room room, bool powered)
	{
		RefreshSim(refreshParent: false);
	}

	private void OnGameVictory()
	{
		if (IsRecruited && base.HealthCpnt.IsAlive() && base.NetSyncElement.IsOwnedByLocalPlayer() && base.WasInExitRoomAtExitTime)
		{
			if (UnlockLevel > 0)
			{
				UnlockHero(Config);
			}
			dungeon.RegisterHeroVictory(this);
		}
	}

	private void OnLeftClickDown(ClickDownInfo clickInfo)
	{
		if (IsRecruited && base.NetSyncElement.IsOwnedByLocalPlayer())
		{
			Select();
			Services.GetService<IInputService>().StopClickEventPropagation();
		}
	}

	private void OnRightClickDown(ClickDownInfo clickInfo)
	{
		if (selectedHeroes.Count > 0)
		{
			if (!IsRecruited && !IsDismissing)
			{
				foreach (Hero selectedHero in selectedHeroes)
				{
					selectedHero.MoveToHero(this);
				}
			}
			else
			{
				foreach (Hero selectedHero2 in selectedHeroes)
				{
					selectedHero2.MoveToRoom(base.RoomElement.ParentRoom);
				}
			}
			Services.GetService<IInputService>().StopClickEventPropagation();
		}
		else if (!IsRecruited && localPlayerActiveRecruitedHeroes.Count < 1 && gameNetManager.IsMultiplayerSession())
		{
			DisplayRecruitmentDialog();
		}
	}

	private void OnDisable()
	{
		gameEventManager.OnGameVictory -= OnGameVictory;
		if (!IsRecruited && base.HealthCpnt.IsAlive())
		{
			dungeon.AddToDiscoverableHeroPool(Config);
		}
	}

	private void OnDestroy()
	{
		if (gameEventManager != null)
		{
			gameEventManager.OnDungeonTurnChanged -= OnDungeonTurnChangedDuringAction;
			gameEventManager.OnDungeonTurnChanged -= GameEventManager_OnDungeonTurnChanged;
		}
		if (playerNamePanel != null)
		{
			playerNamePanel.Unload();
			UnityEngine.Object.Destroy(playerNamePanel.gameObject);
			playerNamePanel = null;
		}
		if (gameNetManager == null)
		{
			gameNetManager = SingletonManager.Get<GameNetworkManager>();
		}
		if (gameNetManager != null)
		{
			gameNetManager.ClearCachedAITargets(this);
		}
	}

	public static void ModifyLocalActiveHeroes(bool add, Hero hero)
	{
		if (add)
		{
			LocalPlayerActiveRecruitedHeroes.Add(hero);
		}
		else
		{
			LocalPlayerActiveRecruitedHeroes.Remove(hero);
		}
		if (Hero.OnLocalActiveHeroesChange != null)
		{
			Hero.OnLocalActiveHeroesChange();
		}
	}
}
