using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Goose_Vintage_BlockPiles
{
	// Token: 0x0200000F RID: 15
	public class BlockEntityPile : BlockEntityPileAbstract
	{
        ILoadedSound ambientSound;
		public override string BlockCode
		{
			get
			{
				return "blockPile";
			}
		}
		public override int MaxStackSize
		{
			get
			{
				return this.Block.Attributes?["maxStackSize"].AsInt(16) ?? 16;
			}
		}
		public override int DefaultTakeQuantity
		{
			get
			{
                return this.Block.Attributes?["defaultTakeQuantity"].AsInt(2) ?? 2;
            }
        }

        public override int BulkTakeQuantity
        {
            get
            {
                return this.Block.Attributes?["bulkTakeQuantity"].AsInt(4) ?? 4;
            }
        }
        
        public override AssetLocation SoundLocation
		{
			get
			{
                return new AssetLocation(this.Block.Attributes?["interactSoundPath"].AsString("sounds/block/rock-break-pickaxe") ?? "sounds/block/rock-break-pickaxe");

			}
		}
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            capi = api as ICoreClientAPI;
        }
	}
}
