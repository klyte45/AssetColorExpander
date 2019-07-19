# BuildingColorExpander

This mod allows to have more than 4 colors variations per building, if it allow to variate color.

## The XML Format

```xml
<bceConfig>
  <bce coloringMode="LIST" assetName="*">
    <color>F90011</color>
    <color>F90016</color>
  </bce>
  <bce coloringMode="PASTEL_HIGHER_SATURATION" pastelConfig="AVOID_NEUTRALS_REDS"  assetName="__ZONED__"/>
  <bce coloringMode="PASTEL_FULL_VIVID" pastelConfig="BLUES_ONLY"  assetName="CommercialBuildingAI"/>
  <bce coloringMode="PASTEL_ORIG" assetName="7894567987.WorkshopItem_Data"/>
</bceConfig>
```
## File name

It can be named by any name in AppData's folder, but must have the `.xml` extension.
Inside the asset folder to export to the Workshop, the file **must** be named as `k45_bce_data.xml`.


## Attributes
### `coloringMode`

It tells the mod which kind of coloring will be used to pick the color. There are 4 valid options:
+ `LIST`: Will get one color from the list of `<color>` tags inside this tag to paint the building. Ignored if there's no colors declared;
+ `PASTEL_ORIG`: Will use the default pastel color generator. All colors have a minimal difference between them. Note the `pastelConfig` attribute have **NO** effect when using this mode;
+ `PASTEL_HIGHER_SATURATION`: Same of above, but with a slight saturation difference that makes the colors seems vivid. 
+ `PASTEL_FULL_VIVID`: Same of above but with a stronger saturation add.

### `pastelConfig` (Only works with `coloringMode="PASTEL_HIGHER_SATURATION"` and `coloringMode="PASTEL_FULL_VIVID"`)

+ `ALLOW_ALL`: Nothing is filtered (Default value)

+ `REDS_ONLY`: Only red tone colors are generated
+ `GREENS_ONLY`: Only green tone colors are generated
+ `BLUES_ONLY`: Only blue tone colors are generated
+ `NEUTRAL_ONLY`: Only neutral (equalized) colors are generated

+ `AVOID_BLUES`: All but blue tone colors are generated
+ `AVOID_GREENS`: All but green tone colors are generated
+ `AVOID_REDS`: All but reds tone colors are generated
+ `AVOID_NEUTRALS`: All but neutral tone colors are generated

+ `AVOID_BLUES_GREENS`: All but blue tone and green tone colors are generated
+ `AVOID_REDS_BLUES`: All but blue tone and red tone colors are generated
+ `AVOID_REDS_GREENS`: All but red tone and green tone colors are generated
+ `AVOID_NEUTRALS_BLUES`: All but blue tone and neutral tone colors are generated
+ `AVOID_NEUTRALS_GREENS`: All but green tone and neutral tone colors are generated
+ `AVOID_NEUTRALS_REDS`: All but red tone and neutral tone colors are generated

### `assetName`

This field sets which assets will be affected by this. Currently supports 4 kinds of data, and are searched in this order in the game:

+ Asset full name: The full name of asset. Note the name of the uploaded asset is **different** from the local edition name;
+ Building AI class name: It'll affect all buildings that uses this class to behave in the game (Examples: TrainStationAI, CommercialBuildingAI, HarborAI...);
+ `__ZONED__` special keyword: All RICO buildings will be affected by this configuration; 

## Recommended xml file for fallback
```xml
<bceConfig>
<bce coloringMode="PASTEL_FULL_VIVID" pastelConfig="AVOID_NEUTRALS_REDS" assetName="__ZONED__" />
</bceConfig> 
```
Put this content at the `fallback.xml` file in the configuration folder to see this mod working. It's not mandatory. There's a button in the mod options config screen in mod menu that drives you to the mod configuration folder.

## Future

Feel free to add suggestions or branch this mod to pull request expansions. I'm now looking to GitHub with some frequency and I will update this mod as soon this get some new stuff.
