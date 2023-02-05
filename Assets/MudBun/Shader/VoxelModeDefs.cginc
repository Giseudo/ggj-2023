/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_MODE_DEFS
#define MUDBUN_VOXEL_MODE_DEFS

#define kVoxelModeFlatCubes     (0)
#define kVoxelModeFacetedCubes  (1)
#define kVoxelModeFlatSpheres   (2)
#define kVoxelModeSmoothSpheres (3)
#define kVoxelModeCustom        (100)

#define kVoxelPaddingModeNone       (0)
#define kVoxelPaddingModeByDistance (1)
#define kVoxelPaddingModeFull       (2)

int rayTracedVoxelMode;
int rayTracedVoxelPaddingMode;
float rayTracedVoxelInternalPaddingDistance;
float rayTracedVoxelSizeFadeDistance;

#endif

