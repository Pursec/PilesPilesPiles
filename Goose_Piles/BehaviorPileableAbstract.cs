using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
namespace Goose_Vintage_BlockPiles
{
    public abstract class BehaviorPileableAbstract : CollectibleBehavior
    {
        protected abstract AssetLocation PileBlockCode { get; }
        public virtual bool IsPileable => true;


        public BehaviorPileableAbstract(CollectibleObject collObj) : base(collObj)
        {
            this.collObj = collObj;
        }
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
        }

        //public virtual void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            ICoreAPI API = byEntity.Api;
            if (!IsPileable)
            {
                collObj.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }

            if (blockSel == null || byEntity?.World == null || !byEntity.Controls.ShiftKey)
            {
                collObj.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }

            BlockPos onBlockPos = blockSel.Position;

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            if (byPlayer == null) return;

            if (!byEntity.World.Claims.TryAccess(byPlayer, onBlockPos, EnumBlockAccessFlags.BuildOrBreak))
            {
                API.World.BlockAccessor.MarkBlockEntityDirty(onBlockPos.AddCopy(blockSel.Face));
                API.World.BlockAccessor.MarkBlockDirty(onBlockPos.AddCopy(blockSel.Face));
                return;
            }

            var atblock = byEntity.World.BlockAccessor.GetBlock(onBlockPos);
            BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(onBlockPos);

            if (be is BlockEntityLabeledChest || be is BlockEntitySignPost || be is BlockEntitySign || be is BlockEntityBloomery || be is BlockEntityFirepit || be is BlockEntityForge || be is BlockEntityCrate || atblock.HasBehavior<BlockBehaviorJonasGasifier>()) return;

            if (be is IBlockEntityItemPile)
            {
                IBlockEntityItemPile pile = (IBlockEntityItemPile)be;
                if (pile.OnPlayerInteract(byPlayer))
                {
                    handHandling = EnumHandHandling.PreventDefaultAction;

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                    return;
                }
            }

            if (!byEntity.World.Claims.TryAccess(byPlayer, onBlockPos.AddCopy(blockSel.Face), EnumBlockAccessFlags.BuildOrBreak))
            {
                return;
            }


            be = byEntity.World.BlockAccessor.GetBlockEntity(onBlockPos.AddCopy(blockSel.Face));
            if (be is IBlockEntityItemPile)
            {
                IBlockEntityItemPile pile = (IBlockEntityItemPile)be;
                if (pile.OnPlayerInteract(byPlayer))
                {
                    handHandling = EnumHandHandling.PreventDefaultAction;

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                    return;
                }
            }

            Block block = byEntity.World.GetBlock(PileBlockCode);
            if (block == null) return;
            BlockPos pos = onBlockPos.Copy();
            if (byEntity.World.BlockAccessor.GetBlock(pos).Replaceable < 6000) pos.Add(blockSel.Face);

            bool ok = ((IBlockItemPile)block).Construct(slot, byEntity.World, pos, byPlayer);

            Cuboidf[] collisionBoxes = byEntity.World.BlockAccessor.GetBlock(pos).GetCollisionBoxes(byEntity.World.BlockAccessor, pos);

            if (collisionBoxes != null && collisionBoxes.Length > 0 && CollisionTester.AabbIntersect(collisionBoxes[0], pos.X, pos.Y, pos.Z, byPlayer.Entity.SelectionBox, byPlayer.Entity.SidedPos.XYZ))
            {
                byPlayer.Entity.SidedPos.Y += collisionBoxes[0].Y2 - (byPlayer.Entity.SidedPos.Y - (int)byPlayer.Entity.SidedPos.Y);
            }

            if (ok)
            {
                handHandling = EnumHandHandling.PreventDefaultAction;
                ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            }
            else
            {
                collObj.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
            }


        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            if (!IsPileable)
            {
                return collObj.GetHeldInteractionHelp(inSlot);
            }

            return new WorldInteraction[]
            {
                new WorldInteraction
                {
                    HotKeyCode = "shift",
                    ActionLangCode = "heldhelp-place",
                    MouseButton = EnumMouseButton.Right
                }
            }.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
        }


    }
}