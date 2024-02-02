# Goose_Piles
A library mod for adding in Json extensibility for adding Coal-style Piles to Vintage Story.
This can be achieved entirely with a regular content mod for vintage story, comprising only of Json files.

This library adds the following relevant classes: `BlockPile, BlockEntityPile, & BehaviorPileItem`.

The process of implementing a new pile is as follows:

1. Create a blocktype json with the class: BlockPile and the entityClass: BlockEntityPile

2. (optional) Add attributes to define the pile rules, if not specified default values for all will be used instead.

- These values are as follows:

      "attributes":
      {
         "addItemLabel" : "String" or "LangKey",
         "removeItemLabel": "String" or "LangKey",
         "defaultAddQuantity": Integer (default: 2),
         "defaultTakeQuantity": Integer (default: 2),
         "bulkTakeQuantity": Integer (default: 4),
         "maxStackSize": Integer (default: 16),
         "interactSoundPath" : "domain:pathToSound",
         "allowUnstablePlacement": true/false (default: false)
      }

 3. Create a patch to add BehaviorPileITem to the behaviors array (or create one if not present) of desired pileable item
 - Specifcy behaivor property `"pileBlockCode":"domain:NEWPILEBLOCK"` where `NEWPILEBLOCK` is the code of the blocktype created above.

This is the entire process of creating a new pile.

Some notes about variant piles and texture, this library will support items to create piles by wildcard by changing the NEWPILEBLOCK code to the corresponding variant. An example of this would be grain piles including a variant `{type}`, a grainPile block can be created as `"domain:grainPile-{type}"` in the property. The grainPile blocktype should have a corresponding set of variants.

Piles also support normal simple block-texutre assignment, including specifying a texture perface/direction and altering the texture path by variant group.

Examples can be found with the default Rot, Salt, and Grain Piles added.
