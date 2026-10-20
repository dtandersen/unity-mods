// Minimal Valheim fakes: only what OdinsAi.Mod compiles against.
// Signatures mirror the real game API; bodies are never run (CI compile only).
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class StringExtensions
{
    public static int GetStableHashCode(this string s) => 0;
}

public class Player : MonoBehaviour
{
    public static Player m_localPlayer;
    public Inventory GetInventory() => null;
    public Dictionary<string, string> GetKnownTexts() => new Dictionary<string, string>();
    public bool HaveSeenTutorial(string name) => false;
    public string GetPlayerName() => string.Empty;
    public Vector3 GetHeadPoint() => Vector3.zero;
    public Heightmap.Biome GetCurrentBiome() => Heightmap.Biome.Meadows;
    public Heightmap.BiomeData GetCurrentBiomeData() => null;
    public bool IsRecipeKnown(string name) => false;
}

public class ObjectDB : MonoBehaviour
{
    public static ObjectDB instance;
    public List<GameObject> m_items = new List<GameObject>();
    public List<Recipe> m_recipes = new List<Recipe>();
}

public class ZNet : MonoBehaviour
{
    public static ZNet instance;
    public bool IsServer() => false;
    public ZNetPeer GetPeer(long peerId) => null;
}

public class ZNetPeer
{
    public ZDOID m_characterID;
}

public class ZDOMan : MonoBehaviour
{
    public static ZDOMan instance;
    public ZDO GetZDO(ZDOID id) => null;
}

public class ZDO { }

public struct ZDOID
{
    public bool IsNone() => true;
}

public class ZNetScene : MonoBehaviour
{
    public static ZNetScene instance;
    public List<GameObject> m_prefabs = new List<GameObject>();
    public ZNetView FindInstance(ZDO zdo) => null;
}

public class ZNetView : MonoBehaviour
{
    public static object[] Deserialize(long senderPeerID, ParameterInfo[] parameters, ZPackage package) => new object[0];
}

public class ZPackage { }

public class ZRoutedRpc : MonoBehaviour
{
    public static ZRoutedRpc instance;
    public static readonly long Everybody = 0;

    public class RoutedRPCData
    {
        public int m_methodHash;
        public long m_senderPeerID;
        public ZDOID m_targetZDO;
        public ZPackage m_parameters;
    }

    public void RouteRPC(RoutedRPCData rpcData) { }
    public void InvokeRoutedRPC(long targetPeerID, string methodName, params object[] parameters) { }
}

public class Talker : MonoBehaviour
{
    public enum Type
    {
        Normal,
        Shout,
        Whisper,
    }

    private void RPC_Say(long sender, Type type, UserInfo user, string text) { }
}

public class Chat : MonoBehaviour
{
    public static Chat instance;
    private float m_hideTimer;

    private void RPC_ChatMessage(long sender, Vector3 position, int type, UserInfo user, string text) { }
    public void SendText(Talker.Type type, string text) { }
    public void OnNewChatMessage(GameObject sender, string text) { }
}

public class Terminal : MonoBehaviour
{
    public void AddString(string text) { }
}

public class UserInfo { }

public class Character : MonoBehaviour
{
    public string m_defeatSetGlobalKey;
    public string m_name;
    public string GetHoverName() => string.Empty;
    public bool IsPlayer() => false;
    public bool IsTamed() => false;
}

public class ZoneSystem : MonoBehaviour
{
    public static ZoneSystem instance;
    public bool GetGlobalKey(string key) => false;
}

public class ItemDrop : MonoBehaviour
{
    public ItemData m_itemData;

    public class ItemData
    {
        public SharedData m_shared;
        public int m_stack;
        public int m_quality;
        public GameObject m_dropPrefab;

        public enum ItemType
        {
            Misc,
            Consumable,
            OneHandedWeapon,
            Bow,
            Shield,
            Helmet,
            Chest,
            Ammo,
            Customization,
            Legs,
            Hands,
            Trophie,
            Shoulder,
            Utility,
            Tool,
            Placeable,
        }

        public class SharedData
        {
            public string m_name;
            public float m_food;
            public float m_foodStamina;
            public float m_foodEitr;
            public float m_foodBurnTime;
            public float m_foodRegen;
            public ItemType m_itemType;
            public int m_maxQuality;
        }
    }
}

public class Recipe : MonoBehaviour
{
    public ItemDrop m_item;
    public int m_amount;
    public CraftingStation m_craftingStation;
    public List<Piece.Requirement> m_resources = new List<Piece.Requirement>();
    public CraftingStation GetRequiredStation(int quality) => null;
    public int GetRequiredStationLevel(int quality) => 0;
}

public class Piece : MonoBehaviour
{
    public class Requirement
    {
        public ItemDrop m_resItem;
        public int m_amount;
        public bool m_upgraderResource;
        public int GetAmount(int quality) => 0;
    }
}

public class CraftingStation : MonoBehaviour
{
    public static List<CraftingStation> Instances = new List<CraftingStation>();
    public string m_name;
    public bool m_upgrader;
    public float GetStationBuildRange() => 0f;
    public int GetLevel() => 0;
}

public class Container : MonoBehaviour
{
    public string m_name;
    public Inventory GetInventory() => null;
    public bool IsOwner() => false;
}

public class Inventory
{
    public List<ItemDrop.ItemData> GetAllItems() => new List<ItemDrop.ItemData>();
}

public class Localization : MonoBehaviour
{
    public static Localization instance;
    public string Localize(string token) => token;
}

public class Tutorial : MonoBehaviour
{
    public static Tutorial instance;
    public List<TutorialText> m_texts = new List<TutorialText>();

    public class TutorialText
    {
        public string m_topic;
        public string m_label;
        public string m_name;
        public string m_text;
    }
}

public class DungeonGenerator : MonoBehaviour { }

public class MineRock5 : MonoBehaviour
{
    public DropTable m_dropItems;
    public string GetHoverName() => string.Empty;
}

public class MineRock : MonoBehaviour
{
    public DropTable m_dropItems;
    public string GetHoverName() => string.Empty;
}

public class Pickable : MonoBehaviour
{
    public string GetHoverName() => string.Empty;
}

public class TreeBase : MonoBehaviour { }

public class DropTable : MonoBehaviour
{
    public List<ItemDrop.ItemData> GetDropListItems() => new List<ItemDrop.ItemData>();
}

public static class Utils
{
    public static float DistanceXZ(Vector3 a, Vector3 b) => 0f;
}

public static class BiomeSector
{
    public static string GetBiomeName(Heightmap.Biome biome) => string.Empty;
}

public class Heightmap : MonoBehaviour
{
    public enum Biome
    {
        None,
        Meadows,
        Swamp,
        Mountain,
        BlackForest,
        Plains,
        AshLands,
        DeepNorth,
        Ocean,
        Mistlands,
    }

    public class BiomeData
    {
        public string GetName() => string.Empty;
    }
}
