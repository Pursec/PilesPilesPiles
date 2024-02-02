using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
namespace Goose_Vintage_BlockPiles
{
    public class BehaviorPileItem : BehaviorPileableAbstract
    {
        string pileBlockCodeProperty;
        //EnumModifierKey ModifierKey;
        public BehaviorPileItem(CollectibleObject collObj) : base(collObj)
        {
            this.collObj = collObj;
        }
        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            pileBlockCodeProperty = properties["pileBlockCode"].AsString();
            /*
            string key = properties["addAndCreationKey"].AsString() ?? "Shift";
            switch (key)
            {
                case "Shift":
                    ModifierKey = EnumModifierKey.SHIFT;
                    break;
                case "Alt":
                    ModifierKey = EnumModifierKey.ALT;
                    break;

            }
            */
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }
        protected override AssetLocation PileBlockCode => new AssetLocation(pileBlockCodeProperty);
        //protected override EnumModifierKey creationAndAddModifierKey => ModifierKey;
    }
}