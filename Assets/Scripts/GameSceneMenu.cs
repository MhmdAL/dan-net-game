using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private TextMeshProUGUI heroTitleText;
    [SerializeField] private TextMeshProUGUI zombieTitleText;
    [SerializeField] private TextMeshProUGUI towerTitleText;
    
    [SerializeField] private TextMeshProUGUI connectionsText;

    [SerializeField] private TMP_InputField heroSpeedInput;
    [SerializeField] private TMP_InputField heroHealthInput;
    [SerializeField] private TMP_InputField heroDmgInput;
    [SerializeField] private TMP_InputField heroRateInput;

    [SerializeField] private TMP_InputField zombieSpeedInput;
    [SerializeField] private TMP_InputField zombieHealthInput;
    [SerializeField] private TMP_InputField zombieDmgInput;
    [SerializeField] private TMP_InputField zombieRateInput;
    [SerializeField] private TMP_InputField zombieSpawnRateInput;

    [SerializeField] private TMP_InputField towerRangeInput;
    [SerializeField] private TMP_InputField towerDmgInput;
    [SerializeField] private TMP_InputField towerRateInput;

    [SerializeField] private TMP_InputField fireStacksInput;
    [SerializeField] private TMP_InputField slowStacksInput;

    private EntityQuery _allHeroQuery;
    private EntityQuery _heroQuery;
    private EntityQuery _allZombieQuery;
    private EntityQuery _zombieQuery;
    private EntityQuery _allTowerQuery;
    private EntityQuery _towerQuery;

    private EntityQuery _zombieSpawnerQuery;

    private EntityQuery _effectsDataQuery;
    private EntityQuery _modifiersDataQuery;

    private EntityQuery _connectionsQuery;

    private World _serverWorld;

    private void Start()
    {
        Invoke("Init", .5f);

        heroSpeedInput.onEndEdit.AddListener(x => UpdateStats());
        heroHealthInput.onEndEdit.AddListener(x => UpdateStats());
        heroDmgInput.onEndEdit.AddListener(x => UpdateStats());
        heroRateInput.onEndEdit.AddListener(x => UpdateStats());

        zombieSpeedInput.onEndEdit.AddListener(x => UpdateStats());
        zombieHealthInput.onEndEdit.AddListener(x => UpdateStats());
        zombieDmgInput.onEndEdit.AddListener(x => UpdateStats());
        zombieRateInput.onEndEdit.AddListener(x => UpdateStats());

        zombieSpawnRateInput.onEndEdit.AddListener(x => UpdateStats());

        towerRangeInput.onEndEdit.AddListener(x => UpdateStats());
        towerDmgInput.onEndEdit.AddListener(x => UpdateStats());
        towerRateInput.onEndEdit.AddListener(x => UpdateStats());

        fireStacksInput.onEndEdit.AddListener(x => UpdateStats());
        slowStacksInput.onEndEdit.AddListener(x => UpdateStats());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }

        if (pauseMenu.activeSelf)
        {
            var heroCount = _heroQuery.CalculateEntityCount();
            var zombieCount = _zombieQuery.CalculateEntityCount();
            var towerCount = _towerQuery.CalculateEntityCount();

            var connectionCount = _connectionsQuery.CalculateEntityCount();

            heroTitleText.text = $"Hero ({heroCount})";
            zombieTitleText.text = $"Zombie ({zombieCount})";
            towerTitleText.text = $"Tower ({towerCount})";

            connectionsText.text = $"Connections ({connectionCount})";
        }
    }

    private void Init()
    {
        World serverWorld = null;
        for (int i = 0; i < World.All.Count; i++)
        {
            if (World.All[i].IsServer())
            {
                serverWorld = World.All[i];
                break;
            }
        }

        _serverWorld = serverWorld;

        _heroQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<HeroTag, SpeedData, HealthComponent, AttackData>()
            .Build(_serverWorld.EntityManager);

        _allHeroQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<HeroTag, SpeedData, HealthComponent, AttackData>()
            .WithOptions(EntityQueryOptions.IncludePrefab)
            .Build(_serverWorld.EntityManager);

        _zombieQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<ZombieTag, SpeedData, HealthComponent, AttackData>()
            .Build(_serverWorld.EntityManager);

        _allZombieQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<ZombieTag, SpeedData, HealthComponent, AttackData>()
            .WithOptions(EntityQueryOptions.IncludePrefab)
            .Build(_serverWorld.EntityManager);

        _towerQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<TowerTag, AttackData, AttackRangeData>()
            .Build(_serverWorld.EntityManager);

        _allTowerQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<TowerTag, AttackData, AttackRangeData>()
            .WithOptions(EntityQueryOptions.IncludePrefab)
            .Build(_serverWorld.EntityManager);

        _zombieSpawnerQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<ZombieSpawnerComponent>()
            .Build(_serverWorld.EntityManager);

        _effectsDataQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<EffectsData>()
            .WithOptions(EntityQueryOptions.IncludeSystems)
            .Build(_serverWorld.EntityManager);
        
        _modifiersDataQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<ModifiersData>()
            .WithOptions(EntityQueryOptions.IncludeSystems)
            .Build(_serverWorld.EntityManager);

        _connectionsQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<NetworkStreamInGame>()
            .Build(_serverWorld.EntityManager);

        InitFields();
    }

    private void InitFields()
    {
        // Hero

        var heroPrefabQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Prefab, HeroTag, SpeedData, AttackData, HealthComponent>()
            .WithOptions(EntityQueryOptions.IncludePrefab)
            .Build(_serverWorld.EntityManager);
        
        var heroPrefab = heroPrefabQuery.ToEntityArray(Allocator.Temp)[0];

        var heroSpeed = _serverWorld.EntityManager.GetComponentData<SpeedData>(heroPrefab).OriginalValue;
        var heroHealth = _serverWorld.EntityManager.GetComponentData<HealthComponent>(heroPrefab).MaxHealth;
        var heroAtkData = _serverWorld.EntityManager.GetComponentData<AttackData>(heroPrefab);

        heroSpeedInput.text = heroSpeed.ToString();
        heroHealthInput.text = heroHealth.ToString();
        heroDmgInput.text = heroAtkData.Damage.ToString();
        heroRateInput.text = heroAtkData.MaxCooldown.ToString();

        // Zombies

        var zombiePrefabQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Prefab, ZombieTag, SpeedData, AttackData, HealthComponent>()
            .WithOptions(EntityQueryOptions.IncludePrefab)
            .Build(_serverWorld.EntityManager);

        var zombiePrefab = zombiePrefabQuery.ToEntityArray(Allocator.Temp)[0];

        var zombieSpeed = _serverWorld.EntityManager.GetComponentData<SpeedData>(zombiePrefab).OriginalValue;
        var zombieHealth = _serverWorld.EntityManager.GetComponentData<HealthComponent>(zombiePrefab).MaxHealth;
        var zombieAtkData = _serverWorld.EntityManager.GetComponentData<AttackData>(zombiePrefab);

        zombieSpeedInput.text = zombieSpeed.ToString();
        zombieHealthInput.text = zombieHealth.ToString();
        zombieDmgInput.text = zombieAtkData.Damage.ToString();
        zombieRateInput.text = zombieAtkData.MaxCooldown.ToString();

        // Zombie Spawner

        var zombieSpawner = _zombieSpawnerQuery.ToEntityArray(Allocator.Temp)[0];

        var zombieSpawnRate = _serverWorld.EntityManager.GetComponentData<ZombieSpawnerComponent>(zombieSpawner)
            .SpawnCooldown;

        zombieSpawnRateInput.text = zombieSpawnRate.ToString();

        // Towers

        var towerPrefabQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Prefab, TowerTag, AttackData, AttackRangeData>()
            .WithOptions(EntityQueryOptions.IncludePrefab)
            .Build(_serverWorld.EntityManager);

        var towerPrefab = towerPrefabQuery.ToEntityArray(Allocator.Temp)[0];

        var towerRange = _serverWorld.EntityManager.GetComponentData<AttackRangeData>(towerPrefab).Value;
        var towerAtkData = _serverWorld.EntityManager.GetComponentData<AttackData>(towerPrefab);

        towerRangeInput.text = towerRange.ToString();
        towerDmgInput.text = towerAtkData.Damage.ToString();
        towerRateInput.text = towerAtkData.MaxCooldown.ToString();

        // Stacks

        var effectsData = _effectsDataQuery.ToComponentDataArray<EffectsData>(Allocator.Temp)[0];
        
        fireStacksInput.text = effectsData.MaxStacksMap[(int)EffectType.FireDoT].ToString();
        
        var modifiersData = _modifiersDataQuery.ToComponentDataArray<ModifiersData>(Allocator.Temp)[0];
        
        slowStacksInput.text = modifiersData.MaxStacksMap[(int)ModifierType.Slow].ToString();
    }

    private void UpdateStats()
    {
        // Hero

        var heros = _allHeroQuery.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < heros.Length; i++)
        {
            if (float.TryParse(heroSpeedInput.text, out var heroSpeed))
                _serverWorld.EntityManager.SetComponentData(heros[i],
                    new SpeedData { OriginalValue = heroSpeed, CurrentValue = heroSpeed });

            if (int.TryParse(heroHealthInput.text, out var heroHealth))
                _serverWorld.EntityManager.SetComponentData(heros[i],
                    new HealthComponent { MaxHealth = heroHealth, CurrentHealth = heroHealth });

            if (int.TryParse(heroDmgInput.text, out var dmg) && float.TryParse(heroRateInput.text, out var rate))
                _serverWorld.EntityManager.SetComponentData(heros[i],
                    new AttackData { Damage = dmg, MaxCooldown = rate, CurrentCooldown = rate, HitChance = 1 });
        }

        // Zombies

        var zombies = _allZombieQuery.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < zombies.Length; i++)
        {
            if (float.TryParse(zombieSpeedInput.text, out var zombieSpeed))
                _serverWorld.EntityManager.SetComponentData(zombies[i],
                    new SpeedData { OriginalValue = zombieSpeed, CurrentValue = zombieSpeed });

            if (int.TryParse(zombieHealthInput.text, out var zombieHealth))
                _serverWorld.EntityManager.SetComponentData(zombies[i],
                    new HealthComponent { MaxHealth = zombieHealth, CurrentHealth = zombieHealth });

            if (int.TryParse(zombieDmgInput.text, out var dmg) && float.TryParse(zombieRateInput.text, out var rate))
                _serverWorld.EntityManager.SetComponentData(zombies[i],
                    new AttackData { Damage = dmg, MaxCooldown = rate, CurrentCooldown = rate, HitChance = 1f });
        }
        
        // Zombie Spawner

        var zombieSpawner = _zombieSpawnerQuery.ToEntityArray(Allocator.Temp)[0];

        var zombieSpawnerComponent = _serverWorld.EntityManager.GetComponentData<ZombieSpawnerComponent>(zombieSpawner);

        if (float.TryParse(zombieSpawnRateInput.text, out float spawnRate))
        {
            _serverWorld.EntityManager.SetComponentData(zombieSpawner,
                new ZombieSpawnerComponent()
                {
                    Prefab = zombieSpawnerComponent.Prefab, SpawnCooldown = spawnRate, TimeTillNextSpawn = spawnRate
                });
        }

        // Towers

        var towers = _allTowerQuery.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < towers.Length; i++)
        {
            if (float.TryParse(towerRangeInput.text, out var range))
                _serverWorld.EntityManager.SetComponentData(towers[i],
                    new AttackRangeData { Value = range });

            if (int.TryParse(towerDmgInput.text, out var dmg) && float.TryParse(towerRateInput.text, out var rate))
                _serverWorld.EntityManager.SetComponentData(towers[i],
                    new AttackData { Damage = dmg, MaxCooldown = rate, CurrentCooldown = rate, HitChance = .5f });
        }

        // Stacks

        var effectsData = _effectsDataQuery.ToComponentDataArray<EffectsData>(Allocator.Temp)[0];
        
        var effectsStacksMap = effectsData.MaxStacksMap;
        
        if (int.TryParse(fireStacksInput.text, out var fireStacks))
        {
            effectsStacksMap[(int)EffectType.FireDoT] = fireStacks;
        }
        
        _serverWorld.EntityManager.SetComponentData(_serverWorld.GetExistingSystem(typeof(EffectSystem)),
            new EffectsData() { MaxStacksMap = effectsStacksMap });
        
        var modifiersData = _modifiersDataQuery.ToComponentDataArray<ModifiersData>(Allocator.Temp)[0];
        
        var modifierStacksMap = modifiersData.MaxStacksMap;
        
        if (int.TryParse(slowStacksInput.text, out var slowStacks))
        {
            modifierStacksMap[(int)ModifierType.Slow] = slowStacks;
        }
        
        _serverWorld.EntityManager.SetComponentData(_serverWorld.GetExistingSystem(typeof(ModifierSystem)),
            new ModifiersData() { MaxStacksMap = modifierStacksMap });

        zombies.Dispose();
        heros.Dispose();
        towers.Dispose();
    }

    public void RestartSimulation()
    {
        _serverWorld.EntityManager.DestroyEntity(_zombieQuery);
        _serverWorld.EntityManager.DestroyEntity(_towerQuery);
    }

    public void LoadMainMenu()
    {
        var clientServerWorlds = new List<World>();
        foreach (var world in World.All)
        {
            if (world.IsClient() || world.IsServer())
                clientServerWorlds.Add(world);
        }

        foreach (var world in clientServerWorlds)
            world.Dispose();

        SceneManager.LoadScene("Menu");
    }
}