# XML Template Validation Checklist

## Pre-Release Validation Protocol

This checklist ensures XML template structures maintain correct GTA V compatibility before any code release.

### ✅ **Validation Steps**

#### 1. Reference File Verification
- [ ] Manually extracted reference files exist in `bin/Debug/net8.0/manually_extracted/dlc1.rpf/common/data/`
- [ ] Reference files are from known working GTA V DLC content
- [ ] Reference files parse as valid XML without errors
- [ ] Reference files contain expected container structures

#### 2. Template Structure Validation

**carcols.meta**
- [ ] Container: `<CVehicleModelInfoVarGlobal>` (NOT `CVehicleModColours`)
- [ ] Contains: `<Kits>` element for modification data
- [ ] Contains: `<Lights />` empty element (REQUIRED)
- [ ] Structure matches reference file exactly

**carvariations.meta**
- [ ] Container: `<CVehicleModelInfoVariation>` (NOT `CVehicleVariations`)
- [ ] Contains: `<variationData>` element for variation items
- [ ] Structure matches reference file exactly

**vehicles.meta**
- [ ] Container: `<CVehicleModelInfo__InitDataList>` (note double underscore)
- [ ] Contains: `<residentTxd>vehshare</residentTxd>`
- [ ] Contains: `<residentAnims />` empty element
- [ ] Contains: `<InitDatas>` element for vehicle data
- [ ] Contains: `<txdRelationships>` element for texture relationships
- [ ] Structure matches reference file exactly

**handling.meta**
- [ ] Container: `<CHandlingDataMgr>`
- [ ] Contains: `<HandlingData>` element for handling items
- [ ] Structure matches reference file exactly

**vehiclelayouts.meta**
- [ ] Container: `<CVehicleMetadataMgr>`
- [ ] Contains: `<VehicleLayoutInfos>` element
- [ ] Contains: `<VehicleEntryPointInfos>` element
- [ ] Contains: `<VehicleExtraPointsInfos>` element
- [ ] Contains: `<VehicleEntryPointAnimInfos>` element
- [ ] Contains: `<VehicleSeatInfos>` element
- [ ] Contains: `<VehicleSeatAnimInfos>` element
- [ ] Structure matches reference file exactly

#### 3. Code Implementation Validation
- [ ] Templates located in `XmlMerger.cs` in `XmlTemplates` dictionary
- [ ] All template keys match expected file names (lowercase with .meta extension)
- [ ] Template XML is well-formed and parseable
- [ ] No extra whitespace or formatting issues
- [ ] Inline documentation explains critical requirements

#### 4. Generation Testing
- [ ] Generate test DLC using updated templates
- [ ] Verify generated XML files parse without errors
- [ ] Compare generated structures with reference files
- [ ] Check for proper container nesting and element order
- [ ] Validate generated content against GTA V XML schema (if available)

#### 5. Game Compatibility Testing (Recommended)
- [ ] Install generated test DLC in GTA V test environment
- [ ] Verify game loads without crashes or errors
- [ ] Check that vehicle modifications appear correctly
- [ ] Test vehicle spawning and behavior
- [ ] Verify no console errors or warnings

### 🔧 **Fix Verification**

#### Recent Fixes Applied (2024-07-18)
- [x] **carcols.meta**: `CVehicleModColours` → `CVehicleModelInfoVarGlobal`
- [x] **carcols.meta**: Added required `<Lights />` element
- [x] **carvariations.meta**: `CVehicleVariations` → `CVehicleModelInfoVariation`
- [x] **All templates**: Validated against reference files from dlc1.rpf

#### Change Documentation
- [x] Updated inline code comments explaining critical requirements
- [x] Created comprehensive technical documentation
- [x] Documented validation methodology and evidence sources
- [x] Added change history and impact assessment

### 🚨 **Critical Warning Signs**

**Immediate Action Required If:**
- [ ] Any template container name doesn't match reference files
- [ ] Required elements are missing from templates
- [ ] Generated DLCs cause game crashes
- [ ] Vehicle modifications don't appear in game
- [ ] XML parsing errors during generation

**Investigation Required If:**
- [ ] New GTA V updates change XML format requirements
- [ ] Reference files become outdated or corrupted
- [ ] Generated content loads but behaves incorrectly
- [ ] Performance issues during XML generation

### 📋 **Maintenance Schedule**

**Monthly (or before major releases):**
- [ ] Run complete validation checklist
- [ ] Update reference files if new GTA V content available
- [ ] Test generated DLCs in current game version
- [ ] Review and update documentation

**Per GTA V Update:**
- [ ] Extract new reference files from updated game content
- [ ] Compare with existing templates for changes
- [ ] Update templates if format changes detected
- [ ] Re-validate entire system

**Per CodeWalker Release:**
- [ ] Complete validation checklist
- [ ] Document any template changes
- [ ] Include validation results in release notes
- [ ] Update version information in documentation

### 📁 **Reference File Management**

**Current Reference Location:**
```
CodeWalker.DLCMerger/bin/Debug/net8.0/manually_extracted/dlc1.rpf/common/data/
```

**Required Reference Files:**
- `carcols.meta` - Vehicle modification data
- `carvariations.meta` - Vehicle variation data  
- `vehicles.meta` - Vehicle model information
- `handling.meta` - Vehicle handling data
- `vehiclelayouts.meta` - Vehicle layout and animation data

**Reference File Validation:**
- [ ] Files are from known working GTA V DLC
- [ ] Files represent current game format version
- [ ] Files parse correctly as XML
- [ ] Files contain expected data structures

### 🎯 **Success Criteria**

**Template validation is successful when:**
1. All checklist items are verified ✅
2. Generated DLCs load without errors in GTA V
3. Vehicle content appears and functions correctly
4. No regression in existing functionality
5. Documentation accurately reflects current state

**Template validation must be repeated when:**
- Any template structure is modified
- GTA V receives major updates
- Reference files are updated or replaced
- Generation logic is changed
- User reports compatibility issues

---

**Document Version**: 1.0  
**Last Updated**: 2024-07-18  
**Next Review**: Before next major release  
**Maintained By**: CodeWalker Development Team