using System;
using System.Drawing;
using System.Text;
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
    public abstract class BlockEntityPileAbstract : BlockEntityItemPile, ITexPositionSource
    {
        public ICoreClientAPI capi;
        public int Layers => inventory[0].StackSize == 1 ? 1 : inventory[0].StackSize / 2 ;
        public BlockEntityPileAbstract()
        {
        }
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.capi = api as ICoreClientAPI;
        }

        public TextureAtlasPosition this[string textureCode]
		{
			get
			{
                string itemcode = inventory[0].Itemstack.Collectible.Code.Path;
                
                int blockid = base.Block.BlockId;
                string blockCode = capi.World.BlockAccessor.GetBlock(Pos).ToString();
                /*
                Api.Logger.Log(EnumLogType.Debug, "Texture Code: " + textureCode);
                Api.Logger.Log(EnumLogType.Debug, "Block id: " + blockid + " block code: " + blockCode);
                Api.Logger.Log(EnumLogType.Debug, "texcode: " + itemcode);
                Api.Logger.Log(EnumLogType.Debug, "itemcode: " + itemcode);
                */
                return this.capi.BlockTextureAtlas.Positions[base.Block.Textures[textureCode].Baked.TextureSubId];
                //return this.capi.BlockTextureAtlas.Positions[base.Block.Textures["all"].Baked.TextureSubId];
			}
		}
        public void GetDecalMesh(ITexPositionSource decalTexSource, out MeshData meshdata)
        {
            int size = this.Layers * 2;

            Shape shape = this.capi.TesselatorManager.GetCachedShape(new AssetLocation("block/basic/layers/" + GameMath.Clamp(size, 2, 16) + "voxel"));
            this.capi.Tesselator.TesselateShape(this.BlockCode, shape, out meshdata, decalTexSource, null, 0, 0, 0, null, null);
        }

        public virtual bool OnPlayerInteract32(IPlayer byPlayer)
        {
            BlockPos blockPos = Pos.UpCopy();
            BlockEntityItemPile blockEntity = Api.World.BlockAccessor.GetBlockEntity(blockPos) as BlockEntityPileAbstract;
            if (blockEntity is BlockEntityPileAbstract)
            {
                return blockEntity.OnPlayerInteract(byPlayer);
            }

            bool shiftKey = byPlayer.Entity.Controls.ShiftKey;
            ItemSlot activeHotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            bool flag = activeHotbarSlot.Itemstack != null && activeHotbarSlot.Itemstack.Equals(Api.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes);
            if (shiftKey && !flag)
            {
                return false;
            }

            if (shiftKey && flag && OwnStackSize >= MaxStackSize)
            {
                Block block = Api.World.BlockAccessor.GetBlock(Pos);
                if (Api.World.BlockAccessor.GetBlock(blockPos).IsReplacableBy(block))
                {
                    if (Api.World is IServerWorldAccessor)
                    {
                        Api.World.BlockAccessor.SetBlock(block.Id, blockPos);
                        if (Api.World.BlockAccessor.GetBlockEntity(blockPos) is BlockEntityItemPile blockEntityItemPile)
                        {
                            blockEntityItemPile.TryPutItem(byPlayer);
                        }
                    }

                    return true;
                }

                return false;
            }

            lock (inventoryLock)
            {
                if (shiftKey)
                {
                    return TryPutItem(byPlayer);
                }

                return TryTakeItem(byPlayer);
            }
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            int size;
            if (mesher is EntityBlockFallingRenderer)
            {
                //Api.Logger.Debug("Tesselating stacksize: " + inventory[0]?.StackSize + " Falling: " + (mesher is EntityBlockFallingRenderer) + " Empty?: " + inventory[0]?.Empty.ToString());
                if (inventory[0].Empty) 
                {
                    InventoryGeneric otherinv2 = new InventoryGeneric(1, BlockCode, null, null, null);
                    EntityBlockFalling entityblock = Api.World.GetNearestEntity(Pos.ToVec3d().Add(0.5, 0.5, 0.5), 1, 1.5f) as EntityBlockFalling;

                    //otherinv2.FromTreeAttributes(entityblock.blockEntityAttributes.GetAttribute("inventory") as TreeAttribute);
                    //otherinv2.Api = Api;
                    //otherinv2.ResolveBlocksOrItems();
                    this.inventory.FromTreeAttributes(entityblock.blockEntityAttributes.GetAttribute("inventory") as TreeAttribute);
                    this.inventory.ResolveBlocksOrItems();
                    //Api.Logger.Debug("Inv: " + inventory[0].Itemstack.ToString());
                }

                //Api.Logger.Debug("Tesselation | Falling entity inv: " + otherinv2[0].Itemstack.ToString());
                //Api.Logger.Debug("Hello?");
                //Api.Logger.Debug("StackSize: " + otherinv2[0].Itemstack?.StackSize.ToString());

                //size = (otherinv2[0].StackSize == 1 ? 1 : otherinv2[0].StackSize / 2) * 2;
                size = this.Layers * 2;
                //Api.Logger.Debug("Size: " + size.ToString());
                
                Shape shape = capi.TesselatorManager.GetCachedShape(new AssetLocation("block/basic/layers/" + GameMath.Clamp(size, 2, 16) + "voxel"));
                MeshData meshdata;
                this.capi.Tesselator.TesselateShape(this.BlockCode, shape, out meshdata, this);

                mesher.AddMeshData(meshdata);
                return true;
            }


            lock (inventoryLock)
            {
                if (!inventory[0].Empty)
                {
                    //Api.Logger.Debug("Tesselating stacksize: " + inventory[0].StackSize + " Falling: " + (mesher is EntityBlockFallingRenderer));
                    size = this.Layers * 2;
                    //if (mesher is EntityBlockFallingRenderer) size = 2; // Haxy solution >.>

                    Shape shape = capi.TesselatorManager.GetCachedShape(new AssetLocation("block/basic/layers/" + GameMath.Clamp(size, 2, 16) + "voxel"));
                    MeshData meshdata;
                    this.capi.Tesselator.TesselateShape(this.BlockCode, shape, out meshdata, this);

                    mesher.AddMeshData(meshdata);
                }
            }

            return true;
        }
        
        public override bool OnPlayerInteract(IPlayer byPlayer)
        {
            //Api.Logger.Debug("Size before Interact: {0}", inventory[0].StackSize);
            bool ok = OnPlayerInteract32(byPlayer);
            //Api.Logger.Debug("Size after interact: {0}", inventory[0].StackSize);
            //MarkDirty();
            if (Api.Side == EnumAppSide.Server)
            {
                triggerTopMostPile(this);
            }
            //TriggerPileChanged();
            //Api.Logger.Debug("Pilesize after Trigger | {0}", inventory[0].StackSize);
            return ok;
        }

        public bool triggerTopMostPile(BlockEntityPileAbstract pile)
        {
            if (Api.World.BlockAccessor.GetBlockEntity(pile.Pos.UpCopy()) is BlockEntityPileAbstract uppile) return false;
            TriggerPileChanged();
            return true;
            /*
            if (Api.World.BlockAccessor.GetBlockEntity(pile.Pos.UpCopy()) is BlockEntityPileAbstract uppile)
            {
                return triggerTopMostPile(uppile);
            }
            else
            {
                Api.Logger.Debug("Triggering the topmost pile");
                pile.TriggerPileChanged();
                return true;
            }
            */
        }
        void TriggerPileChanged()
        {
            if (Api.Side != EnumAppSide.Server) return;

            int maxSteepness = 4;

            BlockPileAbstract belowpile = Api.World.BlockAccessor.GetBlock(Pos.DownCopy()) as BlockPileAbstract;
            int belowwlayers = belowpile == null ? 0 : belowpile.GetLayercount(Api.World, Pos.DownCopy());
            //int test = 0;
            foreach (var face in BlockFacing.HORIZONTALS)
            {
                BlockPos npos = Pos.AddCopy(face);
                Block nblock = Api.World.BlockAccessor.GetBlock(npos);
                BlockPileAbstract nblockpile = Api.World.BlockAccessor.GetBlock(npos) as BlockPileAbstract;

                // When should it collapse?
                int neighbourLayers = nblockpile?.GetLayercount(Api.World, npos) ?? 0;

                // 1. When our own layer count exceeds maxSteepness
                int nearbyCollapsibleCount = nblock.Replaceable > 6000 ? Layers - maxSteepness : 0;

                // 2. Nearby coal pile is maxsteepness smaller
                int nearbyToPileCollapsibleCount = nblockpile != null ? (Layers - neighbourLayers) - maxSteepness : 0;

                // 3. We are a 2 tall pile that is collapsible onto another pile
                BlockPileAbstract nbelowblockpile = Api.World.BlockAccessor.GetBlock(npos.DownCopy()) as BlockPileAbstract;
                int nbelowwlayers = nbelowblockpile == null ? 0 : nbelowblockpile.GetLayercount(Api.World, npos.DownCopy());
                int selfTallPileCollapsibleCount = belowpile != null && nbelowblockpile != null ? (Layers + belowwlayers - nbelowwlayers - maxSteepness) : 0;

                int collapsibleLayerCount = GameMath.Max(nearbyCollapsibleCount, nearbyToPileCollapsibleCount, selfTallPileCollapsibleCount);

                if (Api.World.Rand.NextDouble() < collapsibleLayerCount / (float)maxSteepness)
                {
                    int quantity = Math.Clamp(Api.World.Rand.Next(1, (MaxStackSize / 8) + 1), 1, inventory[0].StackSize);
                    //Api.Logger.Debug("Pile with {0} stackSize collapsing | quantity: {1}", inventory[0].StackSize, quantity);
                    //test += 1;
                    //Api.Logger.Debug("Test num: " + test);
                    if (TryPartialCollapse(npos.UpCopy(), quantity))
                    {
                        //Api.Logger.Debug("Sanity Check after collapse, pile size: {0}", inventory[0].StackSize);
                        return;
                    }

                }
            }
        }
        private bool TryPartialCollapse(BlockPos pos, int quantity)
        {
            if (inventory[0].Empty) return false;

            IWorldAccessor world = Api.World;
            //Api.Logger.Debug("BlockEntity at pos:" + world.BlockAccessor.GetBlockEntity(pos) + " Block: " + world.BlockAccessor.GetBlock(pos) + " Replaceable: " + world.BlockAccessor.GetBlock(pos).Replaceable);

            if (world.Side == EnumAppSide.Server)
            {
                ICoreServerAPI sapi = (world as IServerWorldAccessor).Api as ICoreServerAPI;
                if (!sapi.Server.Config.AllowFallingBlocks) return false;
            }
            //Api.Logger.Debug(String.Format("Blocktype: {0} | Replaceable: {1} | {1} >= 6000 : {2}", world.BlockAccessor.GetBlock(pos), world.BlockAccessor.GetBlock(pos).Replaceable, world.BlockAccessor.GetBlock(pos).Replaceable >= 6000));
            if (world.BlockAccessor.GetBlock(pos).Replaceable >= 6000 && (IsReplacableBeneath(world, pos) || IsReplacableBeneathAndSideways(world, pos)))
            {
                // Prevents duplication
                Entity entity = world.GetNearestEntity(pos.ToVec3d().Add(0.5, 0.5, 0.5), 1, 1.5f, (e) =>
                {
                    return e is EntityBlockFalling && ((EntityBlockFalling)e).initialPos.Equals(pos);
                });

                if (entity == null)
                {
                    lock (inventoryLock) {
                        int prevstacksize = inventory[0].StackSize;

                        //Api.Logger.Debug("Prev stack size: " + inventory[0].StackSize);

                        //inventory[0].Itemstack.StackSize = quantity;

                        //Api.Logger.Debug("Quanity: " + quantity);

                        TreeAttribute blockEntityAttributes = new TreeAttribute();
                        this.ToTreeAttributes(blockEntityAttributes);
                        InventoryGeneric otherinv = new InventoryGeneric(1, BlockCode, null, null, null);

                        otherinv.FromTreeAttributes(blockEntityAttributes.GetTreeAttribute("inventory"));
                        otherinv.Api = Api;
                        otherinv.ResolveBlocksOrItems();
                        otherinv[0].Itemstack.StackSize = quantity;

                        ((IBlockItemPile)world.GetBlock(Block.BlockId)).Construct(otherinv[0], world, pos, null);
                        world.BlockAccessor.MarkBlockDirty(pos);

                        //Api.Logger.Debug("What block did I just make?: " + world.BlockAccessor.GetBlock(pos).ToString());

                        EntityBlockFalling entityblock = new EntityBlockFalling(world.BlockAccessor.GetBlock(pos), world.BlockAccessor.GetBlockEntity(pos), pos, null, 1, true, 0.05f);
                        entityblock.DoRemoveBlock = true; // We want to split the pile, not remove it 
                        
                        world.SpawnEntity(entityblock);

                        //Api.Logger.Debug("Falling blocktype: " + entityblock.Block);

                        InventoryGeneric otherinv2 = new InventoryGeneric(1, BlockCode, null, null, null);
                        
                        otherinv2.FromTreeAttributes(entityblock.blockEntityAttributes.GetAttribute("inventory") as TreeAttribute);
                        otherinv2.Api = Api;
                        otherinv2.ResolveBlocksOrItems();
                        
                        //Api.Logger.Debug("Falling attribute tree:" + string.Join("\n", entityblock.blockEntityAttributes.Keys));
                        //Api.Logger.Debug("Falling Inventory: " + otherinv2[0].Itemstack.ToString());
                        

                        inventory[0].Itemstack.StackSize = prevstacksize - quantity;

                        Api.Logger.Debug("New Stack size: " + inventory[0].StackSize);

                        if (inventory[0].StackSize < 1)
                        {
                            Api.World.BlockAccessor.SetBlock(0, Pos);
                        }
                    }
                    //Api.Logger.Debug("Collapsed pile now size: {0}", inventory[0].StackSize);
                    return true;
                }
            }
            return false;
        }
        private bool IsReplacableBeneathAndSideways(IWorldAccessor world, BlockPos pos)
        {
            for (int i = 0; i < 4; i++)
            {
                BlockFacing facing = BlockFacing.HORIZONTALS[i];

                Block nBlock = world.BlockAccessor.GetBlockOrNull(pos.X + facing.Normali.X, pos.Y + facing.Normali.Y, pos.Z + facing.Normali.Z);
                Block nBBlock = world.BlockAccessor.GetBlockOrNull(pos.X + facing.Normali.X, pos.Y + facing.Normali.Y - 1, pos.Z + facing.Normali.Z);

                if (nBlock != null && nBBlock != null && nBlock.Replaceable >= 6000 && nBBlock.Replaceable >= 6000)
                {
                    return true;
                }
            }

            return false;
        }
        private bool IsReplacableBeneath(IWorldAccessor world, BlockPos pos)
        {
            Block bottomBlock = world.BlockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
            return (bottomBlock != null && bottomBlock.Replaceable > 6000);
        }
        public bool MergeWith(TreeAttribute blockEntityAttributes)
        {
            InventoryGeneric otherinv = new InventoryGeneric(1, BlockCode, null, null, null);
            otherinv.FromTreeAttributes(blockEntityAttributes.GetTreeAttribute("inventory"));
            //Api.Logger.Debug("OtherInv initial stackSize: {0}", otherinv[0].StackSize);
            //Api.Logger.Debug("MyInv initial stackSize: {0}", inventory[0].StackSize);
            otherinv.Api = Api;
            otherinv.ResolveBlocksOrItems();

            if (!inventory[0].Empty && otherinv[0].Itemstack.Equals(Api.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes))
            {
                int quantityToMove = Math.Min(otherinv[0].StackSize, Math.Max(0, MaxStackSize - inventory[0].StackSize));
                inventory[0].Itemstack.StackSize += quantityToMove;

                otherinv[0].TakeOut(quantityToMove);
                if (otherinv[0].StackSize > 0)
                {
                    BlockPos uppos = Pos.UpCopy();
                    Block upblock = Api.World.BlockAccessor.GetBlock(uppos);
                    if (upblock.Replaceable > 6000)
                    {
                        ((IBlockItemPile)Block).Construct(otherinv[0], Api.World, uppos, null);
                    }
                }
                MarkDirty(true);
                triggerTopMostPile(this);
            }
            else
            {
                Api.World.SpawnItemEntity(otherinv[0].Itemstack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
            Api.World.BlockAccessor.TriggerNeighbourBlockUpdate(this.Pos);
            return true;
        }
    }
}