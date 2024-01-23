using System;
using System.Text;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
namespace Goose_Vintage_BlockPiles
{
    public abstract class BlockPileAbstract : Block, IBlockItemPile
    {
        public abstract string AddItemLabel { get; }
		public abstract string RemoveItemLabel { get; }
		public abstract int DefaultAddQuantity { get; }
        private Cuboidf[][] CollisionBoxesbyFillLevel;
        public BlockPileAbstract()
        {
            this.CollisionBoxesbyFillLevel = new Cuboidf[9][];
            this.CollisionBoxesbyFillLevel[0] = new Cuboidf[0];

            for (int i = 1; i < this.CollisionBoxesbyFillLevel.Length; i++)
            {
                this.CollisionBoxesbyFillLevel[i] = new Cuboidf[] { new Cuboidf(0, 0, 0, 1, i * 0.125f, 1) };
            }
        }
        public override void OnLoaded(ICoreAPI api)
        {
            EnumAppSide side = api.Side;
            base.OnLoaded(api);
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			BlockEntityPileAbstract blockEntityPile = world.BlockAccessor.GetBlockEntity(selection.Position) as BlockEntityPileAbstract;
			if (blockEntityPile != null && blockEntityPile.inventory[0].Itemstack != null)
			{
				return new WorldInteraction[]
				{
					new WorldInteraction
					{
						ActionLangCode = this.AddItemLabel,
						MouseButton = EnumMouseButton.Right,
						HotKeyCode = "sneak",
						Itemstacks = new ItemStack[]
						{
							new ItemStack(blockEntityPile.inventory[0].Itemstack.Item, this.DefaultAddQuantity)
						}
					},
					new WorldInteraction
					{
						ActionLangCode = this.RemoveItemLabel,
						MouseButton = EnumMouseButton.Right,
						HotKeyCode = null
					}
				};
			}
			return new WorldInteraction[0];
		}
        public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
        {
            BlockEntityPileAbstract bep = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPileAbstract;

            if(bep == null)
            {
                base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
                return;
            }
            decalModelData.Clear();
            bep.GetDecalMesh(decalTexSource, out decalModelData);
        }

        public bool Construct(ItemSlot slot, IWorldAccessor world, BlockPos pos, IPlayer byPlayer)
        {
            if(!world.BlockAccessor.GetBlock(pos).IsReplacableBy(this))
            {
                return false;
            }
            if(!world.BlockAccessor.GetBlock(pos.DownCopy(1)).CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(1), BlockFacing.UP, null))
            {
                return false;
            }
            world.BlockAccessor.SetBlock(this.BlockId, pos);
            BlockEntity be = world.BlockAccessor.GetBlockEntity(pos);
            if (be is BlockEntityPileAbstract)
            {
                BlockEntityPileAbstract bep = (BlockEntityPileAbstract)be;
                if (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                {
                    bep.inventory[0].Itemstack = (ItemStack)slot.TakeOut(byPlayer?.Entity.Controls.CtrlKey == true ? bep.BulkTakeQuantity : bep.DefaultTakeQuantity);
                    slot.MarkDirty();
                }
                else
                {
                    bep.inventory[0].Itemstack = (ItemStack)slot.Itemstack.Clone();
                    bep.inventory[0].Itemstack.StackSize = Math.Min(bep.inventory[0].Itemstack.StackSize, bep.MaxStackSize);
                }
                bep.MarkDirty();
                world.BlockAccessor.MarkBlockDirty(pos);
                world.PlaySoundAt(bep.SoundLocation, pos.X, pos.Y, pos.Z, byPlayer, true);
                //api.Logger.Debug("Created {0} stacksize {1}pile of {2} Layers", bep.inventory[0].StackSize,bep.inventory[0].GetStackName(), bep.Layers);
            }
            return true;
        }
        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            BlockEntityPileAbstract bep = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPileAbstract;
            if(bep != null && bep.inventory[0].Itemstack != null)
            {
                return bep.inventory[0].Itemstack.Clone();
            }
            return base.OnPickBlock(world, pos);
        }
        
        public int GetLayercount(IWorldAccessor world, BlockPos pos)
        {
            return GetLayercount(world.BlockAccessor, pos);
        }

        public int GetLayercount(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BlockEntityPileAbstract bep = blockAccessor.GetBlockEntity(pos) as BlockEntityPileAbstract;
            if (bep != null && bep.inventory != null)
            {
                //api.Logger.Debug("BEP Inventory empty despite not being null? : {0}", bep.inventory.Empty);
                //api.Logger.Debug("BlockEntityPile layers: {0} | CBFillLevel length - 1: {1} | BEP Inv stacksize: {2} | PileItem: {3} ", bep.Layers, this.CollisionBoxesbyFillLevel.Length - 1, bep.inventory[0].StackSize, bep.inventory[0].Itemstack.GetName());
                int tempNum = Math.Min(bep.Layers, this.CollisionBoxesbyFillLevel.Length - 1);
                return tempNum > 0 ? tempNum : 1;
            }
            //api.Logger.Debug("BEP was null or its inventory was null");
            return 0;
        }
        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return this.CollisionBoxesbyFillLevel[GetLayercount(blockAccessor, pos)];
        }
        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return this.CollisionBoxesbyFillLevel[GetLayercount(blockAccessor, pos)];
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return new BlockDropItemStack[0];
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            // Handled by BlockEntityItemPile
            return new ItemStack[0];
        }
        public override bool OnFallOnto(IWorldAccessor world, BlockPos pos, Block block, TreeAttribute blockEntityAttributes)
        {
            if (block is BlockPileAbstract)
            {
                BlockEntityPileAbstract bep = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPileAbstract;
                if (bep != null)
                {
                    //api.Logger.Debug("Pile size {0} fell onto/this is my size before", bep.OwnStackSize);
                    return bep.MergeWith(blockEntityAttributes);
                }
            }

            return base.OnFallOnto(world, pos, block, blockEntityAttributes);
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (be is BlockEntityPileAbstract)
            {
                BlockEntityPileAbstract bep = (BlockEntityPileAbstract)be;
                return bep.OnPlayerInteract(byPlayer);
            }

            return false;
        }
        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
		{
			if (!world.BlockAccessor.GetBlock(pos.DownCopy(1)).CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(1), BlockFacing.UP, null))
            {
                //var thisBlock = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPile;
                //var downBlock = world.BlockAccessor.GetBlockEntity(pos.DownCopy()) as BlockEntityPile;
                //api.Logger.Debug("Breaking block size: {0}, downBlocks size: {1}", thisBlock.OwnStackSize, downBlock.OwnStackSize);
				world.BlockAccessor.BreakBlock(pos, null, 1f);
			}
		}
        public override bool CanAttachBlockAt(IBlockAccessor blockAccessor, Block block, BlockPos pos, BlockFacing blockFace, Cuboidi attachmentArea = null)
		{
			BlockEntityPileAbstract bep = blockAccessor.GetBlockEntity(pos) as BlockEntityPileAbstract;
			if (bep != null)
			{
				return bep.OwnStackSize == bep.MaxStackSize;
			}
			return base.CanAttachBlockAt(blockAccessor, block, pos, blockFace, attachmentArea);
		}
    }

}