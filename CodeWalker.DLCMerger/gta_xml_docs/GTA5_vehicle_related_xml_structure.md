**carvariations.meta**

```
<CVehicleModelInfoVariation>
    <variationData>
        <Item>…</Item>
        <Item>…</Item>
    </variationData>
</CVehicleModelInfoVariation>
```

---

**vehiclelayouts.meta**

```
<CVehicleMetadataMgr>
    <VehicleLayoutInfos>
        <Item type="CVehicleLayoutInfo"/>
        <Item type="CVehicleLayoutInfo"/>
    </VehicleLayoutInfos>
    <VehicleEntryPointInfos>
        <Item type="CVehicleEntryPointInfo"/>
        <Item type="CVehicleEntryPointInfo"/>
    </VehicleEntryPointInfos>
    <VehicleExtraPointsInfos>
        <Item type="CVehicleExtraPointsInfo"/>
        <Item type="CVehicleExtraPointsInfo"/>
    </VehicleExtraPointsInfos>
    <VehicleEntryPointAnimInfos>
        <Item type="CVehicleEntryPointAnimInfo"/>
        <Item type="CVehicleEntryPointAnimInfo"/>
    </VehicleEntryPointAnimInfos>
    <VehicleSeatInfos>
        <Item type="CVehicleSeatInfo"/>
        <Item type="CVehicleSeatInfo"/>
    </VehicleSeatInfos>
    <VehicleSeatAnimInfos>
        <Item type="CVehicleSeatAnimInfo"/>
        <Item type="CVehicleSeatAnimInfo"/>
    </VehicleSeatAnimInfos>
</CVehicleMetadataMgr>
```

---

**vehicles.meta**

```
<CVehicleModelInfo__InitDataList>
    <residentTxd>vehshare</residentTxd>
    <residentAnims/>
    <InitDatas>
        <Item>…</Item>
        <Item>…</Item>
    </InitDatas>
    <txdRelationships>
        <Item>…</Item>
        <Item>…</Item>
    </txdRelationships>
</CVehicleModelInfo__InitDataList>
```

---

**handling.meta**

```
<CHandlingDataMgr>
    <HandlingData>
        <Item type="CHandlingData">…</Item>
        <Item type="CHandlingData">…</Item>
    </HandlingData>
</CHandlingDataMgr>
```

*Each lowest‑level `<Item>` block represents one vehicle; keep the shown hierarchy unchanged when adding new ones.*


1. **XML header**

   ```
   <?xml version="1.0" encoding="UTF-8"?>
   ```

2. **Single root element** (top‑level wrapper for the whole file).

3. **One‑step‑down “container” tags** inside the root—each groups similar data (e.g., `<variationData>`, `<HandlingData>`, `<VehicleLayoutInfos>`).

4. **Repeated `<Item>` blocks** inside each container—the third tier.

   * Each `<Item>` represents one complete vehicle (or one complete record of the container’s type).
   * An `<Item>` and everything nested within it must stay together; treat it as an indivisible object when adding, removing, or editing vehicles.

The structure is always:

```
<?xml …?>
<Root>
    <Container>
        <Item>…</Item>
        <Item>…</Item>
    </Container>
    <!-- more containers, each with their own Item list -->
</Root>
```

Keep this hierarchy intact; only insert or delete whole `<Item>` blocks at the lowest level.
