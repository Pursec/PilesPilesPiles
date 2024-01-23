using Goose_Vintage_BlockPiles;
using Vintagestory.API.Common;

namespace Goose_Vintage
{
    public class Goose_PilesModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockPile",typeof(BlockPile));
            api.RegisterBlockEntityClass("BlockEntityPile", typeof(BlockEntityPile));
            api.RegisterCollectibleBehaviorClass("BehaviorPileItem", typeof(BehaviorPileItem));
        }
    }
}
