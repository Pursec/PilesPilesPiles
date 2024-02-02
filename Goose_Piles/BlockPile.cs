using System;

namespace Goose_Vintage_BlockPiles
{
    public class BlockPile : BlockPileAbstract
    {
        public override string AddItemLabel
        {
            get
            {
                return Attributes?["addItemLabel"].AsString("gespileslib:blockhelp-genericpile-addItem") ?? "gespileslib:blockhelp-genericpile-addItem";
            }
        }
        public override string RemoveItemLabel
        {
            get
            {
                return Attributes?["removeItemLabel"].AsString("gespileslib:blockhelp-genericpile-removeItem") ?? "gespileslib:blockhelp-genericpile-removeItem";
            }
        }
        public override string BulkAddLabel
        {
            get
            {
                return Attributes?["bulkAddLabel"].AsString("gespileslib:blockhelp-genericpile-bulkAdd") ?? "gespileslib:blockhelp-genericpile-bulkAdd";
            }
        }
        public override string BulkRemoveLabel
        {
            get
            {
                return Attributes?["bulkRemoveLabel"].AsString("gespileslib:blockhelp-genericpile-bulkRemove") ?? "gespileslib:blockhelp-genericpile-bulkRemove";
            }
        }
        public override int DefaultAddQuantity
        {
            get
            {
                return Attributes?["defaultAddQuantity"].AsInt(2) ?? 2;
            }
        }
    }
}