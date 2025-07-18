using GameData.Core.Enums;

namespace WoWSharpClient.Handlers;

public static class GuidFieldHandler
{
    public static bool IsGuidField(UpdateFields.EItemFields field)
    {
        return field == UpdateFields.EItemFields.ITEM_FIELD_OWNER
            || field == UpdateFields.EItemFields.ITEM_FIELD_CONTAINED
            || field == UpdateFields.EItemFields.ITEM_FIELD_CREATOR
            || field == UpdateFields.EItemFields.ITEM_FIELD_GIFTCREATOR;
    }

    public static byte[] ReadGuidField(BinaryReader reader)
    {
        byte[] low = reader.ReadBytes(4);
        byte[] high = reader.ReadBytes(4);
        byte[] full = new byte[8];
        Array.Copy(low, 0, full, 0, 4);
        Array.Copy(high, 0, full, 4, 4);
        return full;
    }
}
