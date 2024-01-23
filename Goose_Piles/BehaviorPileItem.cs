using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
namespace Goose_Vintage_BlockPiles
{
    public class BehaviorPileItem : BehaviorPileableAbstract
    {
        string pileBlockCodeProperty;
        public BehaviorPileItem(CollectibleObject collObj) : base(collObj)
        {
            this.collObj = collObj;
        }
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            pileBlockCodeProperty = properties["pileBlockCode"].AsString();
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }
        protected override AssetLocation PileBlockCode => new AssetLocation(pileBlockCodeProperty);
    }
}