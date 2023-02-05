/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_HASH_DEFS
#define MUDBUN_VOXEL_HASH_DEFS

#include "Math/Codec.cginc"

#define kNullVoxelHashKey (0)
#define kMaxVoxelHashCollisions (64) // (nodeHashTableSize)

struct VoxelHashEntry
{
  uint key;
  int iNode;
};

VoxelHashEntry init_voxel_hash_entry()
{
  VoxelHashEntry entry;
  entry.key = kNullVoxelHashKey;
  entry.iNode = -1;
  return entry;
}

#ifdef MUDBUN_IS_COMPUTE_SHADER
RWStructuredBuffer<VoxelHashEntry> nodeHashTable;
#else
StructuredBuffer<VoxelHashEntry> nodeHashTable;
#endif

int nodeHashTableSize;

uint top_node_key(int3 iCenter)
{
  iCenter = clamp(iCenter + 512, int3(0, 0, 0), int3(1023, 1023, 1023));
  return fnv_hash_concat(kFnvDefaultBasis, (uint(iCenter.x) << 21) | (uint(iCenter.y) << 11) | (uint(iCenter.z) << 1) | 1);
}

uint concat_node_key(uint key, uint3 coord)
{
  return fnv_hash_concat(key, (coord.x << 16) | (coord.y << 8) | coord.z);
}

int look_up_node(uint key)
{
  uint slot = key % nodeHashTableSize;

  int i = 0;
  while (i++ < kMaxVoxelHashCollisions)
  {
    uint entryKey = nodeHashTable[slot].key;

    if (entryKey == kNullVoxelHashKey)
      return -1;

    if (entryKey == key)
      return nodeHashTable[slot].iNode;

    slot = (slot + 1) % nodeHashTableSize;
  }

  return -1;
}

#endif

