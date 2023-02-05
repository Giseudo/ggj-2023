/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_DEFS
#define MUDBUN_VOXEL_DEFS

#ifdef MUDBUN_IS_COMPUTE_SHADER
bool enable2dMode;
#endif

bool forceAllBrushes;

struct VoxelNode
{
  float3 center;
  float sdfValue; // only for leaf nodes
  int iParent;
  int iBrushMask;
  uint key;
  int padding;
};

#ifdef MUDBUN_IS_COMPUTE_SHADER
RWStructuredBuffer<VoxelNode> nodePool;
#else
StructuredBuffer<VoxelNode> nodePool;
#endif

uint nodePoolSize;
#ifdef MUDBUN_IS_COMPUTE_SHADER
RWStructuredBuffer<int> aNumNodesAllocated; //(total, L0, L1, ..., voxels)
#else
StructuredBuffer<int> aNumNodesAllocated; //(total, L0, L1, ..., voxels)
#endif
uint chunkVoxelDensity;

int currentNodeDepth;
int currentNodeBranchingFactor;
int maxNodeDepth;
float currentNodeSize;

float voxelSize;
uint voxelTreeBranchingFactorsCompressed;
float4 voxelNodeSizes;

uint4 get_voxel_tree_branching_factors()
{
  uint4 factors = 1;
  unpack_8888(voxelTreeBranchingFactorsCompressed, factors.x, factors.y, factors.z, factors.w);
  return factors;
}

#endif

