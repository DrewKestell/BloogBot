namespace WoWSharpClient.Models
{
    public class Realm
    {
        public uint RealmType;
        public byte Flags;
        public string RealmName = string.Empty;
        public int AddressPort;
        public float Population;
        public float NumChars;
        public byte RealmCategory;
        public byte RealmId;
    }
}
