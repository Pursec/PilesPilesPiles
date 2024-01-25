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
        public override int DefaultAddQuantity
        {
            get
            {
                return Attributes?["defaultAddQuantity"].AsInt(2) ?? 2;
            }
        }
    }
}