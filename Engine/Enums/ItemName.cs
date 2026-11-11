namespace Engine.Enums
{
    /// <summary>
    /// Unique identifier for every item the bot knows about.
    /// Add a new entry here whenever you register a new item in ItemRegistry.
    /// </summary>
    public enum ItemName
    {
        // ── Resources ────────────────────────────────────────────────────────
        Tree,
        PineLog,
        Limestone,
        Stone,
        CopperOre,
        Berry,
        FlaxFibber,
        Leather,
        RawLeather,
        Rope,
        Cloth,
        ThickCloth,
        LeekSeed,
        WheatSeed,
        OakSeed,
        EmptyFlask,
        CopperShard,
        Chain,
        Candle,
        Wire,
        Cords,
        Lock,
        Tin,

        // ── Food / Consumables ───────────────────────────────────────────────
        RawMeat,
        Hollyberry,
        Hawberry,
        Jelly,
        HoneyBarrel,
        Ale,
        Bandage,
        // ── Weapons / Equipment ──────────────────────────────────────────────
        Dagger,
        WeightedMace,
        SpikeMace,
        FlangedMace,
        Sickle,
        MorningStar,
        WoodenSword,
        WoodenWeapon1,
        WoodenWeapon2,
        StoneWeapon1,
        StoneWeapon2,
        CopperWeapon1,
        CopperWeapon2,
        CopperHat,
        CopperHauberk,
        CopperGlove,
        CopperTrousers,
        CopperBoots,
        LeatherHat,
        LeatherHauberk,
        LeatherGlove,
        LeatherTrousers,
        LeatherBoots,
        FibberHat,
        FibberHauberk,
        FibberGlove,
        FibberTrousers,
        FibberBoots,

        // ── Tools ──────────────────────────────────────────────
        FarmingTools,


        // ── Special / Quest ──────────────────────────────────────────────────
        SoulShard,
        BonePowder,
        Fibula,
        Comb,
        Hourglass,
        Mirror,
        Silverware,
        Skull,
        Bowstring,
        GoldRing,
        IronGrate,
        TerrorScroll,
        MapFragment,
        Wheel,
        MandrakeTincture,
        PumpkinSeeds,
        EmptyVial,
        Unknown, // Fallback for unrecognised items
    }
}
